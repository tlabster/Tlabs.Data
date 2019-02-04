
using Tlabs.Misc;
using Tlabs.Data.Entity;

namespace Tlabs.Data.Processing.Intern {

  ///<summary>Repository of <see cref="IDocSchemaProcessor"/>.</summary>
  public class DocProcessorRepo : Intern.AbstractDocProcRepo {
    private static readonly BasicCache<string, DocSchemaProcessor> schemaCache= new BasicCache<string, DocSchemaProcessor>();

    ///<summary>Ctor from services.</summary>
    public DocProcessorRepo(Repo.IDocSchemaRepo schemaRepo,
                            IDocumentClassFactory docClassFactory,
                            Serialize.IDynamicSerializer docSeri) : base(schemaRepo, docClassFactory, docSeri)
    { }

    ///<inherit/>
    protected override IDocSchemaProcessor createProcessor(DocumentSchema schema) => new DocSchemaProcessor(schema, docClassFactory, docSeri);
  }

}