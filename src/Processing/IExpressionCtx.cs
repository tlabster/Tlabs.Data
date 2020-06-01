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

    ///<summary>Property type map.</summary>
    IDictionary<string, Type> TypeMap(Type bdyType);
  }

  /// <summary>Default validation data context type.</summary>
  public class DefaultExpressionContext : IExpressionCtx {
    /// <summary>Ctor from document <paramref name="bdyObj"/>.</summary>
    public DefaultExpressionContext(object bdyObj= null) {
      this.d= bdyObj;
    }

    ///<inheritdoc/>
    public IDictionary<string, Type> TypeMap(Type bdyType) => new Dictionary<string, Type> { [nameof(d)]= bdyType };

    /// <summary>Document exposed as d.</summary>
    public object d { get; }

    ///<inherit/>
    public object GetBody() => d;
  }

  /// <summary>No data context type.</summary>
  public class NoExpressionContext : IExpressionCtx {
    /// <summary>Singleton instance.</summary>
    public static readonly NoExpressionContext Instance= new NoExpressionContext();
    ///<inheritdoc/>
    public IDictionary<string, Type> TypeMap(Type bdyType) => throw new NotImplementedException();
    ///<inherit/>
    public object GetBody() => throw new NotImplementedException();
  }


}