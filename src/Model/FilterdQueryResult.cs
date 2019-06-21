

using System;
using System.Collections.Generic;
using System.Linq;

namespace Tlabs.Data.Model {

  ///<summary>Generic filter paramters for a data query.</summary>
  public class QueryFilter {
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


  ///<summary>Result list returned from a filtered query.</summary>
  public interface IResultList<T> {
    ///<summary>Total (unfiltered) result size.</summary>
    int Total { get; }
    ///<summary>Filtered result of <typeparamref name="T"/>.</summary>
    IList<T> Data { get; }
  }

}