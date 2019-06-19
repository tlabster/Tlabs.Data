using System;
using Microsoft.Extensions.Logging;

using Tlabs.Misc;
using Tlabs.Sync;
using Tlabs.Data.Entity;
using Tlabs.Data.Repo;

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
    private static readonly BasicCache<ProcessorKey, IDocSchemaProcessor> schemaCache= new BasicCache<ProcessorKey, IDocSchemaProcessor>();

    ///<summary>Ctor from services.</summary>
    protected AbstractDocProcRepo(Repo.IDocSchemaRepo schemaRepo,
                                  IDocumentClassFactory docClassFactory,
                                  Serialize.IDynamicSerializer docSeri) {
      this.schemaRepo= schemaRepo;
      this.docClassFactory= docClassFactory;
      this.docSeri= docSeri;
    }

    ///<inherit/>
    public IDocSchemaRepo SchemaRepo => schemaRepo;

    ///<inherit/>
    public IDocSchemaProcessor GetDocumentProcessorBySid<TDoc, TVx, TCx>(string sid, TVx vx, TCx cx)
      where TDoc : Entity.Intern.BaseDocument<TDoc>
      where TVx : class, IExpressionCtx
      where TCx : class, IExpressionCtx
    {
      if (null == sid) throw new ArgumentNullException(nameof(sid));
      Func<IDocSchemaProcessor> loadSchemaProc= () => {  //helping Omnisharp...
        var docSchema= schemaRepo.GetByTypeId(sid);
        return this.createProcessor(docSchema, vx, cx);
      };
      return schemaCache[new ProcessorKey(sid, typeof(TVx), typeof(TCx)), loadSchemaProc];
    }

    ///<inherit/>
    public IDocSchemaProcessor GetDocumentProcessorBySid<TDoc>(string sid) where TDoc : Entity.Intern.BaseDocument<TDoc> {
      return GetDocumentProcessorBySid<TDoc, DefaultExpressionContext, DefaultExpressionContext>(sid, null, null);
    }

    ///<inherit/>
    IDocSchemaProcessor IDocProcessorRepo.GetDocumentProcessorByAltName<TDoc, TVx, TCx>(string altName, TVx vx, TCx cx) {
      if (null == altName) throw new ArgumentNullException(nameof(altName));
      return GetDocumentProcessorBySid<TDoc, TVx, TCx>(schemaRepo.GetByAltTypeName(altName).TypeId, vx, cx);
    }

    ///<inherit/>
    public IDocSchemaProcessor GetDocumentProcessorByAltName<TDoc>(string altName) where TDoc : Entity.Intern.BaseDocument<TDoc> {
      if (null == altName) throw new ArgumentNullException(nameof(altName));
      return GetDocumentProcessorBySid<TDoc>(schemaRepo.GetByAltTypeName(altName).TypeId);
    }

    ///<inherit/>
    IDocSchemaProcessor IDocProcessorRepo.GetDocumentProcessor<TDoc, TVx, TCx>(TDoc doc, TVx vx, TCx cx) {
      if (null == doc) throw new ArgumentNullException(nameof(doc));
      return GetDocumentProcessorBySid<TDoc, TVx, TCx>(doc.Sid, vx, cx);
    }

    ///<inherit/>
    public IDocSchemaProcessor GetDocumentProcessor<TDoc>(TDoc doc) where TDoc : Entity.Intern.BaseDocument<TDoc> {
      if (null == doc) throw new ArgumentNullException(nameof(doc));
      return GetDocumentProcessorBySid<TDoc>(doc.Sid);
    }

    ///<inherit/>
    public object LoadDocumentBodyObject<TDoc>(TDoc doc) where TDoc : Entity.Intern.BaseDocument<TDoc> => GetDocumentProcessor(doc).LoadBodyObject(doc);

    ///<inherit/>
    public object UpdateDocumentBodyObject<TDoc>(TDoc doc, object bodyObj) where TDoc : Entity.Intern.BaseDocument<TDoc> => GetDocumentProcessor(doc).UpdateBodyObject(doc, bodyObj);

    ///<inherit/>
    public IDocSchemaProcessor CreateDocumentProcessor<TDoc>(DocumentSchema schema) where TDoc : Entity.Intern.BaseDocument<TDoc> {
      return CreateDocumentProcessor<TDoc, DefaultExpressionContext, DefaultExpressionContext>(schema, null, null);
    }

    ///<inherit/>
    public IDocSchemaProcessor CreateDocumentProcessor<TDoc, TVx, TCx>(DocumentSchema schema, TVx vx, TCx cx)
      where TDoc : Entity.Intern.BaseDocument<TDoc>
      where TVx : class, IExpressionCtx
      where TCx : class, IExpressionCtx
    {
      if (null == schema) throw new ArgumentNullException(nameof(schema));
      return schemaCache[new ProcessorKey(schema.TypeId, typeof(TVx), typeof(TCx))]= createProcessor(schema, vx, cx);
    }

    ///<summary>Create a new <see cref="IDocSchemaProcessor"/> instance for <paramref name="schema"/>.</summary>
    protected abstract IDocSchemaProcessor createProcessor<TVx, TCx>(DocumentSchema schema, TVx vx, TCx cx) where TVx : class, IExpressionCtx where TCx : class, IExpressionCtx;

    internal struct ProcessorKey : IEquatable<ProcessorKey> {
      string sid;
      Type vxt;
      Type cxt;
      int hash;

      internal ProcessorKey(string sid, Type vxt, Type cxt) {
        this.sid= sid;
        this.vxt= vxt;
        this.cxt= cxt;
        this.hash= sid.GetHashCode() ^ vxt.GetHashCode() ^ cxt.GetHashCode();
      }

      public override int GetHashCode() => hash;

      public override bool Equals(object obj) {
        return obj is ProcessorKey
               && this.Equals((ProcessorKey)obj);
      }

      public bool Equals(ProcessorKey other) {
        return this.sid == other.sid
               && this.vxt == other.vxt
               && this.cxt == other.cxt;
      }
    }

  }


}