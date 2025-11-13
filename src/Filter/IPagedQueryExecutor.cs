using System;
using System.Linq;
using System.Threading.Tasks;

using Tlabs.Data.Filter;

namespace Tlabs.Data.Filter {
  /// <summary>
  /// Interface for executing paged queries with filtering, sorting, and mapping
  /// </summary>
  public interface IPagedQueryExecutor<TEntity, TFilterCriteria, TSortCriteria, TSortField>
      where TEntity : class
      where TFilterCriteria : PagedFilterCriteria
      where TSortCriteria : ISortCriteria<TSortField>
      where TSortField : struct, Enum {
    /// <summary>
    /// Executes a paged query and maps entities to models using a mapper
    /// </summary>
    Task<PagedQueryResult<TModel>> ExecuteAsync<TModel>(
      QuerySpecification<TEntity, TFilterCriteria, TSortCriteria, TSortField> specification,
      Func<TEntity, TModel> mapper,
      IQueryable<TEntity>? query
    );

    /// <summary>
    /// Executes a paged query and maps entities to models using an async mapper
    /// </summary>
    Task<PagedQueryResult<TModel>> ExecuteAsync<TModel>(
      QuerySpecification<TEntity, TFilterCriteria, TSortCriteria, TSortField> specification,
      Func<TEntity, Task<TModel>> asyncMapper,
      IQueryable<TEntity>? query
    );
  }
}