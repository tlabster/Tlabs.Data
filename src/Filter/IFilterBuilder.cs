using System;
using System.Linq.Expressions;

namespace Tlabs.Data.Filter {
  /// <summary>
  /// Interface for converting filter criteria to expressions
  /// </summary>
  public interface IFilterBuilder<TEntity, TFilterCriteria>
      where TEntity : class
      where TFilterCriteria : IFilterCriteria {
    /// <summary>
    /// Converts the filter criteria to a LINQ expression
    /// </summary>
    Expression<Func<TEntity, bool>>? BuildExpression(TFilterCriteria criteria);
  }
}
