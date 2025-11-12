using System;
using System.Collections.Generic;
using System.Linq;

namespace Tlabs.Data {
  /// <summary>
  /// Paged result of a query of entities of type <typeparamref name="T"/>
  /// </summary>
  public class PagedResult<T> {
    /// <summary>
    /// Items of the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
    /// <summary>
    /// Total element count
    /// </summary>
    public int TotalCount { get; set; }
    /// <summary>
    /// Current page
    /// </summary>
    public int Page { get; set; }
    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }
    /// <summary>
    /// States if there is a next page
    /// </summary>
    public bool HasNextPage => Items.Count() < PageSize || !Items.Any();
  }
}