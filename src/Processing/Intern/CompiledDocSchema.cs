using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

using Tlabs.Data.Entity;
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
    /// <summary>Evaluation context type index.</summary>
    IReadOnlyDictionary<string, Type> EvalTypeIdx { get; }

    ///<summary>Check <paramref name="body"/> object against the validation rules (with validation context <paramref name="cx"/>).</summary>
    ///<returns>true if valid. If invalid (false) the offending rule is set in <paramref name="rule"/>.</returns>
    bool CheckValidation(object body, ISchemaEvalContext cx, out DocumentSchema.ValidationRule rule);

    ///<summary>Compute all field formulas.</summary>
    void ComputeFieldFormulas(ISchemaEvalContext cx);
  }

  /// <summary>Generic compiled schema implementation.</summary>
  public class CompiledDocSchema<TCx> : ICompiledDocSchema where TCx : class, ISchemaEvalContext {
    static readonly ILogger<ICompiledDocSchema> log= App.Logger<ICompiledDocSchema>();
    readonly IEnumerable<CompiledValidation> compValidations;
    readonly IEnumerable<CompiledFieldFomula> compFieldFormulas;

    /// <summary><see cref="DocumentSchema"/>.</summary>
    public DocumentSchema Schema { get; }
    ///<inherit/>
    public string Sid => Schema.TypeId;
    ///<inherit/>
    public Type BodyType { get; }
    ///<inherit/>
    public DynamicAccessor BodyAccessor { get; }
    ///<inherit/>
    public IReadOnlyDictionary<string, Type> EvalTypeIdx { get; }

    private class CompiledValidation {
      public DocumentSchema.ValidationRule Rule { get; set; }
      public DynamicExpression<TCx, bool> Validator { get; set; }
    }

    private class CompiledFieldFomula {
      public DocumentSchema.Field Field { get; set; }
      public Action<object, TCx> Compute { get; set; }
    }

    /// <summary>Compile <paramref name="schema"/> for <paramref name="ctxDesc"/> using <paramref name="docClassFactory"/>.</summary>
    public static ICompiledDocSchema Compile(DocumentSchema schema, ISchemaCtxDescriptor ctxDesc, IDocumentClassFactory docClassFactory, bool newSchema) {
      var typeMap= (schema.EvalReferences ?? Enumerable.Empty<DocumentSchema.EvaluationRef>()).ToDictionary(r => r.PropName, r => docClassFactory.GetBodyType(r.ReferenceSid));
      var bodyType=   newSchema
                    ? docClassFactory.CreateBodyType(schema)
                    : docClassFactory.GetBodyType(schema.TypeId);
      typeMap[schema.EvalCtxSelfProp ?? nameof(DefaultSchemaEvalContext.d)]= bodyType;
      var compType= typeof(CompiledDocSchema<>).MakeGenericType(ctxDesc.EvalCtxTypeAccessor.TargetType);
      return (ICompiledDocSchema)Activator.CreateInstance(compType, schema, bodyType, typeMap);
    }

    /// <summary>Ctor form <paramref name="schema"/>, <paramref name="bodyType"/> and <paramref name="evalTypeIdx"/>.</summary>
    public CompiledDocSchema(DocumentSchema schema, Type bodyType, IReadOnlyDictionary<string, Type> evalTypeIdx) {
      log.LogDebug("Compiling schema: {sid}", schema.TypeId);
      if (null == (this.Schema= schema)) throw new ArgumentNullException(nameof(schema));
      if (null == (schema.Fields)) throw new ArgumentException(nameof(schema.Fields));
      var validations= schema.Validations;
      if (null == (validations)) throw new ArgumentException(nameof(schema.Validations));

      this.BodyType= bodyType;
      this.BodyAccessor= new DynamicAccessor(this.BodyType);
      this.EvalTypeIdx= evalTypeIdx;

      this.compValidations= compiledValidation(validations);
      this.compFieldFormulas= compiledFormulas(schema.Fields);
      log.LogDebug("Schema {sid} compiled into class: {cls}.", this.Sid, this.BodyType.Name);
    }

    ///<summary>Check <paramref name="body"/> object against the validation rules.</summary>
    ///<returns>true if valid. If invalid (false) the offending rule is set in <paramref name="rule"/>.</returns>
    public bool CheckValidation(object body, ISchemaEvalContext cx, out DocumentSchema.ValidationRule rule) {
      rule= null;
      var evCtx= cx as TCx;
      if (null == evCtx) throw (ArgumentException)(null == evCtx ? new ArgumentNullException(nameof(evCtx)) : new ArgumentException($"Can't cast {evCtx.GetType()} into {typeof(TCx)}"));
      foreach (var v in compValidations) try {
        rule= v.Rule.NewCopy<DocumentSchema.ValidationRule>();
        log.LogTrace("Evaluating rule {rule} for body type: {bt}", rule.Key, evCtx.GetBody().GetType());
        if (!v.Validator.Evaluate(evCtx))
          return false;
      }
      catch (Exception e) {
        log.LogDebug(e, "Rule {rule} evaluation failed: {msg}", rule.Key, e.Message);
        throw new DocumentValidationException(rule, e);
      }
      rule= null;
      return true;
    }

    ///<inherit/>
    public void ComputeFieldFormulas(ISchemaEvalContext cx) {
      IDictionary<string, object> bdyProps= null;
      var evCtx= cx as TCx;
      if (null == evCtx) throw (ArgumentException)(null == evCtx ? new ArgumentNullException(nameof(evCtx)) : new ArgumentException($"Can't cast {evCtx.GetType()} into {typeof(TCx)}"));
      if (log.IsEnabled(LogLevel.Debug)) {
        log.LogDebug("Computing {cnt} fields for body type: {bt}", compFieldFormulas.Count(), evCtx.GetBody().GetType());
        bdyProps= BodyAccessor.ToDictionary(evCtx.GetBody());
      }
      foreach(var frml in compFieldFormulas) {
        frml.Compute(evCtx.GetBody(), evCtx);
        if (log.IsEnabled(LogLevel.Trace)) log.LogTrace("{fld}[{calc}]]: {val}", frml.Field.Name, frml.Field.CalcFormula, bdyProps[frml.Field.Name]);
      }
    }

    private IEnumerable<CompiledValidation> compiledValidation(List<DocumentSchema.ValidationRule> validations) {
      var validRules= new CompiledValidation[validations.Count];
      var errors= new List<ExpressionSyntaxException>();

      for (var l = 0; l < validations.Count; ++l) try {
          var valid= validations[l];
          var exprCode= valid.Code;
          if (!exprCode.StartsWith("{") || !exprCode.EndsWith("}")) throw new ExpressionSyntaxException("Validation rule expession within {braces} expected.");
          exprCode= exprCode.Substring(1, exprCode.Length-2);
          validRules[l]= new CompiledValidation {
            Rule= validations[l],
            Validator= new DynamicExpression<TCx, bool>(exprCode, EvalTypeIdx, Tlabs.Dynamic.Misc.Function.Library)
          };
        }
        catch (ExpressionSyntaxException se) {
          errors.Add(se);
          if (errors.Count >= 10) break;
        }
      if (errors.Count > 0) throw new ExpressionSyntaxException(errors);  //error aggregate
      log.LogDebug("Compiled {n} validation(s).", validRules.Length);
      return validRules;
    }

    private IEnumerable<CompiledFieldFomula> compiledFormulas(List<DocumentSchema.Field> fields) {
      var compParams= new ParameterExpression[] {
        Expression.Parameter(typeof(object), "body"),
        Expression.Parameter(typeof(TCx), "cx")
      };
      var compFormulas= new List<CompiledFieldFomula>();
      var errors= new List<ExpressionSyntaxException>();

      foreach (var fld in fields) {
        if (string.IsNullOrEmpty(fld.CalcFormula)) continue;
        compFormulas.Add(new CompiledFieldFomula {
          Field= fld,
          Compute= compiledFrml(fld, compParams, EvalTypeIdx)
        });
      }
      log.LogDebug("Compiled {n} comp.field(s).", compFormulas.Count);
      return compFormulas;
    }

    private Action<object, TCx> compiledFrml(DocumentSchema.Field fld, ParameterExpression[] callParams, IReadOnlyDictionary<string, Type> cxCnv) {
      try {
        var ctxExprInfo= DynXHelper.BuildExpression(fld.CalcFormula, callParams[1], fld.Type, Tlabs.Dynamic.Misc.Function.Library, cxCnv);
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
      catch (Exception e) {
        log.LogDebug("Failed to compile (field: '{fld}') calc-foumula: '{fml}' (with body type: {bdy}), message: '{msg}'", fld.Name, fld.CalcFormula, BodyType.Name, e.Message);
        throw;
      }
    }
  }

}