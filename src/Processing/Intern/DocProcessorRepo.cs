
using Tlabs.Misc;
using Tlabs.Data.Entity;

namespace Tlabs.Data.Processing.Intern {

  ///<summary>Repository of <see cref="IDocSchemaProcessor"/>.</summary>
  public class DocProcessorRepo : Intern.AbstractDocProcRepo {
    ///<summary>Ctor from services.</summary>
    public DocProcessorRepo(Repo.IDocSchemaRepo schemaRepo,
                            IDocumentClassFactory docClassFactory,
                            Serialize.IDynamicSerializer docSeri,
                            SchemaCtxDescriptorResolver ctxDescResolver) : base(schemaRepo, docClassFactory, docSeri, ctxDescResolver)
    { }

    ///<inheritdoc/>
    protected override IDocSchemaProcessor createProcessor(ICompiledDocSchema compSchema)
      => new DocSchemaProcessor(compSchema, docSeri);
  }

}