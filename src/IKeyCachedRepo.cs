using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Tlabs.Data.Event;

namespace Tlabs.Data {

  ///<summary>Interface of a look up by key repository.</summary>
  public interface IKeyLookup<T, K> where T : class, new() {
    ///<summary>Return <typeparamref name="T"/> with <paramref name="key"/> (and optional <paramref name="mustExist"/> flag).</summary>
    ///<exception cref="DataEntityNotFoundException{T}"><paramref name="mustExist"/> is true and no entity with given <paramref name="key"/></exception>
    T GetByKey(K key, bool mustExist= false);
  }

  ///<summary>Interface of a <see cref="IRepo{TEntity}"/> for entities that are assumed to have a small number of (cached) persistent instances.</summary>
  public interface IKeyCachedRepo<TEntity, K> : INonQueryRepo<TEntity>, IKeyLookup<TEntity, K> where TEntity : class, new() {

    ///<summary>A queryable enumeration of *ALL* (cached) entities of <typeparamref name="TEntity"/> in the store.</summary>
    ///<remarks>Changes to returned entities are NOT beeing tracked.</remarks>
    System.Linq.IQueryable<TEntity> AllUntracked { get; }
    
    ///<summary>(Mark) <paramref name="ent"/> as updated or inserted.</summary>
    TEntity InsertOrUpdate(TEntity ent);
  }

  ///<summary>Interface of a <see cref="IKeyCachedRepo{TEntity, K}"/> that is caching <typeparamref name="TModel"/> instances.</summary>
  public interface IKeyCachedRepo<TEntity, TModel, K> : INonQueryRepo<TEntity>, IKeyLookup<TModel, K> where TEntity : class, new() where TModel : class, new() {

    ///<summary>A queryable enumeration of *ALL* (cached) entities of <typeparamref name="TModel"/> in the store.</summary>
    System.Linq.IQueryable<TModel> AllUntracked { get; }

    ///<summary>(Mark) <paramref name="ent"/> as updated or inserted.</summary>
    TEntity InsertOrUpdate(TEntity ent);
  }
}

namespace Tlabs.Data.Repo.Intern {
    ///<summary><see cref="IRepo{TEntity}"/> for a small number of (cached) persistent instances with key.</summary>
    public abstract class AbstractKeyCachedRepo<TEntity, K> : Intern.BaseNonQueryRepo<TEntity>, IKeyCachedRepo<TEntity, K> where TEntity : class, new() {
    ///<summary>Maximum cache size.</summary>
    public const int MAX_CACHE= 300;

    static readonly ILogger<AbstractKeyCachedRepo<TEntity, K>> log= Tlabs.App.Logger<AbstractKeyCachedRepo<TEntity, K>>();
    static Dictionary<K, TEntity> cache;
    static object sync= new object();
    static AbstractKeyCachedRepo() { 
      DataStoreEvent<TEntity>.Inserting+= evictCache;
      DataStoreEvent<TEntity>.Updating+= evictCache;
      DataStoreEvent<TEntity>.Deleting+= evictCache;
    }

    static void evictCache(Event.IEvent<TEntity> ev) {
      lock(sync) cache= null;
    }

    Func<TEntity, K> obtainKey;

    ///<summary>Ctor from <paramref name="store"/>.</summary>
    public AbstractKeyCachedRepo(IDataStore store) : base(store) {
      this.obtainKey= this.getKeyExpression.Compile();
    }

    ///<inherit/>
    public IQueryable<TEntity> AllUntracked { get {
      var cache0= cache;
      IQueryable<TEntity> q= cache0?.Values.AsQueryable();
      if (null == cache0) lock (sync) {
        q= supplementalQuery(store.UntrackedQuery<TEntity>());
        cache0= q.Take(MAX_CACHE+1).ToDictionary(obtainKey);
        if (cache0.Count <= MAX_CACHE)
          q= (cache= cache0).Values.AsQueryable();
        else log.LogWarning("Maximum cache size ({max}) exceeded. Using raw IQuerable from store !", MAX_CACHE);
      }
      return q;
    }}

    ///<inherit/>
    public TEntity InsertOrUpdate(TEntity ent) => (null == ent) ? Insert(new TEntity()) : Update(ent);

    ///<inherit/>
    public TEntity GetByKey(K key, bool mustExist= false) {
      if (null == key) {
        if (!mustExist) return null;
        throw new DataEntityNotFoundException<TEntity>(key);
      }

      var cache0= cache;
      if (null != cache0) {
        if (!cache0.TryGetValue(key, out var ent) && mustExist) throw new DataEntityNotFoundException<TEntity>(key);
        return ent;
      }
      lock (sync) {
        var q= AllUntracked;  //force cache load
        if (null != cache) return GetByKey(key, mustExist);

        var predicate= Expression.Lambda<Func<TEntity, Boolean>>( Expression.Equal(Expression.Constant(key), getKeyExpression), getKeyExpression.Parameters[0] );
        var ent= q.SingleOrDefault(predicate);
        if (null == ent && mustExist) throw new DataEntityNotFoundException<TEntity>(key);
        return ent;
      }
    }


    ///<summary>Expression that obtains the key value of an entity.</summary>
    protected abstract Expression<Func<TEntity, K>> getKeyExpression { get; }

    ///<summary>Override implementations should return a supplemented <paramref name="query"/> to add e.g. <c>LoadRelated(...)</c> clauses.</summary>
    protected virtual IQueryable<TEntity> supplementalQuery(IQueryable<TEntity> query) => query;

  }

  ///<summary><see cref="IRepo{TEntity}"/> for a small number of (cached) persistent instances with key.</summary>
  public abstract class AbstractKeyCachedRepo<TEntity, TModel, K> : Intern.BaseNonQueryRepo<TEntity>, IKeyCachedRepo<TEntity, TModel, K> where TEntity : class, new() where TModel : class, new() {
    ///<summary>Maximum cache size.</summary>
    public const int MAX_CACHE= 300;

    static readonly ILogger<AbstractKeyCachedRepo<TEntity, TModel, K>> log= Tlabs.App.Logger<AbstractKeyCachedRepo<TEntity, TModel, K>>();
    static Dictionary<K, TModel> cache;
    static object sync= new object();
    static AbstractKeyCachedRepo() {
      DataStoreEvent<TEntity>.Inserting+= evictCache;
      DataStoreEvent<TEntity>.Updating+= evictCache;
      DataStoreEvent<TEntity>.Deleting+= evictCache;
    }
    static void evictCache(Event.IEvent<TEntity> ev) {
      lock (sync) cache= null;
    }

    Func<TModel, K> obtainKey;

    ///<summary>Ctor from <paramref name="store"/>.</summary>
    public AbstractKeyCachedRepo(IDataStore store) : base(store) {
      this.obtainKey= getKeyExpression.Compile();
    }

    ///<inherit/>
    public IQueryable<TModel> AllUntracked { get {
      var cache0= cache;
      IQueryable<TModel> q= cache0?.Values.AsQueryable();
      if (null == cache0) lock (sync) {
        q= selectQuery(store.UntrackedQuery<TEntity>());
        cache0= q.Take(MAX_CACHE+1).ToDictionary(obtainKey);
        if (cache0.Count <= MAX_CACHE)
          q= (cache= cache0).Values.AsQueryable();
        else log.LogWarning("Maximum cache size ({max}) exceeded. Using raw IQuerable from store !", MAX_CACHE);
      }
      return q;
    }}

    ///<inherit/>
    public TEntity InsertOrUpdate(TEntity ent) => (null == ent) ? Insert(new TEntity()) : Update(ent);

    ///<inherit/>
    public TModel GetByKey(K key, bool mustExist= false) {
      if (null == key) {
        if (!mustExist) return null;
        throw new DataEntityNotFoundException<TEntity>(key);
      }

      var cache0= cache;
      if (null != cache0) {
        if (!cache0.TryGetValue(key, out var ent) && mustExist) throw new DataEntityNotFoundException<TEntity>(key);
        return ent;
      }
      lock (sync) {
        var q= AllUntracked;  //force cache load
        if (null != cache) return GetByKey(key, mustExist);

        var predicate= Expression.Lambda<Func<TModel, Boolean>>( Expression.Equal(Expression.Constant(key), getKeyExpression), getKeyExpression.Parameters[0] );
        var ent= q.SingleOrDefault(predicate);
        if (null == ent && mustExist) throw new DataEntityNotFoundException<TEntity>(key);
        return ent;
      }
    }

    ///<summary>Expression that obtains the key value of a a model object.</summary>
    protected abstract Expression<Func<TModel, K>> getKeyExpression { get; }

    ///<summary>Override implementations must return a selection <paramref name="query"/> to return a <typeparamref name="TModel"/>.</summary>
    protected abstract IQueryable<TModel> selectQuery(IQueryable<TEntity> query);
  }

}
