using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Tlabs.Data.Entity;
using Tlabs.Data.Store;

namespace Tlabs.Data.Repo {

  ///<summary>>see cref="DocumentSchema"/> spcific repository.</summary>
  public interface IDocSchemaRepo : IRepo<DocumentSchema> {
    ///<summary>>Get schema by <paramref name="typeId"/>.</summary>
    DocumentSchema GetByTypeId(string typeId);
    ///<summary>>Try to get <paramref name="schema"/> by <paramref name="typeId"/>.</summary>
    bool TryGetByTypeId(string typeId, out DocumentSchema schema);
    ///<summary>>Get schema by <paramref name="altName"/>.</summary>
    DocumentSchema GetByAltTypeName(string altName);
    ///<summary>>Try to get <paramref name="schema"/> by <paramref name="altName"/>.</summary>
    bool TryGetByAltTypeName(string altName, out DocumentSchema schema);
  }

  ///<summary>>see cref="DocumentSchema"/> spcific repository implementation.</summary>
  public class DocSchemaRepo : Intern.BaseRepo<DocumentSchema>, IDocSchemaRepo {
    ///<summary>>Ctor from <paramref name="store"/>.</summary>
    public DocSchemaRepo(IDataStore store) : base(store) {}

    ///<inherit/>
    public DocumentSchema GetByTypeId(string typeId) {
      Func<DocumentSchema> loadSchema= () => { //helping Omnisharp...
        string typeName, typeVers;
        DocumentSchema.ParseTypeId(typeId, out typeName, out typeVers);
        var docSchema= AllUntracked.LoadRelated(store, s => s.Fields)
                                   .LoadRelated(store, s => s.Validations)
                                   .Single(s => s.TypeName == typeName && s.TypeVers == typeVers);
        DocumentSchema.AltNameCache[typeId]= docSchema.TypeAltName;
        return docSchema;
      };
      return DocumentSchema.Cache[typeId, loadSchema];
    }

    ///<inherit/>
    public bool TryGetByTypeId(string typeId, out DocumentSchema schema) {
      try {
        schema= GetByTypeId(typeId);
      }
      catch (InvalidOperationException) {
        schema= null;
        return false;
      }
      return true;
    }

    ///<inherit/>
    public DocumentSchema GetByAltTypeName(string altName) {
      DocumentSchema docSchema= null;
      string typeAltName, typeVers;
      DocumentSchema.ParseTypeId(altName, out typeAltName, out typeVers);
      Func<string> loadSchema= () => {
        docSchema= AllUntracked.LoadRelated(store, s => s.Fields)
                               .LoadRelated(store, s => s.Validations)
                               .Single(s => s.TypeAltName == typeAltName && s.TypeVers == typeVers);
        DocumentSchema.Cache[docSchema.TypeId]= docSchema;
        return docSchema.TypeId;
      };
      var typeId= DocumentSchema.AltNameCache[typeAltName, loadSchema];

      if (docSchema == null) docSchema= DocumentSchema.Cache[typeId];

      return docSchema;
    }

    ///<inherit/>
    public bool TryGetByAltTypeName(string altName, out DocumentSchema schema) {
      try {
        schema= GetByAltTypeName(altName);
      }
      catch(InvalidOperationException) {
        schema= null;
        return false;
      }
      return true;
    }

    ///<inherit/>
    public override DocumentSchema Get(params object[] keys) {
      var typeId= AllUntracked.Where(s => (int)keys[0] == s.Id).Select(s => s.TypeId).Single();
      return GetByTypeId(typeId); //cached schema
    }

    ///<inherit/>
    public override void Insert(DocumentSchema schema) {
      base.Insert(fixedSchema(schema));
      DocumentSchema.Cache[schema.TypeId]= schema;
      DocumentSchema.AltNameCache[schema.TypeAltName]= schema.TypeId;
    }

    ///<inherit/>
    public override void Update(DocumentSchema schema) {
      base.Update(fixedSchema(schema));
      DocumentSchema.Cache[schema.TypeId]= schema;
      DocumentSchema.AltNameCache[schema.TypeAltName]= schema.TypeId;
    }

    ///<inherit/>
    public override void Delete(DocumentSchema schema) {
      base.Delete(schema);
      DocumentSchema.Cache.Evict(schema.TypeId);
      DocumentSchema.AltNameCache.Evict(schema.TypeAltName);
    }

    ///<inherit/>
    public override void Attach(DocumentSchema schema) {
      base.Attach(schema);
      DocumentSchema.Cache[schema.TypeId]= LoadRelated(schema);
      DocumentSchema.AltNameCache[schema.TypeAltName]= schema.TypeId;
    }

    ///<inherit/>
    public override void Evict(DocumentSchema schema) {
      base.Evict(schema);
      DocumentSchema.Cache.Evict(schema.TypeId);
      DocumentSchema.AltNameCache.Evict(schema.TypeAltName);
    }

    private DocumentSchema LoadRelated(DocumentSchema schema) {
      if (null == schema.Fields) store.LoadExplicit(schema, s => s.Fields);
      if (null == schema.Validations) store.LoadExplicit(schema, s => s.Validations);
      return schema;
    }

    private DocumentSchema fixedSchema(DocumentSchema schema) {
      this.LoadRelated(schema);
      for (int l= 0, n= schema.Fields.Count; l < n; ++l) {
        var fld= schema.Fields[l];
        if (typeof(DocumentSchema.Field) != fld.GetType())
          schema.Fields[l]= new DocumentSchema.Field(fld);    //fix field type
      }
      return schema;
    }
  }
}