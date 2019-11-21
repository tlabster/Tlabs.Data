using System;
using System.Collections.Generic;

using Tlabs.Data.Entity;
using Tlabs.Data.Entity.Intern;
using Tlabs.Dynamic;

namespace Tlabs.Data.Processing {

  /// <summary>Document validation exception.</summary>
  public class DocumentValidationException : GeneralException {
    /// <summary>Rule for which validation failed.</summary>
    public DocumentSchema.ValidationRule Rule { get; }
    /// <summary>Default ctor</summary>
    public DocumentValidationException(DocumentSchema.ValidationRule rule, Exception e) : base(e.Message, e) {
      this.Rule= rule;
    }
  }

  /// <summary><see cref="DocumentSchema"/> processor interface.</summary>
  public interface IDocSchemaProcessor {

    /// <summary><see cref="DocumentSchema"/>.</summary>
    DocumentSchema Schema { get; }
    /// <summary>Schema type Id.</summary>
    string Sid { get; }
    /// <summary><see cref="BaseDocument{TDoc}"/>'s Body type resulting from <see cref="DocumentSchema"/>.</summary>
    Type BodyType { get; }
    ///<summary><see cref="DynamicAccessor"/> to the body type.</summary>
    DynamicAccessor BodyAccessor { get; }

    /// <summary>Empty body object of Type: <see cref="BodyType"/>.</summary>
    object EmptyBody { get; }

    ///<summary>Return <paramref name="doc"/>'s Body as object (according to its <see cref="DocumentSchema"/>).</summary>
    object LoadBodyObject<TDoc>(TDoc doc) where TDoc : BaseDocument<TDoc>;

    ///<summary>Update <paramref name="doc"/>'s Body with <paramref name="bodyObj"/>.</summary>
    /// <remarks>
    /// By specifying a <paramref name="setupData"/> delegate the caller can provide a custom dictionary of data beeing imported into
    /// the CalcNgn model. (Defaults to a dictionary representing all public properties of the <paramref name="bodyObj"/>.)
    /// </remarks>
    object UpdateBodyObject<TDoc>(TDoc doc, object bodyObj, Func<object, IDictionary<string, object>> setupData= null, int bufSz = 10*1024) where TDoc : BaseDocument<TDoc>;

    ///<summary>Merge <paramref name="props"/> into <paramref name="doc"/>'s Body.</summary>
    ///<returns>Updated body properties dictionary.</returns>
    IDictionary<string, object> MergeBodyProperties<TDoc>(TDoc doc, IDictionary<string, object> props) where TDoc : BaseDocument<TDoc>;

    ///<summary>
    /// Check <paramref name="doc"/> against the validation rules (with validation context <paramref name="vx"/>)
    /// and applies the result to the document status properties.
    ///</summary>
    void ApplyValidation<TDoc>(TDoc doc, object vx, out object body) where TDoc : BaseDocument<TDoc>;


    ///<summary>Check <paramref name="doc"/> against the validation rules (with validation context <paramref name="vx"/>).</summary>
    ///<returns>true if valid. If invalid (false) the offending rule is set in <paramref name="rule"/>.</returns>
    bool CheckValidation<TDoc>(TDoc doc, object vx, out DocumentSchema.ValidationRule rule) where TDoc : BaseDocument<TDoc>;

    ///<summary>Check <paramref name="body"/> object against the validation rules.</summary>
    ///<returns>true if valid. If invalid (false) the offending rule is set in <paramref name="rule"/>.</returns>
    bool CheckValidation(object body, object vx, out DocumentSchema.ValidationRule rule);

    ///<summary>Evaluate computed schema fields.</summary>
    void EvaluateComputedFields<TCx>(TCx cx) where TCx : class, IExpressionCtx;
  }


}