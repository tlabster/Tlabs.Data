using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tlabs.Data.Repo.Intern {

  ///<summary>Base data repository for <typeparamref name="TEntity"/>.</summary>
  public class BaseRepo<TEntity> : BaseNonQueryRepo<TEntity>, IRepo<TEntity> where TEntity : class, new() {

    ///<summary>Ctor from <paramref name="store"/>.</summary>
    public BaseRepo(IDataStore store) : base(store) { }

    /// <inheritdoc/>
    public virtual System.Linq.IQueryable<TEntity> All {
      get => store.Query<TEntity>();
    }

    /// <inheritdoc/>
    public virtual System.Linq.IQueryable<TEntity> AllUntracked {
      get => store.UntrackedQuery<TEntity>();
    }
  }

  ///<summary>Base data repository for <typeparamref name="TEntity"/>.</summary>
  public class BaseNonQueryRepo<TEntity> : INonQueryRepo<TEntity> where TEntity : class, new() {
    /// <summary>Data store</summary>
    protected IDataStore store;

    ///<summary>Ctor from <paramref name="store"/>.</summary>
    public BaseNonQueryRepo(IDataStore store) {
      if (null == (this.store= store)) throw new ArgumentNullException(nameof(store));
    }

    /// <inheritdoc/>
    public IDataStore Store { get { return store; } }

    /// <inheritdoc/>
    public virtual TEntity Get(params object[] keys) => store.Get<TEntity>(keys);

    /// <inheritdoc/>
    public Task<TEntity> GetAsync(CancellationToken token = default, params object[] ids) => store.GetAsync<TEntity>(token, ids);

    /// <inheritdoc/>
    public virtual object GetIdentifier(TEntity ent) => store.GetIdentifier<TEntity>(ent);

    /// <inheritdoc/>
    public virtual TEntity Insert(TEntity ent) => store.Insert<TEntity>(ent);

    /// <inheritdoc/>
    public virtual IEnumerable<TEntity> Insert(IEnumerable<TEntity> entities) => store.Insert(entities);

    /// <inheritdoc/>
    public Task<TEntity> InsertAsync(TEntity entity, CancellationToken token = default) => store.InsertAsync(entity, token);

    /// <inheritdoc/>
    public Task<IEnumerable<TEntity>> InsertAsync(IEnumerable<TEntity> entities, CancellationToken token = default) => store.InsertAsync(entities, token);

    /// <inheritdoc/>
    public virtual TEntity Merge(TEntity ent) => store.Merge<TEntity>(ent);

    /// <inheritdoc/>
    public virtual TEntity Update(TEntity ent) => store.Update<TEntity>(ent);

    /// <inheritdoc/>
    public virtual IEnumerable<TEntity> Update(IEnumerable<TEntity> entities) => store.Update(entities);

    /// <inheritdoc/>
    public virtual void Delete(TEntity ent) => store.Delete<TEntity>(ent);

    /// <inheritdoc/>
    public virtual void Delete(IEnumerable<TEntity> entities) => store.Delete(entities);

    /// <inheritdoc/>
    public virtual TEntity Attach(TEntity ent) => store.Attach<TEntity>(ent);

    /// <inheritdoc/>
    public virtual void Evict(TEntity ent) => store.Evict<TEntity>(ent);

    /// <inheritdoc/>
    public void LoadExplicit<P>(TEntity ent, System.Linq.Expressions.Expression<Func<TEntity, System.Collections.Generic.IEnumerable<P>>> prop) where P : class {
      store.LoadExplicit<TEntity, P>(ent, prop);
    }

    /// <inheritdoc/>
    public void LoadExplicit<P>(TEntity ent, System.Linq.Expressions.Expression<Func<TEntity, P?>> prop) where P : class {
      store.LoadExplicit<TEntity, P>(ent, prop);
    }
  }

}