using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tlabs.Misc;
using Tlabs.Dynamic;
using Tlabs.Data.Event;
using Tlabs.Data.Entity;
using Tlabs.Data.Repo;

namespace Tlabs.Data {

  /// <summary>
  /// Factory that generates a dymanic class for a document schema
  /// </summary>
  public class DocumentClassFactory : IDocumentClassFactory {
    static DocumentClassFactory() {
      DataStoreEvent<DocumentSchema>.Updated+= resetCache;
      DataStoreEvent<DocumentSchema>.Deleted+= resetCache;
    }
    private static void resetCache(IEvent<DocumentSchema> obj) => Cache.Evict(obj.Entity.TypeId);

    private IDocSchemaRepo schemaRepo;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected virtual Type baseType => typeof(object);

    /// <summary>Ctor from <paramref name="schemaRepo"/>  </summary>
    public DocumentClassFactory(IDocSchemaRepo schemaRepo) {
      this.schemaRepo= schemaRepo;
    }

    private static readonly BasicCache<string, Type> Cache= new BasicCache<string, Type>();

    ///<inherit/>
    public Type GetBodyType(DocumentSchema schema) {
        return CreateType(schema);
    }

    ///<inherit/>
    public Type GetBodyType(string typeId) {
      /* Cache types for performance and more important
       * to avoid multiple dynamic Schema Type creations (causing memory leaks...).
       */
      return Cache[typeId, () => {
        var schema= schemaRepo.GetByTypeId(typeId);
        return CreateType(schema);
      }];
    }

    ///<inherit/>
    public object CreateEmptyBody(DocumentSchema documentSchema) {
      var bodyType = GetBodyType(documentSchema);
      var bodyTypeInfo= bodyType.GetTypeInfo();
      var obj= Activator.CreateInstance(bodyType);
      foreach (var strFld in documentSchema.Fields.Where(fld => fld.Type == typeof(string))) {
        var prop= bodyTypeInfo.GetDeclaredProperty(strFld.Name);
        prop.SetValue(obj, string.Empty);
      }
      return obj;
    }

    private IList<DynamicAttribute> GetAttributes(DocumentSchema.Field field) {
      if (field== null || field.ExtMappingInfo == null)
        return new List<DynamicAttribute>();

      string mappingInfo = field.ExtMappingInfo;

      var attributes = mappingInfo.Split('\n').Select(value => value.Split('=')).Where(elem => elem.Count() == 2);

      Dictionary<string,string> keyValuePairs = attributes.ToDictionary(pair => pair[0], pair => pair[1]);

      return BuildDynamicAttributes(field, keyValuePairs);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="field"></param>
    /// <param name="keyValuePairs"></param>
    /// <returns></returns>
    protected virtual IList<DynamicAttribute> BuildDynamicAttributes(DocumentSchema.Field field, Dictionary<string, string> keyValuePairs) {
      var attrs= new List<DynamicAttribute>();

      if (field.Sensitive) {
        attrs.Add(new DynamicAttribute(typeof(SensibleDataAttribute), Type.EmptyTypes));
      }

      return attrs;
    }

    private Type CreateType(DocumentSchema schema) {
      if (0 == schema.Fields?.Count) throw new InvalidOperationException("Unable to create DocBody from empty fields list");

      var dynProps= schema.Fields.Select(fld => new DynamicProperty(fld.Name, fld.Type, GetAttributes(fld))).ToList();
      string bdyTypeName= $"{schema.TypeId}-{DateTime.UtcNow.Ticks}";     //unique type name to avoid caching in DynamicClassFactory
      return DynamicClassFactory.CreateType(dynProps, baseType, bdyTypeName);
    }
  }
}