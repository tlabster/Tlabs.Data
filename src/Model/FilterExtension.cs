using System.Collections.Generic;
using System.Linq;


namespace Tlabs.Data.Model {

  ///<summary><see cref="QueryFilter"/> extension.</summary>
  public static class FilterExtension {

    ///<summary>Apply this <paramref name="filter"/> on <paramref name="query"/> using <paramref name="filterMap"/> and optional <paramref name="sorterMap"/>.</summary>
    public static IQueryable<T> Apply<T>(this QueryFilter filter,
                                         IQueryable<T> query,
                                         IDictionary<string, QueryFilter.FilterExpression<T>> filterMap,
                                         IDictionary<string, QueryFilter.SorterExpression<T>>? sorterMap= null) where T : Entity.Intern.BaseEntity
    {
      if (null == filter) return query;
      var filteredQuery= query;
      if (null != filter.Properties) foreach (var kv in filter.Properties) {
        if (filterMap.TryGetValue(kv.Key, out var fx))
          filteredQuery= fx(filteredQuery, kv.Value);
      }

      if (null != sorterMap && null != filter.SortAscBy && 0 != filter.SortAscBy.Count) foreach (var kv in filter.SortAscBy) {
        if (sorterMap.TryGetValue(kv.Key, out var sx))
          filteredQuery= sx(filteredQuery, kv.Value);
      }
      else filteredQuery= filteredQuery.OrderBy(e => e.Id);
      return filteredQuery;
    }

    ///<summary>Apply the limit (start, limit) of this <paramref name="filter"/> on <paramref name="query"/>.</summary>
    public static IQueryable<T> ApplyLimit<T>(this QueryFilter filter, IQueryable<T> query) {
      if (null == filter) return query;
      var limit= query;
      if (filter.Start.HasValue) {
        limit= limit.Skip(filter.Start.Value);
      }
      if (filter.Limit.HasValue)
        limit= limit.Take(filter.Limit.Value);
      return limit;
    }

  }
}