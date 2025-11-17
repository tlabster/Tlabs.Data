using System;
using System.Collections.Generic;
using System.Linq;

namespace Tlabs.Data {
  /// <summary>
  /// Paged result of a query of entities of type <typeparamref name="T"/>
  /// </summary>
  public class PagedQueryResult<T> {
    /// <summary>
    /// Items of the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
    /// <summary>
    /// Total element count
    /// </summary>
    public int? TotalCount { get; set; }
  }
}