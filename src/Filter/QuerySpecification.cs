using System;
using System.Linq;

using Tlabs.Data.Filter;

namespace Tlabs.Data.Filter {
  /// <summary>
  /// Complete query specification combining filtering, sorting, and pagination
  /// </summary>
  public class QuerySpecification<TEntity, TFilterCriteria, TSortCriteria, TSortField>
      where TEntity : class
      where TFilterCriteria : PagedFilterCriteria
      where TSortCriteria : ISortCriteria<TSortField>
      where TSortField : struct, Enum {

    /// <summary>
    /// Creator from <paramref name="filterCriteria"/> and <paramref name="sortCriteria"/>
    /// </summary>
    public QuerySpecification(TFilterCriteria filterCriteria, TSortCriteria? sortCriteria = default) {
      Filter = filterCriteria;
      Sort = sortCriteria;
    }

    /// <summary>
    /// Filter criteria
    /// </summary>
    public TFilterCriteria Filter { get; set; }

    /// <summary>
    /// Sort criteria
    /// </summary>
    public TSortCriteria? Sort { get; set; }

    /// <summary>
    /// Applies the given filter/sort/pagination
    /// </summary>
    /// <param name="query">Original unfiltered query</param>
    /// <param name="filterBuilder">Filter builder for the given <typeparamref name="TEntity"/> and <typeparamref name="TFilterCriteria"/></param>
    /// <param name="sortBuilder">Sort builder for the given <typeparamref name="TEntity"/> and <typeparamref name="TSortCriteria"/></param>
    /// <returns></returns>
    public IQueryable<TEntity> Apply(IQueryable<TEntity> query, IFilterBuilder<TEntity, TFilterCriteria>? filterBuilder, ISortBuilder<TEntity, TSortCriteria, TSortField>? sortBuilder) {
      if (filterBuilder != null) {
        var filterExpression = filterBuilder.BuildExpression(Filter);
        if (filterExpression != null) {
          query = query.Where(filterExpression);
        }
      }

      if (Sort != null && sortBuilder != null) {
        query = sortBuilder.ApplySort(query, Sort);
      }

      Filter.Validate();
      query = query.Skip(Filter.Skip).Take(Filter.Take);

      return query;
    }
  }
}