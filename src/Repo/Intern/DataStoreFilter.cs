using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Tlabs.Data.Repo.Intern {
  /// <summary>Specifies a method that filters a query by returning a filtered query.</summary>
  /// <typeparam name="E">The element type of the query to filter.</typeparam>
  public interface IDataStoreFilter<E> {
    /// <summary>Filters the specified query.</summary>
    /// <param name="query">The query.</param>
    /// <returns>A filtered query.</returns>
    IQueryable<E> Filter(IQueryable<E> query);
  }

  /// <summary>Enables filtering of DataStore entities.</summary>
  /// <typeparam name="E">The type of the entity.</typeparam>
  public static class DataStoreFilter<E> {
    /// <summary>Returns a <see cref="IDataStoreFilter{E}"/> instance that allows construction of <see cref="IDataStoreFilter{E}"/> objects though the use of LINQ syntax. </summary>
    /// <returns>A <see cref="IDataStoreFilter{E}"/> instance.</returns>
    public static IDataStoreFilter<E> AsQueryable() {
      return new EmptyDataStoreFilter();
    }

    /// <summary>Returns a <see cref="IDataStoreFilter{E}"/> that filters a sequence based on a predicate. </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>A new <see cref="IDataStoreFilter{E}"/>.</returns>
    public static IDataStoreFilter<E> Where(Expression<Func<E, bool>> predicate) {
      if (predicate == null) throw new ArgumentNullException(nameof(predicate));

      return new WhereDataStoreFilter<E>(predicate);
    }

    /// <summary>An empty entity filter.</summary>
    [DebuggerDisplay("DataStoreFilter ( Unfiltered )")]
    private sealed class EmptyDataStoreFilter : IDataStoreFilter<E> {
      /// <summary>Filters the specified query.</summary>
      /// <param name="query">The query.</param>
      /// <returns>A filtered query.</returns>
      public IQueryable<E> Filter(IQueryable<E> query) {
        // We don't filter, but simply return the query.
        return query;
      }

      /// <summary>Returns an empty string.</summary>
      /// <returns>An empty string.</returns>
      public override string ToString() {
        return string.Empty;
      }
    }
  }

  /// <summary>Extension methods for the <see cref="IDataStoreFilter{E}"/> interface. </summary>
  public static class DataStoreFilterExtensions {
    /// <summary>Returns a <see cref="IDataStoreFilter{E}"/> that filters a sequence based on a predicate. </summary>
    /// <typeparam name="E">The type of the entity.</typeparam>
    /// <param name="baseFilter">The base filter.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>A new <see cref="IDataStoreFilter{E}"/>.</returns>
    public static IDataStoreFilter<E> Where<E>(this IDataStoreFilter<E> baseFilter, Expression<Func<E, bool>> predicate) {
      if (baseFilter == null) throw new ArgumentNullException(nameof(baseFilter));
      if (predicate == null) throw new ArgumentNullException(nameof(predicate));

      return new WhereDataStoreFilter<E>(baseFilter, predicate);
    }
  }

  /// <summary>Filters the query using a predicate. </summary>
  /// <typeparam name="E">The type of the entity.</typeparam>
  [DebuggerDisplay("DataStoreFilter ( where {ToString()} )")]
  internal sealed class WhereDataStoreFilter<E> : IDataStoreFilter<E> {
    private readonly IDataStoreFilter<E> baseFilter;
    private readonly Expression<Func<E, bool>> predicate;

    /// <summary>Initializes a new instance of the <see cref="WhereDataStoreFilter{E}"/> class.</summary>
    /// <param name="predicate">The predicate.</param>
    public WhereDataStoreFilter(Expression<Func<E, bool>> predicate) {
      this.predicate = predicate;
    }

    /// <summary>Initializes a new instance of the <see cref="WhereDataStoreFilter{E}"/> class.</summary>
    /// <param name="baseFilter">The base filter.</param>
    /// <param name="predicate">The predicate.</param>
    public WhereDataStoreFilter(IDataStoreFilter<E> baseFilter, Expression<Func<E, bool>> predicate) {
      this.baseFilter = baseFilter;
      this.predicate = predicate;
    }

    /// <summary>Filters the specified query.</summary>
    /// <param name="query">The query.</param>
    /// <returns>A filtered query.</returns>
    public IQueryable<E> Filter(IQueryable<E> query) {
      if (this.baseFilter == null)
        return query.Where(this.predicate);

      return this.baseFilter.Filter(query).Where(this.predicate);
    }

    /// <inherit/>
    public override string ToString() {
      string baseFilterPresentation =
                this.baseFilter != null ? this.baseFilter.ToString() : string.Empty;

      // The returned string is used in the DebuggerDisplay.
      if (!string.IsNullOrEmpty(baseFilterPresentation))
        return baseFilterPresentation + ", " + this.predicate.ToString();

      return this.predicate.ToString();
    }
  }
}
