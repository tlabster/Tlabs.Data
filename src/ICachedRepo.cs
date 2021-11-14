using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Tlabs.Data.Event;

namespace Tlabs.Data {

  ///<summary>Interface of a <see cref="IRepo{TEntity}"/> for entities that are assumed to have a small number of (cached) persistent instances.</summary>
  public interface ICachedRepo<TEntity> : INonQueryRepo<TEntity> where TEntity : class, new() {

    ///<summary>A queryable enumeration of *ALL* (cached) entities of <typeparamref name="TEntity"/> in the store
    /// (with optional <paramref name="querySupplement"/> to be used e.g. for <c>LoadRelated(...)</c> clauses).
    ///</summary>
    ///<remarks>Changes to returned entities are NOT beeing tracked.</remarks>
    System.Linq.IQueryable<TEntity> AllUntracked(Func<IQueryable<TEntity>, IQueryable<TEntity>> querySupplement= null);
    
    ///<summary>(Mark) <paramref name="ent"/> as updated or inserted.</summary>
    TEntity InsertOrUpdate(TEntity ent);

  }
}

namespace Tlabs.Data.Repo.Intern {
    ///<summary><see cref="IRepo{TEntity}"/> for a small number of (cached) persistent instances.</summary>
    public class CachedRepo<TEntity> : Intern.BaseNonQueryRepo<TEntity>, ICachedRepo<TEntity> where TEntity : class, new() {
    ///<summary>Maximum cache size.</summary>
    public const int MAX_CACHE= 300;

    static readonly ILogger<CachedRepo<TEntity>> log= Tlabs.App.Logger<CachedRepo<TEntity>>();
    static IQueryable<TEntity> cache;
    static object sync= new object();
    static CachedRepo() { 
      DataStoreEvent<TEntity>.Inserting+= evictCache;
      DataStoreEvent<TEntity>.Updating+= evictCache;
      DataStoreEvent<TEntity>.Deleting+= evictCache;
    }

    private static void evictCache(Event.IEvent<TEntity> ev) {
      lock(sync) cache= null;
    }

    ///<summary>Ctor from <paramref name="store"/>.</summary>
    public CachedRepo(IDataStore store) : base(store) { }

    ///<inherit/>
    public IQueryable<TEntity> AllUntracked(Func<IQueryable<TEntity>, IQueryable<TEntity>> querySupplement= null) {
      IQueryable<TEntity> all= cache;
      if (null == all) lock (sync) {
        all= store.UntrackedQuery<TEntity>();
        all= querySupplement?.Invoke(all) ?? all;
        var lst= all.Take(MAX_CACHE+1).ToList();
        if (lst.Count <= MAX_CACHE)
          all= cache= lst.AsQueryable();
        else log.LogWarning("Maximum cache size ({max}) exceeded. Using raw IQuerable from store !", MAX_CACHE);
      }
      return all;
    }

    ///<inherit/>
    public TEntity InsertOrUpdate(TEntity ent) => (null == ent) ? Insert(new TEntity()) : Update(ent);

  }

}
