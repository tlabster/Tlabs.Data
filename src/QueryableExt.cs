using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Tlabs.Data {

  ///<summary>Extension of <see cref="IQueryable{TEntity}" /> to support eager loading of related data.</summary>
  public static class QueryableExt {
    ///<summary>Returns the only element of a sequence for <paramref name="key"/>.</summary>
    ///<exception cref="DataEntityNotFoundException{T}">if there is NOT exacty one element</exception>
    public static E SingleEntity<E>(this IQueryable<E> query, object key) where E : class {
      var entity= query.SingleOrDefault();
      if (null == entity) throw new DataEntityNotFoundException<E>(key);
      return entity;
    }

    ///<summary>Returns the only element of a sequence for <paramref name="key"/>.</summary>
    ///<exception cref="DataEntityNotFoundException{T}">if there is NOT exacty one element</exception>
    public static E SingleEntity<E>(this IQueryable<E> query, Expression<Func<E, bool>> predicate, object key) where E : class {
      var entity= query.SingleOrDefault(predicate);
      if (null == entity) throw new DataEntityNotFoundException<E>(key);
      return entity;
    }
  }

}