using System;

namespace Tlabs.Data {
  /// <summary>
  /// Exception thrown in case of invalid paged query params
  /// </summary>
  public class PagedQueryException : GeneralException {

    /// <summary>Ctor from <paramref name="templateMessage"/>.</summary>
    public PagedQueryException(string templateMessage) : base(ExceptionDataKey.ResolvedMsgParams(templateMessage, out var tmpData)) {
      this.SetMsgData(tmpData);
    }
    /// <summary>Ctor from <paramref name="templateMessage"/> and inner exception <paramref name="e"/>.</summary>
    public PagedQueryException(string templateMessage, Exception e) : base(ExceptionDataKey.ResolvedMsgParams(templateMessage, out var tmpData), e) {
      this.SetMsgData(tmpData);
    }
  }
}