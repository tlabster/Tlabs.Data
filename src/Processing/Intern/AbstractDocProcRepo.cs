using System;
using Microsoft.Extensions.Logging;

using Tlabs.Misc;
using Tlabs.Sync;
using Tlabs.Data.Entity;

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
    private static readonly BasicCache<string, IDocSchemaProcessor> schemaCache= new BasicCache<string, IDocSchemaProcessor>();

    ///<summary>Ctor from services.</summary>
    protected AbstractDocProcRepo(Repo.IDocSchemaRepo schemaRepo,
                                  IDocumentClassFactory docClassFactory,
                                  Serialize.IDynamicSerializer docSeri) {
      this.schemaRepo= schemaRepo;
      this.docClassFactory= docClassFactory;
      this.docSeri= docSeri;
    }

    ///<inherit/>
    public Mutex<IDocSchemaProcessor> GetDocumentProcessorBySid<DocT>(string sid) where DocT : Entity.Intern.BaseDocument<DocT> {
      if (null == sid) throw new ArgumentNullException(nameof(sid));
      Func<IDocSchemaProcessor> loadSchemaProc= () => {  //helping Omnisharp...
        var docSchema= schemaRepo.GetByTypeId(sid);
        return this.createProcessor(docSchema);
      };
      return new Mutex<IDocSchemaProcessor>(schemaCache[sid, loadSchemaProc]);
    }

    ///<inherit/>
    public Mutex<IDocSchemaProcessor> GetDocumentProcessorByAltName<DocT>(string altName) where DocT : Entity.Intern.BaseDocument<DocT> {
      if (null == altName) throw new ArgumentNullException(nameof(altName));
      return GetDocumentProcessorBySid<DocT>(schemaRepo.GetByAltTypeName(altName).TypeId);
    }

    ///<inherit/>
    public Mutex<IDocSchemaProcessor> GetDocumentProcessor<DocT>(DocT doc) where DocT : Entity.Intern.BaseDocument<DocT> {
      if (null == doc) throw new ArgumentNullException(nameof(doc));
      return GetDocumentProcessorBySid<DocT>(doc.Sid);
    }

    ///<inherit/>
    public object LoadDocumentBodyObject<DocT>(DocT doc) where DocT : Entity.Intern.BaseDocument<DocT> {
      using (var mtx= GetDocumentProcessor(doc))
        return mtx.Value.LoadBodyObject(doc);
    }

    ///<inherit/>
    public object UpdateDocumentBodyObject<DocT>(DocT doc, object bodyObj) where DocT : Entity.Intern.BaseDocument<DocT> {
      using (var mtx= GetDocumentProcessor(doc))
        return mtx.Value.UpdateBodyObject(doc, bodyObj);
    }

    ///<inherit/>
    public Mutex<IDocSchemaProcessor> CreateDocumentProcessor<DocT>(DocumentSchema schema) where DocT : Entity.Intern.BaseDocument<DocT> {
      if (null == schema) throw new ArgumentNullException(nameof(schema));
      var docProc= schemaCache[schema.TypeId]= createProcessor(schema);
      return new Mutex<IDocSchemaProcessor>(docProc);
    }

    ///<summary>Create a new <see cref="IDocSchemaProcessor"/> instance for <paramref name="schema"/>.</summary>
    protected abstract IDocSchemaProcessor createProcessor(DocumentSchema schema);

  }


}