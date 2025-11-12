using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Tlabs.Data.Filter {
  /// <summary>
  /// Base interface for filter criteria specific to an entity type
  /// </summary>
  public interface IFilterCriteria {
  }

  /// <summary>
  /// Base abstract class for all typed filter criteria, including pagination
  /// </summary>
  public abstract class PagedFilterCriteria : Pagination, IFilterCriteria {
  }

  /// <summary>
  /// Sort criteria
  /// </summary>
  /// <typeparam name="TSortField"></typeparam>
  public interface ISortCriteria<TSortField> where TSortField : struct, Enum {
    /// <summary>
    /// First sort parameter
    /// </summary>
    public SortRule<TSortField>? SortBy { get; set; }

    /// <summary>
    /// Second sort parameter
    /// </summary>
    public SortRule<TSortField>? ThenBy { get; set; }
  }

  /// <summary>
  /// Interface for generic sorting rule
  /// </summary>
  /// <typeparam name="TSortField"></typeparam>
  public interface ISortRule<TSortField> where TSortField : struct, Enum {
    /// <summary>
    /// Field sorting will be performed on
    /// </summary>
    TSortField Field { get; set; }

    /// <summary>
    /// Sort direction
    /// </summary>
    SortDirection Direction { get; set; }
  }

  /// <summary>
  /// Generic class to encapsulate sorting criteria supporting two sorting fields
  /// </summary>
  /// <typeparam name="TSortField"></typeparam>
  public class SortCriteria<TSortField> : ISortCriteria<TSortField> where TSortField : struct, Enum {
    /// <inheritdoc/>
    public SortRule<TSortField>? SortBy { get; set; }

    /// <inheritdoc/>
    public SortRule<TSortField>? ThenBy { get; set; }
  }

  /// <summary>
  /// Generic class defining a sorting rule
  /// </summary>
  /// <typeparam name="TSortField">Enum type defining all possible sorting fields</typeparam>
  public class SortRule<TSortField> : ISortRule<TSortField> where TSortField : struct, Enum {
    /// <inheritdoc/>
    public TSortField Field { get; set; }

    /// <inheritdoc/>
    public SortDirection Direction { get; set; } = SortDirection.Ascending;
  }

  /// <summary>
  /// Supported sorting directions
  /// </summary>
  public enum SortDirection {
    /// <summary>
    /// Sorted ascending
    /// </summary>
    Ascending,
    /// <summary>
    /// Sorted descending
    /// </summary>
    Descending
  }
}