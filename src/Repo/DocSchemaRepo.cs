using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

using Tlabs.Data.Serialize;
using Tlabs.Data.Entity;

namespace Tlabs.Data.Repo {

  ///<summary>>see cref="DocumentSchema"/> spcific repository.</summary>
  public interface IDocSchemaRepo : IRepo<DocumentSchema> {
    ///<summary>Get schema by <paramref name="typeId"/>.</summary>
    DocumentSchema GetByTypeId(string typeId);
    ///<summary>Try to get <paramref name="schema"/> by <paramref name="typeId"/>.</summary>
    bool TryGetByTypeId(string typeId, out DocumentSchema schema);
    ///<summary>Get schema def. streams by <paramref name="typeId"/>.</summary>
    SchemaDefinitionStreams StreamsByTypeId(string typeId, bool schemaStream= false);
    ///<summary>>Get schema by <paramref name="altName"/>.</summary>
    DocumentSchema GetByAltTypeName(string altName);
    ///<summary>Try to get <paramref name="schema"/> by <paramref name="altName"/>.</summary>
    bool TryGetByAltTypeName(string altName, out DocumentSchema schema);
    ///<summary>List of <see cref="DocumentSchema.TypeId"/>(s) optionally filterd by <paramref name="typeIdFilter"/>.</summary>
    IQueryable<string> FilteredTypeIdList(string typeIdFilter= null);
    ///<summary>Create schema from <paramref name="defStreams"/> (using <paramref name="docProcRepo"/> for schema syntax validation).</summary>
    DocumentSchema CreateFromStreams(SchemaDefinitionStreams defStreams, Processing.IDocProcessorRepo docProcRepo);
    ///<summary>Create schema from <paramref name="defStreams"/> using <paramref name="validateSchemaSyntax"/> callback for schema syntax validation.</summary>
    DocumentSchema CreateFromStreams(SchemaDefinitionStreams defStreams, Func<DocumentSchema, Processing.IDocSchemaProcessor> validateSchemaSyntax);

    ///<summary>Create schema from streams (using <paramref name="docProcRepo"/> for schema syntax validation).</summary>
    DocumentSchema CreateFromStreams(Processing.IDocProcessorRepo docProcRepo, Stream schemaStrm, Stream formStrm= null, Stream styleStrm= null, Stream calcModelStrm= null);

    ///<summary>Create schema from streams (using <paramref name="validateSchemaSyntax"/> callback for schema syntax validation).</summary>
    DocumentSchema CreateFromStreams(Func<DocumentSchema, Processing.IDocSchemaProcessor> validateSchemaSyntax,
                                     Stream schemaStrm, Stream formStrm= null, Stream styleStrm= null, Stream calcModelStrm= null);

    ///<summary>Returns form data for <paramref name="schemaId"/> of <paramref name="type"/>.</summary>
    ///<returns>Data <see cref="Stream"/>.</returns>
    Stream FormData(string schemaId, FormDataType type);
  }

  ///<summary>Form data type enum.</summary>
  public enum FormDataType {
    ///<summary>Markup form data.</summary>
    Markup,
    ///<summary>Style form data.</summary>
    Style
  }

  ///<summary>>see cref="DocumentSchema"/> spcific repository implementation.</summary>
  public class DocSchemaRepo : Intern.BaseRepo<DocumentSchema>, IDocSchemaRepo {
    static readonly ILogger<DocSchemaRepo> log= App.Logger<DocSchemaRepo>();
    readonly ISerializer<DocumentSchema> schemaSeri;

    ///<summary>Ctor from <paramref name="store"/>.</summary>
    public DocSchemaRepo(IDataStore store, ISerializer<DocumentSchema> schemaSeri) : base(store) {
      this.schemaSeri= schemaSeri;
    }

    ///<inheritdoc/>
    public DocumentSchema GetByTypeId(string typeId) {
      DocumentSchema loadSchema() { //helping Omnisharp...
        DocumentSchema.ParseTypeId(typeId, out var typeName, out var typeVers);
        var docSchema = AllUntracked.LoadRelated(store, s => s.Fields)
                                   .LoadRelated(store, s => s.Validations)
                                   .LoadRelated(Store, s => s.EvalReferences)
                                   .SingleOrDefault(s => s.TypeName == typeName && s.TypeVers == typeVers);
        if (null == docSchema) throw new DataEntityNotFoundException<DocumentSchema>(typeId);
        DocumentSchema.AltNameCache[typeId]= docSchema.TypeAltName;
        log.LogDebug("{id} schema loaded from store.", typeId);
        return docSchema;
      }
      return DocumentSchema.Cache[typeId, loadSchema];
    }

    ///<inheritdoc/>
    public bool TryGetByTypeId(string typeId, out DocumentSchema schema) {
      try {
        schema= GetByTypeId(typeId);
      }
      catch (DataEntityNotFoundException<DocumentSchema>) {
        schema= null;
        return false;
      }
      return true;
    }

    ///<inheritdoc/>
    public SchemaDefinitionStreams StreamsByTypeId(string typeId, bool schemaStream= false) {
      Stream stream= null;
      var schema= GetByTypeId(typeId);
      if (schemaStream) schemaSeri.WriteObj(stream= new MemoryStream(4096), schema);
      return new SchemaDefinitionStreams(schema, stream);
    }

    ///<inheritdoc/>
    public DocumentSchema GetByAltTypeName(string altName) {
      DocumentSchema docSchema= null;
      DocumentSchema.ParseTypeId(altName, out var typeAltName, out var typeVers);
      string loadSchema() {
        docSchema= AllUntracked.LoadRelated(store, s => s.Fields)
                               .LoadRelated(store, s => s.Validations)
                               .SingleOrDefault(s => s.TypeAltName == typeAltName && s.TypeVers == typeVers);
        if (null == docSchema) throw new DataEntityNotFoundException<DocumentSchema>(altName);
        DocumentSchema.Cache[docSchema.TypeId]= docSchema;
        return docSchema.TypeId;
      }
      var typeId= DocumentSchema.AltNameCache[typeAltName, loadSchema];

      if (docSchema == null) docSchema= DocumentSchema.Cache[typeId];

      return docSchema;
    }

    ///<inheritdoc/>
    public bool TryGetByAltTypeName(string altName, out DocumentSchema schema) {
      try {
        schema= GetByAltTypeName(altName);
      }
      catch (DataEntityNotFoundException<DocumentSchema>) {
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

    ///<inheritdoc/>
    public IQueryable<string> FilteredTypeIdList(string typeIdFilter= null) => filterByTypeId(typeIdFilter).Select(s => s.TypeId);

    ///<inheritdoc/>
    public DocumentSchema CreateFromStreams(Processing.IDocProcessorRepo docProcRepo, Stream schemaStrm, Stream formStrm= null, Stream styleStrm= null, Stream calcModelStrm= null) {
      using var defStreams = new SchemaDefinitionStreams {  //also cares about disposing streams
        Schema= schemaStrm,
        Form= formStrm,
        Style= styleStrm,
        CalcModel= calcModelStrm
      };
      return CreateFromStreams(defStreams, docProcRepo);
    }

    ///<inheritdoc/>
    public DocumentSchema CreateFromStreams(Func<DocumentSchema, Processing.IDocSchemaProcessor> validateSchemaSyntax,
                                            Stream schemaStrm, Stream formStrm= null, Stream styleStrm= null, Stream calcModelStrm= null)
    {
      using var defStreams = new SchemaDefinitionStreams {  //also cares about disposing streams
        Schema= schemaStrm,
        Form= formStrm,
        Style= styleStrm,
        CalcModel= calcModelStrm
      };
      return CreateFromStreams(defStreams, validateSchemaSyntax);
    }

    ///<inheritdoc/>
    public DocumentSchema CreateFromStreams(SchemaDefinitionStreams defStreams, Processing.IDocProcessorRepo docProcRepo) {
      var schema= loadFromStreams(defStreams);
      /* Check validation syntax and calc. model:
       */
      docProcRepo.CreateDocumentProcessor(schema);

      return upsertSchema(schema);
    }

    ///<inheritdoc/>
    public DocumentSchema CreateFromStreams(SchemaDefinitionStreams defStreams,  Func<DocumentSchema, Processing.IDocSchemaProcessor> validateSchemaSyntax)
    {
      var schema= loadFromStreams(defStreams);
      /* Check validation syntax and calc. model:
       */
      var docProcRepo= validateSchemaSyntax(schema);
      log.LogInformation("Creating schema '{sid}' with body type: {btype}", schema.TypeId, docProcRepo.BodyType.Name);
      return upsertSchema(schema);
    }

    private DocumentSchema upsertSchema(DocumentSchema schema) {
      log.LogDebug("Upserting schema: {s}", schema.TypeId);
      if (TryGetByTypeId(schema.TypeId, out var oldSchema))
        Delete(oldSchema);
      Insert(schema);
      Store.CommitChanges();
      return schema;
    }

    private DocumentSchema loadFromStreams(SchemaDefinitionStreams defStreams) {
      if (null == defStreams.Schema) throw new ArgumentException("Schema stream required.");
      var schema= schemaSeri.LoadObj(defStreams.Schema);

      using (var memStrm= new MemoryStream()) {
        if (null != defStreams.CalcModel) {
          memStrm.Position= 0;
          defStreams.CalcModel.CopyTo(memStrm);
          memStrm.SetLength(memStrm.Position);
          schema.CalcModelData= memStrm.ToArray();
        }
        if (null != defStreams.Form) {
          memStrm.Position= 0;
          defStreams.Form.CopyTo(memStrm);
          memStrm.SetLength(memStrm.Position);
          schema.FormData= memStrm.ToArray();
        }
        if (null != defStreams.Style) {
          memStrm.Position= 0;
          defStreams.Style.CopyTo(memStrm);
          memStrm.SetLength(memStrm.Position);
          schema.FormStyleData= memStrm.ToArray();
        }
      }
      return schema;
    }

    ///<inheritdoc/>
    public Stream FormData(string schemaId, FormDataType type) {
      var schema= GetByTypeId(schemaId);
      return type switch {
        FormDataType.Markup => new MemoryStream(schema.FormData),
        FormDataType.Style  => new MemoryStream(schema.FormStyleData),
        _                   => throw new ArgumentException($"Unsupported {nameof(FormDataType)}: {type}"),
      };
    }

    ///<inheritdoc/>
    public override DocumentSchema Get(params object[] keys) {
      var typeId= AllUntracked.Where(s => (int)keys[0] == s.Id).Select(s => s.TypeId).SingleOrDefault();
      if (null == typeId) throw new DataEntityNotFoundException<DocumentSchema>(keys[0]?.ToString());
      return GetByTypeId(typeId); //cached schema
    }

    ///<inheritdoc/>
    public override DocumentSchema Insert(DocumentSchema schema) {
      log.LogDebug("Inserting schema: {s}", schema.TypeId);
      schema= base.Insert(fixedSchema(schema));
      DocumentSchema.Cache[schema.TypeId]= schema;
      DocumentSchema.AltNameCache[schema.TypeAltName]= schema.TypeId;
      return schema;
    }

    ///<inheritdoc/>
    public override DocumentSchema Update(DocumentSchema schema) {
      log.LogDebug("Updating schema: {s}", schema.TypeId);
      schema= base.Update(fixedSchema(schema));
      DocumentSchema.Cache[schema.TypeId]= schema;
      DocumentSchema.AltNameCache[schema.TypeAltName]= schema.TypeId;
      return schema;
    }

    ///<inheritdoc/>
    public override void Delete(DocumentSchema schema) {
      log.LogDebug("Deleting schema: {s}", schema.TypeId);
      base.Delete(schema);
      DocumentSchema.Cache.Evict(schema.TypeId);
      DocumentSchema.AltNameCache.Evict(schema.TypeAltName);
    }

    ///<inheritdoc/>
    public override DocumentSchema Attach(DocumentSchema schema) {
      log.LogDebug("Attaching schema: {s}", schema.TypeId);
      schema= base.Attach(schema);
      DocumentSchema.Cache[schema.TypeId]= LoadRelated(schema);
      DocumentSchema.AltNameCache[schema.TypeAltName]= schema.TypeId;
      return schema;
    }

    ///<inheritdoc/>
    public override void Evict(DocumentSchema schema) {
      log.LogDebug("Evicting schema: {s}", schema.TypeId);
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