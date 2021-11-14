using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Tlabs.Data.Model {

  ///<summary>Generic filter paramters for a data query.</summary>
  public class QueryFilter {
    ///<summary>Default ctor.</summary>
    public QueryFilter() { }

    ///<summary>Copy ctor from <paramref name="other"/>.</summary>
    public QueryFilter(QueryFilter other) {
      if (null == other) return;
      this.Start= other.Start;
      this.Limit= other.Limit;
      this.NoTotalCount= other.NoTotalCount;
      this.PrecursorOffset= other.PrecursorOffset;
      if (null != other.Properties)
        this.Properties= new Dictionary<string, IConvertible>(other.Properties);
      if (null != other.SortAscBy)
        this.SortAscBy= new Dictionary<string, bool>(other.SortAscBy);
    }

    ///<summary>Precursor property name.</summary>
    public const string Precursor= nameof(Precursor);

    ///<summary>Indicator that the query resulting from this filter can omit to determine a total count value.</summary>
    public virtual bool NoTotalCount { get; set; }

    ///<summary>Indicator to use <c>Properties[nameof(Precursor)]</c> as offset criterion.</summary>
    public virtual bool PrecursorOffset { get; set; }

    ///<summary>Limit result to start at position (used with result paging).</summary>
    public virtual int? Start { get; set; }

    ///<summary>Limit result to max. entries (used with result paging).</summary>
    public virtual int? Limit { get; set; }

    ///<summary>Limit result to to match these properties.</summary>
    public virtual IDictionary<string, IConvertible> Properties { get; set; }

    ///<summary>Sort result by these properties.</summary>
    ///<remarks>true: asc, false desc.</remarks>
    public virtual IDictionary<string, bool> SortAscBy { get; set; }

    ///<summary>Delegate function to add a filter to <c>IQueryable&lt;T&gt;</c>.</summary>
    public delegate IQueryable<T> FilterExpression<T>(IQueryable<T> q, IConvertible filterVal);
    ///<summary>Delegate function to add a soter to <c>IQueryable&lt;T&gt;</c>.</summary>
    public delegate IQueryable<T> SorterExpression<T>(IQueryable<T> q, bool isAsc);
  }

  ///<summary>Time constraint <see cref="QueryFilter"/>.</summary>
  public class TimeQueryFilter : QueryFilter {
    ///<summary>Default ctor.</summary>
    public TimeQueryFilter() {
      this.Properties= new Dictionary<string, IConvertible> {
        [nameof(Since)]= null,
        [nameof(Until)]= null
      };
      this.SortAscBy= new Dictionary<string, bool>();
    }
    ///<summary>Default ctor.</summary>
    public TimeQueryFilter(QueryFilter qfilter) : this() {
      if (null == qfilter) return;
      this.Start= qfilter.Start;
      this.Limit= qfilter.Limit;
      this.NoTotalCount= qfilter.NoTotalCount;
      this.PrecursorOffset= qfilter.PrecursorOffset;
      if (null != qfilter.Properties) foreach(var prop in qfilter.Properties)
        this.Properties[prop.Key]= prop.Value;
    }
    ///<summary>Filter by <see cref="Since"/>.</summary>
    public virtual DateTime? Since {
      get => Properties[nameof(Since)] as DateTime?;
      set => Properties[nameof(Since)]= value;
    }
    ///<summary>Filter by <see cref="Until"/>.</summary>
    public virtual DateTime? Until{
      get => Properties[nameof(Until)] as DateTime?;
      set => Properties[nameof(Until)]= value;
    }
  }

  ///<summary>Result with identifiable last entry.</summary>
  public interface ILastResultIdentifiable {
    ///<summary>Property value of the last entry in <see cref="Data"/> to be used to identify any successive data.</summary>
    IConvertible LastId { get; }
  }

  ///<summary>Result list returned from a filtered query.</summary>
  public interface IResultList<T> : ILastResultIdentifiable {
    ///<summary>Total (unfiltered) result size.</summary>
    int Total { get; }
    ///<summary>Filtered result of <typeparamref name="T"/>.</summary>
    IList<T> Data { get; }
  }

  ///<summary>Query result list returned from a filtered query.</summary>
  public class QueryResult<T> : IResultList<T> {
    ///<summary>Default max result count</summary>
    public const int MAX_RESULT_COUNT= 1111;
    ///<summary>Unlimited result count</summary>
    public const int UNLIMITED_RESULT_COUNT= -1;
    ///<summary>Default ctor.</summary>
    public QueryResult() { }
    ///<summary>Ctor to provide result <see cref="Data"/> with full <see cref="Total"/> from unlimitted <paramref name="query"/>.</summary>
    public QueryResult(IQueryable<T> query) : this(query, query, UNLIMITED_RESULT_COUNT) { }
    ///<summary>Ctor to provide result <see cref="Data"/> with <see cref="Total"/> (typically max. count, optionally full total) .</summary>
    public QueryResult(IQueryable<T> query, IQueryable<T> limitedQuery, int maxCount= MAX_RESULT_COUNT) {
      this.Total= maxCount > UNLIMITED_RESULT_COUNT ? query.Take(maxCount).Count() : query.Count();
      this.Data= limitedQuery.ToList();
    }
    ///<summary>Ctor to provide result <see cref="Data"/> with <see cref="Total"/> (typically max. count, optionally full total) .</summary>
    public QueryResult(IQueryable<T> query, QueryFilter filter, int maxCount= MAX_RESULT_COUNT) {
      if (!filter.NoTotalCount)
        this.Total= maxCount > UNLIMITED_RESULT_COUNT ? query.Take(maxCount).Count() : query.Count();
      else this.Total= -1;
      this.Data= filter.ApplyLimit(query).ToList();
    }
    ///<inheritdoc/>
    public int Total { get; set; }
    ///<inheritdoc/>
    public IList<T> Data { get; set; }
    ///<inheritdoc/>
    public IConvertible LastId { get; set; }
  }

    ///<summary>Query result list returned from a filtered query transformed into <typeparamref name="T2"/> .</summary>
    public class QueryResult<T1, T2> : IResultList<T2> {
    ///<summary>Ctor to provide result <see cref="Data"/> with <see cref="Total"/> (typically max. count, optionally full total) .</summary>
    public QueryResult(IQueryable<T1> query, QueryFilter filter, Expression<Func<T1, T2>> selector, int maxCount= QueryResult<T1>.MAX_RESULT_COUNT) {
      if (!filter.NoTotalCount)
        this.Total= maxCount > QueryResult<T1>.UNLIMITED_RESULT_COUNT ? query.Take(maxCount).Count() : query.Count();
      else this.Total= -1;
      this.Data= filter.ApplyLimit(query).Select(selector).ToList();
    }
    ///<inheritdoc/>
    public int Total { get; set; }
    ///<inheritdoc/>
    public IList<T2> Data { get; set; }
    ///<inheritdoc/>
    public IConvertible LastId { get; set; }
  }

}