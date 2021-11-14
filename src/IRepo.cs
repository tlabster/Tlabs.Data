using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Tlabs.Data {

  ///<summary>Interface of an <typeparamref name="TEntity"/> repository.</summary>
  public interface IRepo<TEntity> : INonQueryRepo<TEntity> {

    ///<summary>A queryable enumeration of *ALL* entities of <typeparamref name="TEntity"/> in the store.</summary>
    ///<remarks>Any changes to returned entities are beeing tracked (for potential commit with the underlying store). </remarks>
    System.Linq.IQueryable<TEntity> All { get; }

    ///<summary>A queryable enumeration of *ALL* entities of <typeparamref name="TEntity"/> in the store.</summary>
    ///<remarks>Changes to returned entities are NOT beeing tracked.</remarks>
    System.Linq.IQueryable<TEntity> AllUntracked { get; }
  }

  ///<summary>Interface of an non queryable <typeparamref name="TEntity"/> repository.</summary>
  public interface INonQueryRepo<TEntity> {

    ///<summary>Get the underlying <see ref="IDataStore"/>.</summary>
    IDataStore Store { get; }

    ///<summary>Get a persistent entity instance from the data store.</summary>
    TEntity Get(params object[] ids);

    ///<summary>Get the data store identifier value(s) of the given <paramref name="entity"/>.</summary>
    object GetIdentifier(TEntity entity);

    ///<summary>Add <paramref name="entity"/> for inserting to the store.</summary>
    TEntity Insert(TEntity entity);

    ///<summary>Add an <see cref="IEnumerable{TEntity}"/> of <paramref name="entities"/> for inserting to the store.</summary>
    IEnumerable<TEntity> Insert(IEnumerable<TEntity> entities);

    ///<summary>Merge given <paramref name="ent"/> with any persistent version.</summary>
    ///<remarks>
    ///Only properties with non default values are set on the persistent entity. Any property changed by the merge operation marks the entity
    ///as modified for updating with the store.
    ///</remarks>
    TEntity Merge(TEntity ent);

    ///<summary>Track given <paramref name="entity"/> as modified for updating with the store.</summary>
    TEntity Update(TEntity entity);

    ///<summary>Track <see cref="IEnumerable{TEntity}"/> of <paramref name="entities"/> as modified for updating with the store.</summary>
    IEnumerable<TEntity> Update(IEnumerable<TEntity> entities);

    ///<summary>Mark given <paramref name="entity"/> as deleted for removing from the store.</summary>
    void Delete(TEntity entity);

    ///<summary>Mark <see cref="IEnumerable{TEntity}"/> of  <paramref name="entities"/> as deleted for removing from the store.</summary>
    void Delete(IEnumerable<TEntity> entities);

    ///<summary>Attach given <paramref name="entity"/> as unchanged (but tracked).</summary>
    TEntity Attach(TEntity entity);

    ///<summary>Evict given <paramref name="entity"/> from beeing tracked by the repository.</summary>
    void Evict(TEntity entity);

    ///<summary>Explicitly load collection <paramref name="prop">property</paramref> from <paramref name="ent"/> (if not already loaded).</summary>
    void LoadExplicit<P>(TEntity ent, Expression<Func<TEntity, IEnumerable<P>>> prop) where P : class;

    ///<summary>Explicitly load referenced <paramref name="prop">property</paramref> from <paramref name="ent"/> (if not already loaded).</summary>
    void LoadExplicit<P>(TEntity ent, Expression<Func<TEntity, P>> prop) where P : class;
  }

}
