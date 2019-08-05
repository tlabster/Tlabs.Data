using System;
using System.Linq;
using System.Threading;

using Tlabs.Data.Event;

namespace Tlabs.Data {

  ///<summary>Interface of a <see cref="IRepo{TEntity}"/> for entities that are assumed to have a small number of (cached) persistent instances.</summary>
  public interface ICachedRepo<TEntity> : IRepo<TEntity> where TEntity : class, new() {

    ///<summary>(Mark) <paramref name="ent"/> as updated or inserted.</summary>
    TEntity InserOrUpdate(TEntity ent);

  }
}

namespace Tlabs.Data.Repo.Intern {
    ///<summary><see cref="IRepo{TEntity}"/> for a small number of (cached) persistent instances.</summary>
    public class CachedRepo<TEntity> : Intern.BaseRepo<TEntity>, ICachedRepo<TEntity> where TEntity : class, new() {
    ///<summary>Maximum cache size.</summary>
    public const int MAX_CACHE= 300;

    static IQueryable<TEntity> cache;
    static TEntity sync= new TEntity();
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
    public override IQueryable<TEntity> All => throw new InvalidOperationException("Only untracked queries supported with cached entity.");

    ///<inherit/>
    public override IQueryable<TEntity> AllUntracked {
      get {
        IQueryable<TEntity> all;
        lock(sync) {
          if (null == (all= cache)) {
            all= UntrackedQuery();
            var lst= all.Take(MAX_CACHE+1).ToList();
            if (lst.Count <= MAX_CACHE)
              all= cache= lst.AsQueryable();
          }
        }
        return all;
      }
    }

    ///<inherit/>
    public TEntity InserOrUpdate(TEntity ent) => (null == ent) ? Insert(new TEntity()) : Update(ent);

    ///<summary>Untracked query.</summary>
    protected virtual IQueryable<TEntity> UntrackedQuery() => base.AllUntracked;

  }

}
