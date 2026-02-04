using System;

namespace Tlabs.Data {
  /// <summary>
  /// Exception for operation conflicts with current state of the entity.
  /// </summary>
  public class OperationConflictException : GeneralException
  {
    const string MSG_TEMPLATE = "Conflict during operation '{operation}' on '{entityType}' with key '{key}'. Reason: {reason}";
    /// <summary>
    /// Ctor for operation conflict exception.
    /// </summary>
    public OperationConflictException(string operation, string entityType, string key, string reason) : base(ExceptionDataKey.ResolvedMsgParams(MSG_TEMPLATE, out var data, operation, entityType, key, reason))
    {
      this.SetMsgData(data);
    }
    /// <summary>
    /// Ctor for operation conflict exception with inner exception.
    /// </summary>
    public OperationConflictException(string operation, string entityType, string key, string reason, Exception inner) : base(ExceptionDataKey.ResolvedMsgParams(MSG_TEMPLATE, out var data, operation, entityType, key, reason), inner)
    {
      this.SetMsgData(data);
    }
  }
}