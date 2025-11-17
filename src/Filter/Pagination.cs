using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tlabs.Data {
  /// <summary>
  /// Pagination information
  /// </summary>
  public class Pagination {
    /// <summary>
    /// Page number
    /// </summary>
    [Description("Defines the page number (of size `Limit`) to be retrieved")]
    public int? Page { get; set; }
    /// <summary>
    /// Page number
    /// </summary>
    [Description("Defines the number of records to skip for the retrieved item batch")]
    public int? Offset { get; set; }
    /// <summary>
    /// Page size
    /// </summary>
    [DefaultValue(20)]
    [Description("Limits the number of items to be retrieved")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 20;

    internal int Skip {
      get {
        if (Page != null) {
          return (Page.Value - 1) * Limit;
        }

        return Offset ?? 0;
      }
    }

    internal int Take => Limit;

    /// <summary>
    /// Validates paging criteria
    /// </summary>
    public void ValidatePagination() {
      if (Offset != null && Page != null) throw new PagedQueryException($"Invalid pagination criteria: Either use '{nameof(Offset)}' or '{nameof(Page)}', not both");
      if (Offset == null && Page == null) Page = 1;
      if (Page < 0) throw new PagedQueryException($"Invalid pagination criteria: '{nameof(Page)}' must be > 0'");
      if (Offset < 0) throw new PagedQueryException($"Invalid pagination criteria: '{nameof(Offset)}' must be > 0'");
      if (Limit < 1) throw new PagedQueryException($"Invalid pagination criteria: '{nameof(Limit)}' must be > 1'");
      if (Limit > 1000) throw new PagedQueryException($"Invalid pagination criteria: '{nameof(Limit)}' must be <= 1000'");
    }
  }
}