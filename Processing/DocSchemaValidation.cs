using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

using Microsoft.Extensions.Logging;

using Tlabs.Data.Entity;

namespace Tlabs.Data.Processing {
  using ValidatorFunc= Func<object /*, Insuree, ISource*/, bool>;    //***TODO: Need to develop a more flexible/configurable parameter list!

  /// <summary><see cref="DocumentSchema"/> processor.</summary>
  public partial class DocSchemaProcessor {
    class CompiledValidation {
      private DocumentSchema.ValidationRule rule;
      public DocumentSchema.ValidationRule Rule { get => rule; }
      public readonly ValidatorFunc IsValid;

      public CompiledValidation(DocSchemaProcessor proc, DocumentSchema.ValidationRule rule) {
        this.rule= rule;
        var validExp= parsedValidation(proc.typedValidationParms);
        var callParms= validExp.Parameters.Select((par, l) => l == 0 ? Expression.Parameter(typeof(object)) : par).ToArray();
        var convertedParms= validExp.Parameters.Select((par, l) => l == 0 ? Expression.Convert(callParms[0], proc.BodyType) : (Expression)par).ToArray();
        var delLambda= Expression.Lambda<ValidatorFunc>(Expression.Invoke(validExp, convertedParms), callParms);
        DocSchemaProcessor.Log.LogTrace("{sid}.{vrule}: '{code}' compiled into: '{lambda}'", proc.Sid, rule.Key, rule.Code, delLambda);
        IsValid= delLambda.Compile();
      }

      private LambdaExpression parsedValidation(ParameterExpression[] typedValidationParms) {
        if (string.IsNullOrEmpty(rule.Code)) throw new InvalidOperationException(ruleMsg("Unspecified rule code"));
        if (rule.Code.Length <= 2 || '{' != rule.Code[0] || '}' != rule.Code[rule.Code.Length-1]) throw new InvalidOperationException(ruleMsg("Invalid rule code"));
        try {
          var ruleCode= rule.Code.Substring(1, rule.Code.Length-2); //remove brackets: { Code }
          return DynamicExpressionParser.ParseLambda(false, typedValidationParms, typeof(bool), ruleCode, Formula.Function.Library);
        }
        catch (System.Linq.Dynamic.Core.Exceptions.ParseException e) {
          throw new CodeSyntaxException(ruleMsg($"Rule syntax error: {e.Message}", e));
        }
      }

      private string ruleMsg(string msg, System.Linq.Dynamic.Core.Exceptions.ParseException e = null) {
        var pos= null != e ? $"-pos:{e.Position}" : "";
        return $"{msg} @{rule.Key??""}{pos} ({rule.Description??""})";
      }
    }

    /// <summary>Validation exception.</summary>
    public class ValidationException : GeneralException {
      /// <summary>Rule for which validation failed.</summary>
      public readonly DocumentSchema.ValidationRule Rule;
      /// <summary>Default ctor</summary>
      public ValidationException(DocumentSchema.ValidationRule rule, Exception e) : base(e.Message, e) {
        this.Rule= rule;
      }

    }

  }


}