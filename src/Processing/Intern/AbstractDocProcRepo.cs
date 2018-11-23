using System;
using Microsoft.Extensions.Logging;

using Tlabs.Misc;
using Tlabs.Sync;
using Tlabs.Data.Entity;

namespace Tlabs.Data.Processing.Intern {

  ///<summary>Abstract repository of <see cref="DocSchemaProcessor"/>.</summary>
  ///<remarks>
  ///Implementations must provide a createProcessor() method to return a subclass of <see cref="DocSchemaProcessor"/>.
  ///</remarks>
  public abstract class AbstractDocProcRepo<T> where T : DocSchemaProcessor {
    ///<summary>Internal logger.</summary>
    protected static readonly ILogger<AbstractDocProcRepo<T>> log= App.Logger<AbstractDocProcRepo<T>>();
    ///<summary>Internal logger.</summary>
    protected Repo.IDocSchemaRepo schemaRepo;
    ///<summary>Document schema repo..</summary>
    protected IDocumentClassFactory docClassFactory;
    ///<summary>Document</summary>
    protected Serialize.IDynamicSerializer docSeri;
    private static readonly BasicCache<string, T> schemaCache= new BasicCache<string, T>();

    ///<summary>Ctor from services.</summary>
    protected AbstractDocProcRepo(Repo.IDocSchemaRepo schemaRepo,
                            IDocumentClassFactory docClassFactory,
                            Serialize.IDynamicSerializer docSeri)
    {
      this.schemaRepo= schemaRepo;
      this.docClassFactory= docClassFactory;
      this.docSeri= docSeri;
    }

    ///<summary>Returns a <see cref="Mutex{T}"/> MUTEX for <paramref name="sid"/> (DocumentSchema.TypeId).</summary>
    /// <remarks>NOTE:<p>
    /// The caller MUST <see cref="IDisposable.Dispose()">Dispose()</see> the returned accquired Mutex in order to release the Mutex controlled exclusive access to
    /// the <see cref="DocSchemaProcessor"/>.
    /// </p>
    /// </remarks>
    public Mutex<T> GetDocumentProcessorBySid<DocT>(string sid) where DocT : Entity.Intern.BaseDocument<DocT> {
      if (null == sid) throw new ArgumentNullException(nameof(sid));
      Func<T> loadSchemaProc= () => {  //helping Omnisharp...
        var docSchema= schemaRepo.GetByTypeId(sid);
        return this.createProcessor(docSchema);
      };
      return new Mutex<T>(schemaCache[sid, loadSchemaProc]);
    }

    ///<summary>Returns a <see cref="Mutex{T}"/> MUTEX for <see cref="DocumentSchema"/>'s alternate name.</summary>
    /// <remarks>NOTE:<p>
    /// The caller MUST <see cref="IDisposable.Dispose()">Dispose()</see> the returned accquired Mutex in order to release the Mutex controlled exclusive access to
    /// the <see cref="DocSchemaProcessor"/>.
    /// </p>
    /// </remarks>
    public Mutex<T> GetDocumentProcessorByAltName<DocT>(string altName) where DocT : Entity.Intern.BaseDocument<DocT> {
      if (null == altName) throw new ArgumentNullException(nameof(altName));
      return GetDocumentProcessorBySid<DocT>(schemaRepo.GetByAltTypeName(altName).TypeId);
    }

    ///<summary>Returns a <see cref="Mutex{T}"/> MUTEX for <paramref name="doc"/>.</summary>
    /// <remarks>NOTE:<p>
    /// The caller MUST <see cref="IDisposable.Dispose()">Dispose()</see> the returned accquired Mutex in order to release the Mutex controlled exclusive access to
    /// the <see cref="DocSchemaProcessor"/>.
    /// </p>
    /// </remarks>
    public Mutex<T> GetDocumentProcessor<DocT>(DocT doc) where DocT : Entity.Intern.BaseDocument<DocT> {
      if (null == doc) throw new ArgumentNullException(nameof(doc));
      return GetDocumentProcessorBySid<DocT>(doc.Sid);
    }

    ///<summary>Return <paramref name="doc"/>'s Body as object (according to its <see cref="DocumentSchema"/>).</summary>
    public object LoadDocumentBodyObject<DocT>(DocT doc) where DocT : Entity.Intern.BaseDocument<DocT> {
      using (var mtx= GetDocumentProcessor(doc))
        return mtx.Value.LoadBodyObject(doc);
    }

    ///<summary>Update <paramref name="doc"/>'s body with <paramref name="bodyObj"/>.</summary>
    public object UpdateDocumentBodyObject<DocT>(DocT doc, object bodyObj) where DocT : Entity.Intern.BaseDocument<DocT> {
      using (var mtx = GetDocumentProcessor(doc))
        return mtx.Value.UpdateBodyObject(doc, bodyObj);
    }

    ///<summary>Create a new <see cref="DocSchemaProcessor"/> for <paramref name="schema"/>.</summary>
    ///<exception cref="CodeSyntaxException">Thrown when syntax error(s) in the validation code are detected.</exception>
    public Mutex<T> CreateDocumentProcessor<DocT>(DocumentSchema schema) where DocT : Entity.Intern.BaseDocument<DocT> {
      if (null == schema) throw new ArgumentNullException(nameof(schema));
      var docProc= schemaCache[schema.TypeId]= createProcessor(schema);
      return new Mutex<T>(docProc);
    }

    ///<summary>Create a new <see cref="DocSchemaProcessor"/> instance for <paramref name="schema"/>.</summary>
    protected abstract T createProcessor(DocumentSchema schema);

  }


}