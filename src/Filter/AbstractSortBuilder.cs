using System;
using System.Linq;
using System.Linq.Expressions;

namespace Tlabs.Data.Filter {
  /// <summary>
  /// Provides common sorting logic for entity-specific sort builders.
  /// </summary>
  public abstract class AbstractSortBuilder<TEntity, TSortField>
      where TEntity : class
      where TSortField : struct, Enum {
    /// <summary>
    /// Returns an expression selecting the key to sort by for the given field.
    /// Must be implemented by subclasses.
    /// </summary>
    protected abstract Expression<Func<TEntity, object>> GetSortKeySelector(TSortField field);

    /// <summary>
    /// Applies sorting over a <paramref name="query"/> using the given <paramref name="criteria"/>.
    /// </summary>
    public IOrderedQueryable<TEntity> ApplySort(IQueryable<TEntity> query, SortCriteria<TSortField> criteria) {
      if (criteria == null || criteria.SortBy == null) {
        // Default sort
        return query.OrderBy(p => p);
      }

      var orderedQuery = ApplyOrderBy(query, criteria.SortBy.Field, criteria.SortBy.Direction);

      if (criteria.ThenBy != null) {
        orderedQuery = ApplyThenBy(orderedQuery, criteria.ThenBy.Field, criteria.ThenBy.Direction);
      }

      return orderedQuery;
    }

    /// <summary>
    /// Applies an initial sort (OrderBy / OrderByDescending).
    /// </summary>
    protected IOrderedQueryable<TEntity> ApplyOrderBy(IQueryable<TEntity> query, TSortField field, SortDirection direction) {
      var keySelector = GetSortKeySelector(field);
      return direction == SortDirection.Ascending
          ? query.OrderBy(keySelector)
          : query.OrderByDescending(keySelector);
    }

    /// <summary>
    /// Applies a secondary sort (ThenBy / ThenByDescending).
    /// </summary>
    protected IOrderedQueryable<TEntity> ApplyThenBy(IOrderedQueryable<TEntity> query, TSortField field, SortDirection direction) {
      var keySelector = GetSortKeySelector(field);
      return direction == SortDirection.Ascending
          ? query.ThenBy(keySelector)
          : query.ThenByDescending(keySelector);
    }
  }
}
