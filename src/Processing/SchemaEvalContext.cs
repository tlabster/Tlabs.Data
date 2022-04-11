using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Tlabs.Dynamic;

namespace Tlabs.Data.Processing {

  ///<summary>Schema context descriptor resolver.</summary>
  public class SchemaCtxDescriptorResolver {
    readonly IReadOnlyDictionary<string, ISchemaCtxDescriptor> evalCtxMap;

    ///<summary>Ctor from <paramref name="ctxDescriptors"/>.</summary>
    public SchemaCtxDescriptorResolver(IEnumerable<ISchemaCtxDescriptor> ctxDescriptors) {
      this.evalCtxMap= ctxDescriptors.ToDictionary(d => d.Name);
    }

    ///<summary>Resolve context descriptor by name.</summary>
    public ISchemaCtxDescriptor DescriptorByName(string name) {
      ISchemaCtxDescriptor ctxDesc= DefaultSchemaCtxDescriptor.Instance;
      if (   !string.IsNullOrEmpty(name)
          && !evalCtxMap.TryGetValue(name, out ctxDesc)) throw new InvalidOperationException($"Unknown schema evaluation context type: '{name}'");
      return ctxDesc;
    }
  }

  ///<summary>Schema expression evaluatiion data context descriptor.</summary>
  public interface ISchemaCtxDescriptor {
    ///<summary>Context descriptor name.</summary>
    string Name { get; }

    ///<summary>Evaluation context type.</summary>
    DynamicAccessor EvalCtxTypeAccessor { get; }

  }
  ///<summary>Schema expression evaluatiion data context descriptor.</summary>
  public class SchemaCtxDescriptor : ISchemaCtxDescriptor {
    ///<summary>Ctor from <paramref name="name"/>, <paramref name="type"/>.</summary>
    public SchemaCtxDescriptor(string name, Type type) {
      if (string.IsNullOrEmpty(this.Name= name)) throw new ArgumentException(nameof(name));
      this.EvalCtxTypeAccessor= new DynamicAccessor(type);
    }

    ///<summary>Ctor from <paramref name="name"/>, <paramref name="typeAccessor"/>.</summary>
    public SchemaCtxDescriptor(string name, DynamicAccessor typeAccessor) {
      if (string.IsNullOrEmpty(this.Name= name)) throw new ArgumentException(nameof(name));
      if (null == (this.EvalCtxTypeAccessor= typeAccessor)) throw new ArgumentNullException(nameof(typeAccessor));
    }

    ///<inheritdoc/>
    public string Name { get; }
    ///<inheritdoc/>
    public DynamicAccessor EvalCtxTypeAccessor { get; }

    ///<summary>Context descriptor name.</summary>
    public override string ToString() {
      return Name ?? base.ToString();
    }
  }

  ///<summary>Schema evaluatiion context processor.</summary>
  public class SchemaEvalCtxProcessor : SchemaCtxDescriptor {
    ///<summary>Ctor from <paramref name="desc"/> and <paramref name="docProcIndex"/>.</summary>
    public SchemaEvalCtxProcessor(ISchemaCtxDescriptor desc, IReadOnlyDictionary<string, IDocSchemaProcessor> docProcIndex) : base(desc.Name, desc.EvalCtxTypeAccessor) {
      if (null == (DocProcessorIndex= docProcIndex)) throw new ArgumentNullException(nameof(docProcIndex));
    }
    ///<summary>Index of <see cref="IDocSchemaProcessor"/> by context property name.</summary>
    public IReadOnlyDictionary<string, IDocSchemaProcessor> DocProcessorIndex { get; }
  }


  ///<summary>Default schema context descriptor.</summary>
  public class DefaultSchemaCtxDescriptor : SchemaCtxDescriptor {
    ///<summary>Default instance.</summary>
    public static readonly DefaultSchemaCtxDescriptor Instance= new DefaultSchemaCtxDescriptor();
    private DefaultSchemaCtxDescriptor() : base("_DEFAULT", typeof(DefaultSchemaEvalContext)) { }
  }

  ///<summary>Schema evaluation context.</summary>
  public interface ISchemaEvalContext {
    //Note: This methods must not be a property for not getting confused with 'real' context properties!!!
    ///<summary>Return the body object of the evaluated context.</summary>
    object GetBody();
    /// <summary>Set body object of the evaluated context.</summary>
    void SetBody(object body);
  }

  ///<summary>Default schema evaluation context.</summary>
  public class DefaultSchemaEvalContext : ISchemaEvalContext {
    ///<summary>Default ctor.</summary>
    public DefaultSchemaEvalContext() { }
    ///<summary>Ctor from <paramref name="body"/>.</summary>
    public DefaultSchemaEvalContext(object body) => SetBody(body);

    //Note: This methods must not be a property for not getting confused with 'real' context properties!!!
    ///<summary>Return the body object.</summary>
    public object GetBody() => d;
    /// <summary>Set body of context.</summary>
    public void SetBody(object body) => d= body;
    /// <summary>Document (body) exposed as d (for self referencing).</summary>
    public object d { get; private set; }
  }

  /// <summary>No data context type.</summary>
  public class NoEvaluationContext : ISchemaEvalContext {
    /// <summary>Singleton instance.</summary>
    public static readonly NoEvaluationContext Instance= new NoEvaluationContext();
    ///<inherit/>
    public object GetBody() => throw new NotImplementedException();
    ///<inherit/>
    public void SetBody(object body) => throw new NotImplementedException();
  }

  /// <summary><see cref="ISchemaCtxDescriptor"/> registration extension.</summary>
  public static class EvalCtxSerciceExtension {
    /// <summary>Register <see cref="ISchemaCtxDescriptor"/> for <see cref="ISchemaEvalContext"/> of type <typeparamref name="T"/>.</summary>
    public static IServiceCollection AddSchemaCtxDescriptor<T>(this IServiceCollection services, string contextTypeName) where T : ISchemaEvalContext, new() {
      services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(ISchemaCtxDescriptor), new SchemaCtxDescriptor(contextTypeName, typeof(T))));
      return services;
    }
  }

}