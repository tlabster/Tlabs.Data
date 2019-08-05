using System;
using System.IO;
using System.Linq;

using Tlabs.Data.Serialize;
using Tlabs.Data.Entity;

namespace Tlabs.Data.Repo {

  ///<summary>>see cref="DocumentSchema"/> spcific repository.</summary>
  public interface IDocSchemaRepo : IRepo<DocumentSchema> {
    ///<summary>Get schema by <paramref name="typeId"/>.</summary>
    DocumentSchema GetByTypeId(string typeId);
    ///<summary>Try to get <paramref name="schema"/> by <paramref name="typeId"/>.</summary>
    bool TryGetByTypeId(string typeId, out DocumentSchema schema);
    ///<summary>>Get schema by <paramref name="altName"/>.</summary>
    DocumentSchema GetByAltTypeName(string altName);
    ///<summary>Try to get <paramref name="schema"/> by <paramref name="altName"/>.</summary>
    bool TryGetByAltTypeName(string altName, out DocumentSchema schema);
    ///<summary>List of <see cref="DocumentSchema.TypeId"/>(s) optionally filterd by <paramref name="typeIdFilter"/>.</summary>
    IQueryable<string> FilteredTypeIdList(string typeIdFilter= null);
    ///<summary>Create schema from <paramref name="defStreams"/> (using <paramref name="docProcRepo"/> for schema syntax validation).</summary>
    DocumentSchema CreateFromStreams<TDoc>(SchemaDefinitionStreams defStreams, Processing.IDocProcessorRepo docProcRepo) where TDoc : Entity.Intern.BaseDocument<TDoc>;
    ///<summary>Create schema from <paramref name="defStreams"/> (using <paramref name="docProcRepo"/> for schema syntax validation).</summary>
    DocumentSchema CreateFromStreams<TDoc, TVx, TCx>(SchemaDefinitionStreams defStreams, Processing.IDocProcessorRepo docProcRepo, TVx vx, TCx cx)
      where TDoc : Entity.Intern.BaseDocument<TDoc>
      where TVx : class, Processing.IExpressionCtx
      where TCx : class, Processing.IExpressionCtx;

  }


  ///<summary>>see cref="DocumentSchema"/> spcific repository implementation.</summary>
  public class DocSchemaRepo : Intern.BaseRepo<DocumentSchema>, IDocSchemaRepo {
    ISerializer<DocumentSchema> schemaSeri;

    ///<summary>Ctor from <paramref name="store"/>.</summary>
    public DocSchemaRepo(IDataStore store, ISerializer<DocumentSchema> schemaSeri) : base(store) {
      this.schemaSeri= schemaSeri;
    }

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

    private IQueryable<DocumentSchema> filterByTypeId(string typeIdFilter) {
      var query= AllUntracked;
      string typeName= null;
      string typeVers= null;
      if (!string.IsNullOrEmpty(typeIdFilter)) {
        DocumentSchema.ParseTypeId(typeIdFilter, out typeName, out typeVers);
        if (0 == typeName.Length) typeName= null;
        if (0 == typeVers.Length) typeVers= null;
        query= query.Where(s => (typeName == null || s.TypeName.Contains(typeName)) && (typeVers == null || s.TypeVers == typeVers));
      }
      return query;
    }

    ///<inherit/>
    public IQueryable<string> FilteredTypeIdList(string typeIdFilter = null) => filterByTypeId(typeIdFilter).Select(s => s.TypeId);

    ///<inherit/>
    public DocumentSchema CreateFromStreams<TDoc>(SchemaDefinitionStreams defStreams, Processing.IDocProcessorRepo docProcRepo) where TDoc : Entity.Intern.BaseDocument<TDoc> {
      var schema= loadFromStreams(defStreams);
      /* Check validation syntax and calc. model:
       */
      docProcRepo.CreateDocumentProcessor<TDoc>(schema);
      DocumentSchema oldSchema;
      if (TryGetByTypeId(schema.TypeId, out oldSchema))
        Delete(oldSchema);
      Insert(schema);
      Store.CommitChanges();
      return schema;
    }

    ///<inherit/>
    public DocumentSchema CreateFromStreams<TDoc, TVx, TCx>(SchemaDefinitionStreams defStreams, Processing.IDocProcessorRepo docProcRepo, TVx vx, TCx cx)
      where TDoc : Entity.Intern.BaseDocument<TDoc>
      where TVx : class, Processing.IExpressionCtx
      where TCx : class, Processing.IExpressionCtx
    {
      var schema= loadFromStreams(defStreams);
      /* Check validation syntax and calc. model:
       */
      docProcRepo.CreateDocumentProcessor<TDoc, TVx, TCx>(schema, vx, cx);
      DocumentSchema oldSchema;
      if (TryGetByTypeId(schema.TypeId, out oldSchema))
        Delete(oldSchema);
      Insert(schema);
      Store.CommitChanges();
      return schema;
    }

    private DocumentSchema loadFromStreams(SchemaDefinitionStreams defStreams) {
      if (null == defStreams.Schema) throw new ArgumentException("Schema stream required.");
      var schema= schemaSeri.LoadObj(defStreams.Schema);

      using (var memStrm= new MemoryStream()) {
        memStrm.Position= 0;
        if (null != defStreams.CalcModel) {
          memStrm.Position= 0;
          defStreams.CalcModel.CopyTo(memStrm);
          schema.CalcModelData= memStrm.ToArray();
        }
        if (null != defStreams.Form) {
          memStrm.Position= 0;
          defStreams.Form.CopyTo(memStrm);
          schema.FormData= memStrm.ToArray();
        }
        if (null != defStreams.Style) {
          memStrm.Position= 0;
          defStreams.Style.CopyTo(memStrm);
          schema.FormStyleData= memStrm.ToArray();
        }
      }
      return schema;
    }

    ///<inherit/>
    public override DocumentSchema Get(params object[] keys) {
      var typeId= AllUntracked.Where(s => (int)keys[0] == s.Id).Select(s => s.TypeId).Single();
      return GetByTypeId(typeId); //cached schema
    }

    ///<inherit/>
    public override DocumentSchema Insert(DocumentSchema schema) {
      schema= base.Insert(fixedSchema(schema));
      DocumentSchema.Cache[schema.TypeId]= schema;
      DocumentSchema.AltNameCache[schema.TypeAltName]= schema.TypeId;
      return schema;
    }

    ///<inherit/>
    public override DocumentSchema Update(DocumentSchema schema) {
      schema= base.Update(fixedSchema(schema));
      DocumentSchema.Cache[schema.TypeId]= schema;
      DocumentSchema.AltNameCache[schema.TypeAltName]= schema.TypeId;
      return schema;
    }

    ///<inherit/>
    public override void Delete(DocumentSchema schema) {
      base.Delete(schema);
      DocumentSchema.Cache.Evict(schema.TypeId);
      DocumentSchema.AltNameCache.Evict(schema.TypeAltName);
    }

    ///<inherit/>
    public override DocumentSchema Attach(DocumentSchema schema) {
      schema= base.Attach(schema);
      DocumentSchema.Cache[schema.TypeId]= LoadRelated(schema);
      DocumentSchema.AltNameCache[schema.TypeAltName]= schema.TypeId;
      return schema;
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