using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Tlabs.Data {

  ///<summary>Interface of an eager-loaded <see cref="IQueryable{TEntity}" />.</summary>
  public interface IEagerLoadedQueryable<out TEntity, out TProperty> : IQueryable<TEntity> { }

  ///<summary>Interface of an abstract data persistence store.</summary>
  public interface IDataStore {

    ///<summary>True if tracked changes are (auto) committed on store dispose.</summary>
    bool AutoCommit { get; set; }

    ///<summary>Commit all tracked (detected) changes to the undrlying persistence store.</summary>
    void CommitChanges();

    ///<summary>Reset the change state of all tracked entities to 'unchanged'.</summary>
    void ResetChanges();

    ///<summary>Take actions to make sure the underlying store exists and optional plants all provided <paramref name="seeds"/>.</summary>
    ///<remarks>Should create the entire store, if not present.</remarks>
    void EnsureStore(IEnumerable<IDataSeed> seeds= null);

    ///<summary>Get a persistent entity instance from the data store.</summary>
    E Get<E>(params object[] ids) where E : class;

    ///<summary>Get the data store identifier value(s) of the given <paramref name="ent"/>.</summary>
    object GetIdentifier<E>(E ent) where E : class;

    ///<summary>Return a queryable enumeration of *ALL* entities of <typeparamref name="E"/> in the store.</summary>
    ///<remarks>Any changes to returned entities are beeing tracked (for potential commit with the underlying store). </remarks>
    IQueryable<E> Query<E>() where E : class;

    ///<summary>Return a queryable enumeration of *ALL* entities of <typeparamref name="E"/> in the store.</summary>
    ///<remarks>Changes to returned entities are NOT beeing tracked.</remarks>
    IQueryable<E> UntrackedQuery<E>() where E : class;

    ///<summary>Add <paramref name="ent"/> for inserting to the store.</summary>
    void Insert<E>(E ent) where E : class;

    ///<summary>Merge given <paramref name="ent"/> with any persistent version.</summary>
    ///<remarks>
    /// A merge is especially usefull when persisting data updated from an input form that typically
    /// does not manage any navigation properties (i.e. references to other entities) and thus will leave such properties with
    /// their default value (null).
    ///<list type="bullet">
    ///<item> <description>
    /// if the given entity is not persistent yet it gets inserted,
    /// else only properties with non-null values are set on the persistent entity.
    ///</description></item>
    ///<item> <description>
    /// Any property changed by the merge operation marks the entity
    /// as modified for updating with the store.
    ///</description></item>
    ///</list>
    ///</remarks>
    E Merge<E>(E ent) where E : class, new();
    
    ///<summary>Track given <paramref name="ent"/> as modified for updating with the store.</summary>
    void Update<E>(E ent) where E : class;

    ///<summary>Mark given <paramref name="ent"/> as deleted for removing from the store.</summary>
    void Delete<E>(E ent) where E : class;

    ///<summary>Attach given <paramref name="ent"/> as unchanged (but tracked).</summary>
    void Attach<E>(E ent) where E : class;

    ///<summary>Evict given <paramref name="ent"/> from beeing tracked by the repository.</summary>
    void Evict<E>(E ent) where E : class;

    ///<summary>Explicitly load collection <paramref name="prop">property</paramref> from <paramref name="ent"/> (if not already loaded).</summary>
    void LoadExplicit<E, P>(E ent, Expression<Func<E, IEnumerable<P>>> prop) where E : class where P : class;

    ///<summary>Explicitly load referenced <paramref name="prop">property</paramref> from <paramref name="ent"/> (if not already loaded).</summary>
    void LoadExplicit<E, P>(E ent, Expression<Func<E, P>> prop) where E : class where P : class;

    ///<summary>Load related data associated with the given <paramref name="navigationPropertyPath"/> with the entities selected by the <paramref name="query"/>.</summary>
    ///<remarks>
    /// <paramref name="navigationPropertyPath"/> is a '.' separated path of navigation property names (all) to be included.
    ///</remarks>
    IQueryable<E> LoadRelated<E>(IQueryable<E> query, string navigationPropertyPath) where E : class;

    ///<summary>Load related data associated with the given <paramref name="navProperty"/> with the entities selected by the <paramref name="query"/>.</summary>
    ///<remarks>
    /// This method can be chained to eagerly load multiple navigation properties.
    ///</remarks>
    IEagerLoadedQueryable<E, P> LoadRelated<E, P>(IQueryable<E> query, Expression<Func<E, P>> navProperty) where E : class;

    ///<summary>Load additional related data associated with the given <paramref name="navProperty"/> based on a related type that was just loaded.</summary>
    IEagerLoadedQueryable<E, Prop> ThenLoadRelated<E, Prev, Prop>(IEagerLoadedQueryable<E, IEnumerable<Prev>> query, Expression<Func<Prev, Prop>> navProperty) where E : class;
 
    ///<summary>Load additional related data associated with the given <paramref name="navProperty"/> based on a related type that was just loaded.</summary>
    IEagerLoadedQueryable<E, Prop> ThenLoadRelated<E, Prev, Prop>(IEagerLoadedQueryable<E, Prev> query, Expression<Func<Prev, Prop>> navProperty) where E : class;

  }
}