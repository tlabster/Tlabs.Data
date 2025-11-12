using System;
using System.Linq;

using Tlabs.Data.Filter;

namespace Tlabs.Data.Filter {
  /// <summary>
  /// Interface for applying sort criteria to queries
  /// </summary>
  public interface ISortBuilder<TEntity, TSortCriteria, TSortField>
      where TEntity : class
      where TSortCriteria : ISortCriteria<TSortField>
      where TSortField : struct, Enum {
    /// <summary>
    /// Applies sorting to the queryable
    /// </summary>
    IOrderedQueryable<TEntity> ApplySort(IQueryable<TEntity> query, TSortCriteria criteria);
  }
}