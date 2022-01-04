using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

using Tlabs.Misc;
using Tlabs.Dynamic;
using Tlabs.Data.Entity;
using Tlabs.Data.Repo;

namespace Tlabs.Data {

  /// <summary>Factory that generates a dymanic class for a document schema</summary>
  public class DocumentClassFactory : IDocumentClassFactory {

    /// <summary>Base class of generated dynamic class</summary>
    protected virtual Type baseType => typeof(object);

    private static readonly BasicCache<string, Type> cache= new BasicCache<string, Type>();

    ///<inheritdoc/>
    public Type CreateBodyType(DocumentSchema schema) => cache[schema.TypeId]= createType(schema);

    ///<inheritdoc/>
    public Type GetBodyType(string typeId) {
      /* Cache types for performance and more important
       * to avoid multiple dynamic Schema Type creations (causing memory leaks...).
       */
      return cache[typeId, () => {
        DocumentSchema schema= null;
        App.WithServiceScope(prov => {
          var schemaRepo= prov.GetService<IDocSchemaRepo>();
          schema= schemaRepo.GetByTypeId(typeId);
        });
        return createType(schema);
      }];
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

    private Type createType(DocumentSchema schema) {
      if (0 == schema.Fields?.Count) throw new InvalidOperationException("Unable to create DocBody from empty fields list");

      var dynProps= schema.Fields.Select(fld => new DynamicProperty(fld.Name, fld.Type, GetAttributes(fld))).ToList();
      string bdyTypeName= $"{schema.TypeId}-{DateTime.UtcNow.Ticks}";     //unique type name to avoid caching in DynamicClassFactory
      return DynamicClassFactory.CreateType(dynProps, baseType, bdyTypeName);
    }

  }
}