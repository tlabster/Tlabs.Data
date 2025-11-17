using System;
using System.Linq.Expressions;

namespace Tlabs.Data {
  /// <summary>
  /// Extensions to support definition of filter/sort expressions
  /// </summary>
  public static class ExpressionExtensions {
    /// <summary>
    /// Combines two expressions with AND logic
    /// </summary>
    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>>? first,
        Expression<Func<T, bool>> second) {
      if (first == null) return second;

      var parameter = Expression.Parameter(typeof(T), "x");

      var combined = Expression.AndAlso(
            Expression.Invoke(first, parameter),
            Expression.Invoke(second, parameter)
        );

      return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    /// <summary>
    /// Combines two expressions with OR logic
    /// </summary>
    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>>? first,
        Expression<Func<T, bool>> second) {
      if (first == null) return second;

      var parameter = Expression.Parameter(typeof(T), "x");

      var combined = Expression.OrElse(
            Expression.Invoke(first, parameter),
            Expression.Invoke(second, parameter)
        );

      return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }
  }
}