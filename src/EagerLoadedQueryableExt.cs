using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Tlabs.Data {

  ///<summary>Extension of <see cref="IQueryable{TEntity}" /> to support eager loading of related data.</summary>
  public static class EagerLoadedQueryableExt {
    ///<summary>Load related data associated with the given <paramref name="navigationPropertyPath"/> with the entities selected by the <paramref name="query"/>.</summary>
    ///<remarks>
    /// <paramref name="navigationPropertyPath"/> is a '.' separated path of navigation property names (all) to be included.
    ///</remarks>
    public static IQueryable<E> LoadRelated<E>(this IQueryable<E> query, IDataStore store, string navigationPropertyPath) where E : class
      => store.LoadRelated(query, navigationPropertyPath);

    ///<summary>Load related data associated with the given <paramref name="navProperty"/> with the entities selected by the <paramref name="query"/>.</summary>
    ///<remarks>
    /// This method can be chained to eagerly load multiple navigation properties.
    ///</remarks>
    public static IEagerLoadedQueryable<E, P> LoadRelated<E, P>(this IQueryable<E> query, IDataStore store, Expression<Func<E, P>> navProperty) where E : class
      => store.LoadRelated(query, navProperty);

    ///<summary>Load additional related data associated with the given <paramref name="navProperty"/> based on a related type that was just loaded.</summary>
    public static IEagerLoadedQueryable<E, Prop> ThenLoadRelated<E, Prev, Prop>(this IEagerLoadedQueryable<E, IEnumerable<Prev>> query, IDataStore store, Expression<Func<Prev, Prop>> navProperty) where E : class
      => store.ThenLoadRelated(query, navProperty);

    ///<summary>Load additional related data associated with the given <paramref name="navProperty"/> based on a related type that was just loaded.</summary>
    public static IEagerLoadedQueryable<E, Prop> ThenLoadRelated<E, Prev, Prop>(this IEagerLoadedQueryable<E, Prev> query, IDataStore store, Expression<Func<Prev, Prop>> navProperty) where E : class
    => store.ThenLoadRelated(query, navProperty);

  }

}