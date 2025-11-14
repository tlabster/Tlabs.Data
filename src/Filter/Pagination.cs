using System.ComponentModel;

namespace Tlabs.Data {
  /// <summary>
  /// Pagination information
  /// </summary>
  public class Pagination {
    /// <summary>
    /// Page number
    /// </summary>
    [DefaultValue(1)]
    public int Page { get; set; } = 1;
    /// <summary>
    /// Page size
    /// </summary>
    [DefaultValue(20)]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Maximum number of records to count when filters are applied.
    /// If count reaches this limit, totalCount will be null to avoid performance issues.
    /// </summary>
    public int MaxCountLimit { get; private set; } = 1000;

    internal int Skip => (Page - 1) * PageSize;

    internal int Take => PageSize;

    /// <summary>
    /// Validates paging criteria
    /// </summary>
    public void ValidatePagination() {
      if (Page < 1) Page = 1;
      if (PageSize < 1) PageSize = 20;
      if (PageSize > 1000) PageSize = 1000;
    }
  }
}