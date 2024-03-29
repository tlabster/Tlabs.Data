﻿using System;
using System.Collections.Generic;

namespace Tlabs.Data.Repo.Intern {

  ///<summary>Base data repository for <typeparamref name="TEntity"/>.</summary>
  public class BaseRepo<TEntity> : BaseNonQueryRepo<TEntity>, IRepo<TEntity> where TEntity : class, new() {

    ///<summary>Ctor from <paramref name="store"/>.</summary>
    public BaseRepo(IDataStore store) : base(store) { }

    ///<Inherit/>
    public virtual System.Linq.IQueryable<TEntity> All {
      get => store.Query<TEntity>();
    }

    ///<Inherit/>
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

    ///<Inherit/>
    public IDataStore Store { get { return store; } }

    ///<Inherit/>
    public virtual TEntity Get(params object[] keys) => store.Get<TEntity>(keys);

    ///<Inherit/>
    public virtual object GetIdentifier(TEntity ent) => store.GetIdentifier<TEntity>(ent);

    ///<Inherit/>
    public virtual TEntity Insert(TEntity ent) => store.Insert<TEntity>(ent);

    ///<Inherit/>
    public virtual IEnumerable<TEntity> Insert(IEnumerable<TEntity> entities) => store.Insert(entities);

    ///<Inherit/>
    public virtual TEntity Merge(TEntity ent) => store.Merge<TEntity>(ent);

    ///<Inherit/>
    public virtual TEntity Update(TEntity ent) => store.Update<TEntity>(ent);

    ///<Inherit/>
    public virtual IEnumerable<TEntity> Update(IEnumerable<TEntity> entities) => store.Update(entities);

    ///<Inherit/>
    public virtual void Delete(TEntity ent) => store.Delete<TEntity>(ent);

    ///<Inherit/>
    public virtual void Delete(IEnumerable<TEntity> entities) => store.Delete(entities);

    ///<Inherit/>
    public virtual TEntity Attach(TEntity ent) => store.Attach<TEntity>(ent);

    ///<Inherit/>
    public virtual void Evict(TEntity ent) => store.Evict<TEntity>(ent);

    ///<Inherit/>
    public void LoadExplicit<P>(TEntity ent, System.Linq.Expressions.Expression<Func<TEntity, System.Collections.Generic.IEnumerable<P>>> prop) where P : class {
      store.LoadExplicit<TEntity, P>(ent, prop);
    }

    ///<Inherit/>
    public void LoadExplicit<P>(TEntity ent, System.Linq.Expressions.Expression<Func<TEntity, P?>> prop) where P : class {
      store.LoadExplicit<TEntity, P>(ent, prop);
    }
  }

}