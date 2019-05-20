using System;
using System.Collections.Generic;

namespace Tlabs.Data.Processing {

  ///<summary>Interface of an expression contexts.</summary>
  public interface IExpressionCtx {
    //Note: This methods must not be properties for not getting confused with 'real' context properties!!!
    ///<summary>Return a context converter dictionary.</summary>
    IDictionary<string, Type> GetContextConverter();
    ///<summary>Return the body object.</summary>
    object GetBody();
  }

  /// <summary>Validation datat context type.</summary>
  public class DefaultExpressionContext : IExpressionCtx {
    readonly IDictionary<string, Type> cxCnv;
    /// <summary>Ctor from document body type.</summary>
    public DefaultExpressionContext(Type objType, object bdyObj= null) {
      this.d= bdyObj;
      this.cxCnv= new Dictionary<string, Type> {
        ["d"]= objType
      };
    }
    /// <summary>Document exposed as d.</summary>
    public object d { get; }

    ///<inherit/>
    public IDictionary<string, Type> GetContextConverter() => cxCnv;
    ///<inherit/>
    public object GetBody() => d;
  }

}