using System;
using System.Linq;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Tlabs.Data.Event;
using Tlabs.Data.Entity.Intern;
using Tlabs.Data.Processing;


namespace Tlabs.Data {
  ///<summary>Document cache for <typeparamref name="TDoc"/>.</summary>
  public interface IDocBodyCache<K, TDoc> where TDoc : BaseDocument<TDoc> {
    ///<summary>Product entity repository.</summary>
    IRepo<TDoc> DocRepo { get; }
    
    ///<summary>Document processor repo.</summary>
    IDocProcessorRepo DocProcessorRepo { get; }

    ///<summary>Gets or sets a document body object from the cache with specified <paramref name="tmplDoc"/> as key.</summary>
    ///<remarks>
    ///<list type="bullet">
    ///<item><description>Setting a value of null evicts any entry with key from the cache.</description></item>
    ///<item><description>Setting a value of of <typeparamref name="TDoc"/> converts the document into its body object (using <c>DocProcessorRepo.GetDocumentProcessorBySid(...))</c>).</description></item>
    ///</list>
    ///</remarks>
    object this[TDoc tmplDoc] { get; set; }

    ///<summary>Perform cache warm-up.</summary>
    bool WarmUp();
    
  }
}

namespace Tlabs.Data.Repo.Intern {

  ///<summary>Object cache.</summary>
  public interface IObjectCache<K, E> : IMemoryCache {
    ///<summary>Count of currently cached objects (informational).</summary>
    int Count { get; }

    ///<summary>Get a key from <paramref name="tmplObj"/>.</summary>
    K GetKey(E tmplObj);

    ///<summary>Configured cache options in use.</summary>
    AbstractObjectCache<K, E>.Options CfgOptions { get; }
  }

  ///<summary>Abstract object cache.</summary>
  public abstract class AbstractObjectCache<K, E> : MemoryCache, IObjectCache<K, E> {
    ///<summary>Ctor from <paramref name="opt"/>.</summary>
    public AbstractObjectCache(IOptions<Options> opt) : base(opt ?? new Options()) {
      this.CfgOptions= (opt ?? new Options()).Value;

      DataStoreEvent<E>.Deleted+= evictObj;
      DataStoreEvent<E>.Updated+= evictObj;
      DataStoreEvent<E>.Inserted+= evictObj;
    }

    void evictObj(Event.IEvent<E> ev) => Remove(GetKey(ev.Entity));

    ///<inheritdoc/>
    public abstract K GetKey(E tmplObj);

    ///<inheritdoc/>
    public Options CfgOptions { get; }

    ///<summary><see cref="MemoryCacheOptions"/>.</summary>
    public class Options : MemoryCacheOptions, IOptions<Options> {
      ///<summary>Set to true to indicate that cache warming is desired.</summary>
      public bool Warming { get; set; }
      Options IOptions<Options>.Value => this;
    }
  }

  ///<summary>Abstract document Cache.</summary>
  public abstract class AbstractDocBodyCache<K, TDoc> : IDocBodyCache<K, TDoc> where TDoc : BaseDocument<TDoc> {
    readonly IObjectCache<K, TDoc> cache;
    readonly MemoryCacheEntryOptions entryOpt= new MemoryCacheEntryOptions();

    ///<summary>Ctor from <paramref name="cache"/>, <paramref name="docProcRepo"/> and <paramref name="docRepo"/>.</summary>
    public AbstractDocBodyCache(IObjectCache<K, TDoc> cache, IDocProcessorRepo docProcRepo, IRepo<TDoc> docRepo) {
      if (null == (this.cache= cache)) throw new ArgumentNullException(nameof(cache));
      this.DocRepo= docRepo;
      this.DocProcessorRepo= docProcRepo;

      if (cache.CfgOptions.SizeLimit.HasValue)
        entryOpt.Size= 1;
    }

    ///<summary>Document <see cref="IRepo{TDoc}"/>.</summary>
    public IRepo<TDoc> DocRepo { get; }

    ///<summary><see cref="IDocProcessorRepo"/>.</summary>
    public IDocProcessorRepo DocProcessorRepo { get; }

    ///<inheritdoc/>
    public object this[TDoc tmplDoc] {
      get {
        var key= cache.GetKey(tmplDoc);
        if (null == key) return null;
        if (cache.TryGetValue(key, out var obj)) return obj;
        var doc= ObtainMissingDocument(tmplDoc);
        if (null == doc) return null;
        return cache.Set(key, ConvertBodyObj(doc), entryOpt);
      }
      set {
        var key= cache.GetKey(tmplDoc);
        if (null == key) return;
        if (null == value) {
          cache.Remove(key);
          return;
        }
        cache.Set(key, ConvertBodyObj(value), entryOpt);
      }
    }

    ///<summary>Convert <paramref name="doc"/> into body object.</summary>
    protected virtual object ConvertBodyObj(object doc) {
      var docObj= doc as TDoc;
      if (null != docObj)
        return DocProcessorRepo.GetDocumentProcessorBySid<TDoc>(docObj.Sid).LoadBodyObject<TDoc>(docObj);
      return doc; //no convertion
    }

    ///<summary>Obtain a <typeparamref name="TDoc"/> not contained in cache from <paramref name="tmplObj"/>.</summary>
    ///<remarks>
    ///If implementation returns null, null is returned from the cache indexer.
    ///</remarks>
    protected abstract TDoc ObtainMissingDocument(TDoc tmplObj);

    ///<summary>Perform cache warm-up.</summary>
    ///<remarks>
    ///Default implementation is loading all documents of <see cref="DocRepo"/>.
    ///</remarks>
    public virtual bool WarmUp() {
      var cfg= cache.CfgOptions;
      int cnt= 0;
      if (   !cfg.Warming
          || cfg.SizeLimit.HasValue && cfg.SizeLimit < (cnt= DocRepo.AllUntracked.Count())) return false;

      /* Fetch first to initialize IDocProcessorRepo's IDocSchemaProcessor cache to avoid a nasty
       *  "A second operation started on this context before a previous operation completed" error from EF...
       */
      var d= DocRepo.AllUntracked.FirstOrDefault();
      this[d]= ConvertBodyObj(d);

      foreach(var doc in DocRepo.AllUntracked.AsEnumerable())
        this[doc]= ConvertBodyObj(doc);   //add to cache
      return true;
    }

  }

}