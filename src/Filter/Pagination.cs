using System.ComponentModel;

namespace Tlabs.Data {
  /// <summary>
  /// Pagination information
  /// </summary>
  public class Pagination {
    /// <summary>
    /// Page number
    /// </summary>
    [DefaultValue(0)]
    public int Page { get; set; } = 0;
    /// <summary>
    /// Page size
    /// </summary>
    [DefaultValue(20)]
    public int PageSize { get; set; } = 20;

    internal int Skip => (Page - 1) * PageSize;

    internal int Take => PageSize;

    /// <summary>
    /// Validates paging criteria
    /// </summary>
    public void Validate() {
      if (Page < 1) Page = 1;
      if (PageSize < 1) PageSize = 20;
      if (PageSize > 100) PageSize = 100; // Max page size
    }
  }
}