using System;
using System.Collections.Generic;

namespace Tlabs.Data.Processing {

  ///<summary>Expression context converter factorys.</summary>
  public delegate IDictionary<string, Type> CtxConverterFactory(Type bodyType);

  ///<summary>Interface of an expression contexts.</summary>
  public interface IExpressionCtx {
    //Note: This methods must not be properties for not getting confused with 'real' context properties!!!
    ///<summary>Return the body object.</summary>
    object GetBody();
  }

  /// <summary>Validation datat context type.</summary>
  public class DefaultExpressionContext : IExpressionCtx {
    /// <summary>Ctor from document body type.</summary>
    public DefaultExpressionContext(Type objType, object bdyObj= null) {
      this.d= bdyObj;
    }

    /// <summary>Default context converter.</summary>
    public static IDictionary<string, Type> GetContextConverter(Type objType) => new Dictionary<string, Type> {
      [nameof(d)]= objType
    };

    /// <summary>Document exposed as d.</summary>
    public object d { get; }

    ///<inherit/>
    public object GetBody() => d;
  }

}