using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

using Tlabs.Data.Entity;
using Tlabs.Data.Serialize;
using Tlabs.Dynamic;

namespace Tlabs.Data.Processing.Intern {

  /// <summary>Compiled schema.</summary>
  public interface ICompiledDocSchema {
    /// <summary><see cref= "DocumentSchema"/>.</summary>
    DocumentSchema Schema { get; }
    /// <summary>schema id.</summary>
    string Sid { get; }
    /// <summary>Effective schema body type.</summary>
    Type BodyType { get; }
    ///<summary><see cref="DynamicAccessor"/> to the body type.</summary>
    DynamicAccessor BodyAccessor { get; }
    ///<summary>Check <paramref name="body"/> object against the validation rules (with validation context <paramref name="vx"/>).</summary>
    ///<returns>true if valid. If invalid (false) the offending rule is set in <paramref name="rule"/>.</returns>
    bool CheckValidation(object body, object vx, out DocumentSchema.ValidationRule rule);

    ///<summary>Compute all field formulas.</summary>
    void ComputeFieldFormulas(IExpressionCtx cx);
  }

  /// <summary>Generic compiled schema implementation.</summary>
  public class CompiledDocSchema<TVx, TCx> : ICompiledDocSchema where TVx : class, IExpressionCtx  where TCx : class, IExpressionCtx {
    private static readonly ILogger<ICompiledDocSchema> log= App.Logger<ICompiledDocSchema>();
    readonly IEnumerable<CompiledValidation> compValidations;
    readonly IEnumerable<CompiledFieldFomula> compFieldFormulas;

    /// <summary><see cref="DocumentSchema"/>.</summary>
    public DocumentSchema Schema { get; }
    ///<inherit/>>
    public string Sid => Schema.TypeId;
    ///<inherit/>>
    public Type BodyType { get; }
    ///<inherit/>>
    public DynamicAccessor BodyAccessor { get; }

    private class CompiledValidation {
      public DocumentSchema.ValidationRule Rule { get; set; }
      public DynamicExpression<TVx, bool> Validator { get; set; }
    }

    private class CompiledFieldFomula {
      public DocumentSchema.Field Field { get; set; }
      public Action<object, TCx> Compute { get; set; }
    }

    /// <summary>Ctor form <paramref name="schema"/>, <paramref name="docClassFactory"/> and optional context templates.</summary>
    public CompiledDocSchema(DocumentSchema schema, IDocumentClassFactory docClassFactory, CtxConverterFactory valCfac, CtxConverterFactory evaCfac) {
      if (null == (this.Schema= schema)) throw new ArgumentNullException(nameof(schema));
      if (null == (schema.Fields)) throw new ArgumentException(nameof(schema.Fields));
      var validations= schema.Validations;
      if (null == (validations)) throw new ArgumentException(nameof(schema.Validations));

      this.BodyType= docClassFactory.GetBodyType(schema);
      this.BodyAccessor= new DynamicAccessor(this.BodyType);

      this.compValidations= compiledValidation(validations, valCfac);
      this.compFieldFormulas= compiledFormulas(schema.Fields, evaCfac);
    }

    ///<summary>Check <paramref name="body"/> object against the validation rules.</summary>
    ///<returns>true if valid. If invalid (false) the offending rule is set in <paramref name="rule"/>.</returns>
    public bool CheckValidation(object body, object vx, out DocumentSchema.ValidationRule rule) {
      rule= null;
      foreach (var v in compValidations) try {
          rule= v.Rule.NewCopy<DocumentSchema.ValidationRule>();
          if (!v.Validator.Evaluate((TVx)vx))
            return false;
        }
        catch (Exception e) { throw new DocumentValidationException(rule, e); }
      rule= null;
      return true;
    }

    ///<inherit/>
    public void ComputeFieldFormulas(IExpressionCtx icx) {
      var cx= (TCx)icx;
      foreach(var frml in compFieldFormulas)
        frml.Compute(cx.GetBody(), cx);
    }

    private IEnumerable<CompiledValidation> compiledValidation(List<DocumentSchema.ValidationRule> validations, CtxConverterFactory valCfac) {
      var vxCnv= valCfac?.Invoke(BodyType) ?? DefaultExpressionContext.GetContextConverter(this.BodyType);
      var validRules= new CompiledValidation[validations.Count];
      var errors= new List<ExpressionSyntaxException>();

      for (var l = 0; l < validations.Count; ++l) try {
          var valid= validations[l];
          var exprCode= valid.Code;
          if (!exprCode.StartsWith("{") || !exprCode.EndsWith("}")) throw new ExpressionSyntaxException("Validation rule expession within {braces} expected.");
          exprCode= exprCode.Substring(1, exprCode.Length-2);
          validRules[l]= new CompiledValidation {
            Rule= validations[l],
            Validator= new DynamicExpression<TVx, bool>(exprCode, vxCnv)
          };
        }
        catch (ExpressionSyntaxException se) {
          errors.Add(se);
          if (errors.Count >= 10) break;
        }
      if (errors.Count > 0) throw new ExpressionSyntaxException(errors);  //error aggregate
      return validRules;
    }

    private IEnumerable<CompiledFieldFomula> compiledFormulas(List<DocumentSchema.Field> fields, CtxConverterFactory evaCfac) {
      var cxCnv= evaCfac?.Invoke(BodyType) ?? DefaultExpressionContext.GetContextConverter(this.BodyType);
      var compParams= new ParameterExpression[] {
        // Expression.Parameter(this.BodyType),
        Expression.Parameter(typeof(object), "body"),
        Expression.Parameter(typeof(TCx), "cx")
      };
      var compFormulas= new List<CompiledFieldFomula>();
      var errors= new List<ExpressionSyntaxException>();

      foreach (var fld in fields) {
        if (string.IsNullOrEmpty(fld.CalcFormula)) continue;
        compFormulas.Add(new CompiledFieldFomula {
          Field= fld,
          Compute= compiledFrml(fld, compParams, cxCnv)
        });
      }
      return compFormulas;
    }

    private Action<object, TCx> compiledFrml(DocumentSchema.Field fld, ParameterExpression[] callParams, IDictionary<string, Type> cxCnv) {
      var ctxExprInfo= DynXHelper.BuildExpression(fld.CalcFormula, callParams[1], fld.Type, null, cxCnv);
      var asgnParms= callParams.Select((par, l) => l == 0 ? Expression.Parameter(BodyType, "bdy") : par).ToArray();
      var convParms= callParams.Select((par, l) => l == 0 ? Expression.Convert(par, BodyType) : (Expression)par).ToArray();

      var propAssign= Expression.Lambda(Expression.Assign(
                      Expression.MakeMemberAccess(asgnParms[0], BodyType.GetProperty(fld.Name, fld.Type)),
                      ctxExprInfo.Expression), asgnParms);
      var delType= typeof(Action<,>).MakeGenericType(BodyType, typeof(TCx));
      var lambda=  Expression.Lambda<Action<object, TCx>>(Expression.Invoke(propAssign, convParms), callParams);
      log.LogTrace("'{sid}.{fld}= {code}' compiled into: '{lambda}'", Sid, fld.Name, fld.CalcFormula, lambda);
      return (Action<object, TCx>)lambda.Compile();
    }
  }

}