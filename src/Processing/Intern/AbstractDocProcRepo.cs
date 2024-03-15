using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

using Tlabs.Misc;
using Tlabs.Data.Entity;
using Tlabs.Data.Repo;
using Tlabs.Data.Entity.Intern;

namespace Tlabs.Data.Processing.Intern {

  ///<summary>Abstract repository of T.</summary>
  ///<remarks>
  ///Implementations must provide a createProcessor() method to return an instance of <see cref="IDocSchemaProcessor"/>>.
  ///</remarks>
  public abstract class AbstractDocProcRepo : IDocProcessorRepo {
    ///<summary>Internal logger.</summary>
    protected static readonly ILogger<IDocProcessorRepo> log= App.Logger<IDocProcessorRepo>();
    ///<summary>Internal logger.</summary>
    protected Repo.IDocSchemaRepo schemaRepo;
    ///<summary>Document schema repo..</summary>
    protected IDocumentClassFactory docClassFactory;
    ///<summary>Document</summary>
    protected Serialize.IDynamicSerializer docSeri;
    readonly SchemaCtxDescriptorResolver ctxDescResolver;
    static readonly BasicCache<string, IDocSchemaProcessor> procCache= new();

    class Doc : Entity.Intern.BaseDocument<Doc> { }

    ///<summary>Ctor from services.</summary>
    protected AbstractDocProcRepo(Repo.IDocSchemaRepo schemaRepo,
                                  IDocumentClassFactory docClassFactory,
                                  Serialize.IDynamicSerializer docSeri,
                                  SchemaCtxDescriptorResolver ctxDescResolver) {
      this.schemaRepo= schemaRepo;
      this.docClassFactory= docClassFactory;
      this.docSeri= docSeri;
      this.ctxDescResolver= ctxDescResolver;
    }

    ///<inheritdoc/>
    public IDocSchemaRepo SchemaRepo => schemaRepo;

    ///<inheritdoc/>
    public IDocSchemaProcessor GetDocumentProcessorBySid(string? sid) {
      ArgumentNullException.ThrowIfNull(sid);

      IDocSchemaProcessor loadSchemaProc() {  //helping Omnisharp...
        var docSchema= schemaRepo.GetByTypeId(sid);
        ISchemaCtxDescriptor ctxDesc= ctxDescResolver.DescriptorByName(docSchema.EvalContextType);
        log.LogDebug("Caching new processsor for document with schema: {sid} and evalType: {type}", sid, ctxDesc.Name);
        return this.createProcessor(CompiledDocSchema<DefaultSchemaEvalContext>.Compile(docSchema, ctxDesc, docClassFactory, newSchema: false));
      }

      return procCache[sid, loadSchemaProc];
    }

    ///<inheritdoc/>
    public IDocSchemaProcessor GetDocumentProcessorByAltName(string altName) {
      return GetDocumentProcessorBySid(schemaRepo.GetByAltTypeName(altName).TypeId);
    }

    ///<inheritdoc/>
    public IDocSchemaProcessor GetDocumentProcessor<TDoc>(TDoc doc) where TDoc : Entity.Intern.BaseDocument<TDoc> {
      ArgumentNullException.ThrowIfNull(doc);
      return GetDocumentProcessorBySid(doc.Sid);
    }

    ///<inheritdoc/>
    public SchemaEvalCtxProcessor GetSchemaEvalCtxProcessor(string sid) {
      var docProc= GetDocumentProcessorBySid(sid);
      ISchemaCtxDescriptor ctxDesc= ctxDescResolver.DescriptorByName(docProc.Schema.EvalContextType);
      var docProcIndex= docProc.Schema.EvalReferences?.ToDictionary(r => r.PropName ?? throw new ArgumentNullException(nameof(r.PropName)),
                                                                    r => GetDocumentProcessorBySid(r.ReferenceSid))
                        ?? new();
      docProcIndex[docProc.Schema.EvalCtxSelfProp ?? nameof(DefaultSchemaEvalContext.d)]= docProc;
      return new SchemaEvalCtxProcessor(ctxDesc, docProcIndex);
    }

    ///<inheritdoc/>
    public object LoadDocumentBodyObject<TDoc>(TDoc doc) where TDoc : Entity.Intern.BaseDocument<TDoc> => GetDocumentProcessor(doc).LoadBodyObject(doc);

    ///<inheritdoc/>
    public object UpdateDocumentBodyObject<TDoc>(TDoc doc, object bodyObj) where TDoc : Entity.Intern.BaseDocument<TDoc> => GetDocumentProcessor(doc).UpdateBodyObject(doc, bodyObj);

    ///<inheritdoc/>
    public IDictionary<string, object?> LoadBodyProperties<TDoc>(TDoc doc) where TDoc : BaseDocument<TDoc> => GetDocumentProcessor(doc).LoadBodyProperties(doc);

    ///<inheritdoc/>
    public IDocSchemaProcessor CreateDocumentProcessor(DocumentSchema docSchema) {
      ArgumentNullException.ThrowIfNull(docSchema);
      var sid= docSchema.TypeId;
      ISchemaCtxDescriptor ctxDesc= ctxDescResolver.DescriptorByName(docSchema.EvalContextType);
      log.LogDebug("Creating new processsor for schema: {sid} and evalType: {type}", sid, ctxDesc.Name);
      return procCache[sid]= this.createProcessor(CompiledDocSchema<DefaultSchemaEvalContext>.Compile(docSchema, ctxDesc, docClassFactory, newSchema: true));
    }

    ///<summary>Create a new <see cref="IDocSchemaProcessor"/> instance for <paramref name="compSchema"/>.</summary>
    protected abstract IDocSchemaProcessor createProcessor(ICompiledDocSchema compSchema);

  }

}