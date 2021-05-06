
using Tlabs.Misc;
using Tlabs.Data.Entity;

namespace Tlabs.Data.Processing.Intern {

  ///<summary>Repository of <see cref="IDocSchemaProcessor"/>.</summary>
  public class DocProcessorRepo : Intern.AbstractDocProcRepo {
    private static readonly BasicCache<string, DocSchemaProcessor> schemaCache= new BasicCache<string, DocSchemaProcessor>();

    ///<summary>Ctor from services.</summary>
    public DocProcessorRepo(Repo.IDocSchemaRepo schemaRepo,
                            IDocumentClassFactory docClassFactory,
                            Serialize.IDynamicSerializer docSeri,
                            SchemaCtxDescriptorResolver ctxDescResolver) : base(schemaRepo, docClassFactory, docSeri, ctxDescResolver)
    { }

    ///<inherit/>
    protected override IDocSchemaProcessor createProcessor(ICompiledDocSchema compSchema)
      => new DocSchemaProcessor(compSchema, docSeri);
  }

}