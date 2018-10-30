using System;
using Microsoft.Extensions.Logging;

using Tlabs.Misc;
using Tlabs.Sync;
using Tlabs.Data.Entity;

namespace Tlabs.Data.Processing {

  ///<summary>Repository of <see cref="DocSchemaProcessor"/>.</summary>
  public class DocProcessorRepo {
    private ILogger<DocProcessorRepo> log;
    private Repo.IDocSchemaRepo schemaRepo;
    private IDocumentClassFactory docClassFactory;
    private Serialize.IDynamicSerializer docSeri;
    private Tlabs.CalcNgn.Calculator calcNgn;
    private static readonly BasicCache<string, DocSchemaProcessor> schemaCache= new BasicCache<string, DocSchemaProcessor>();

    ///<summary>Ctor from services.</summary>
    public DocProcessorRepo(Repo.IDocSchemaRepo schemaRepo,
                            IDocumentClassFactory docClassFactory,
                            Serialize.IDynamicSerializer docSeri,
                            Tlabs.CalcNgn.Calculator calcNgn)
    {
      this.log= App.Logger<DocProcessorRepo>();
      this.schemaRepo= schemaRepo;
      this.docClassFactory= docClassFactory;
      this.docSeri= docSeri;
      this.calcNgn= calcNgn;
    }

    ///<summary>Returns a <see cref="Mutex{DocSchemaProcessor}"/> MUTEX for <paramref name="sid"/> (DocumentSchema.TypeId).</summary>
    /// <remarks>NOTE:<p>
    /// The caller MUST <see cref="IDisposable.Dispose()">Dispose()</see> the returned accquired Mutex in order to release the Mutex controlled exclusive access to
    /// the <see cref="DocSchemaProcessor"/>.
    /// </p>
    /// </remarks>
    public Mutex<DocSchemaProcessor> GetDocumentProcessorBySid<T>(string sid) where T : Entity.Intern.BaseDocument<T> {
      if (null == sid) throw new ArgumentNullException(nameof(sid));
      Func<DocSchemaProcessor> loadSchemaProc= () => {  //helping Omnisharp...
        var docSchema= schemaRepo.GetByTypeId(sid);
        return new DocSchemaProcessor(docSchema, docClassFactory, docSeri, calcNgn);
      };
      return new Mutex<DocSchemaProcessor>(schemaCache[sid, loadSchemaProc]);
    }

    ///<summary>Returns a <see cref="Mutex{DocSchemaProcessor}"/> MUTEX for <see cref="DocumentSchema"/>'s alternate name.</summary>
    /// <remarks>NOTE:<p>
    /// The caller MUST <see cref="IDisposable.Dispose()">Dispose()</see> the returned accquired Mutex in order to release the Mutex controlled exclusive access to
    /// the <see cref="DocSchemaProcessor"/>.
    /// </p>
    /// </remarks>
    public Mutex<DocSchemaProcessor> GetDocumentProcessorByAltName<T>(string altName) where T : Entity.Intern.BaseDocument<T> {
      if (null == altName) throw new ArgumentNullException(nameof(altName));
      return GetDocumentProcessorBySid<T>(schemaRepo.GetByAltTypeName(altName).TypeId);
    }

    ///<summary>Returns a <see cref="Mutex{DocSchemaProcessor}"/> MUTEX for <paramref name="doc"/>.</summary>
    /// <remarks>NOTE:<p>
    /// The caller MUST <see cref="IDisposable.Dispose()">Dispose()</see> the returned accquired Mutex in order to release the Mutex controlled exclusive access to
    /// the <see cref="DocSchemaProcessor"/>.
    /// </p>
    /// </remarks>
    public Mutex<DocSchemaProcessor> GetDocumentProcessor<T>(T doc) where T : Entity.Intern.BaseDocument<T> {
      if (null == doc) throw new ArgumentNullException(nameof(doc));
      return GetDocumentProcessorBySid<T>(doc.Sid);
    }

    ///<summary>Return <paramref name="doc"/>'s Body as object (according to its <see cref="DocumentSchema"/>).</summary>
    public object LoadDocumentBodyObject<T>(T doc) where T : Entity.Intern.BaseDocument<T> {
      using (var mtx= GetDocumentProcessor(doc))
        return mtx.Value.LoadBodyObject(doc);
    }

    ///<summary>Update <paramref name="doc"/>'s body with <paramref name="bodyObj"/>.</summary>
    public object UpdateDocumentBodyObject<T>(T doc, object bodyObj) where T : Entity.Intern.BaseDocument<T> {
      using (var mtx = GetDocumentProcessor(doc))
        return mtx.Value.UpdateBodyObject(doc, bodyObj);
    }

    ///<summary>Create a new <see cref="DocSchemaProcessor"/> for <paramref name="schema"/>.</summary>
    ///<exception cref="CodeSyntaxException">Thrown when syntax error(s) in the validation code are detected.</exception>
    public Mutex<DocSchemaProcessor> CreateDocumentProcessor<T>(DocumentSchema schema) where T : Entity.Intern.BaseDocument<T> {
      if (null == schema) throw new ArgumentNullException(nameof(schema));
      var docProc= schemaCache[schema.TypeId]= new DocSchemaProcessor(schema, docClassFactory, docSeri, calcNgn);
      return new Mutex<DocSchemaProcessor>(docProc);
    }


  }


}