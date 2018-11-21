
using Tlabs.Misc;
using Tlabs.Data.Entity;

namespace Tlabs.Data.Processing {

  ///<summary>Repository of <see cref="DocSchemaProcessor"/>.</summary>
  public class DocProcessorRepo : Intern.AbstractDocProcRepo<DocSchemaProcessor> {
    private static readonly BasicCache<string, DocSchemaProcessor> schemaCache= new BasicCache<string, DocSchemaProcessor>();

    ///<summary>Ctor from services.</summary>
    public DocProcessorRepo(Repo.IDocSchemaRepo schemaRepo,
                            IDocumentClassFactory docClassFactory,
                            Serialize.IDynamicSerializer docSeri) : base(schemaRepo, docClassFactory, docSeri)
    { }

    ///<inherit/>
    protected override DocSchemaProcessor createProcessor(DocumentSchema schema) => new DocSchemaProcessor(schema, docClassFactory, docSeri);
  }


}