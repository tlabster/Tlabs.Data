using System;

namespace Tlabs.Data {

  /// <summary>Data persistence exception.</summary>
  public class DataPersistenceException : GeneralException {
    /// <summary>Ctor from inner exception <paramref name="e"/>.</summary>
    public DataPersistenceException(Exception e) : base(e.Message, e) { }

    /// <summary>Ctor from <paramref name="msg"/> and inner exception <paramref name="e"/>.</summary>
    public DataPersistenceException(string msg, Exception e) : base(msg, e) { }
  }

  /// <summary>Data transaction exception.</summary>
  public class DataTransactionException : GeneralException {
    /// <summary>Ctor from inner exception <paramref name="e"/>.</summary>
    public DataTransactionException(Exception e) : base(e.Message, e) { }

    /// <summary>Ctor from <paramref name="msg"/> and inner exception <paramref name="e"/>.</summary>
    public DataTransactionException(string msg, Exception e) : base(msg, e) { }
  }

  /// <summary>Data concurrent persistence exception.</summary>
  public class DataConcurrentPersistenceException : DataPersistenceException {
    /// <summary>Ctor from inner exception <paramref name="e"/>.</summary>
    public DataConcurrentPersistenceException(Exception e) : base(e.Message, e) { }

    /// <summary>Ctor from <paramref name="msg"/> and inner exception <paramref name="e"/>.</summary>
    public DataConcurrentPersistenceException(string msg, Exception e) : base(msg, e) { }
  }

  /// <summary>Data entity not found exception.</summary>
  public class DataEntityNotFoundException : GeneralException {
    const string TMPL_MSG= "No valid {entity} with key: '{key}'";

    /// <summary>Ctor from <paramref name="ent"/> and <paramref name="key"/>.</summary>
    public DataEntityNotFoundException(string ent, object key) : base(ExceptionDataKey.ResolvedMsgParams(TMPL_MSG, out var tmpData, ent, key)) {
      this.SetMsgData(tmpData);
    }
    /// <summary>Ctor from <paramref name="ent"/>, <paramref name="key"/> and inner exception <paramref name="e"/>.</summary>
    public DataEntityNotFoundException(string ent, object key, Exception e) : base(ExceptionDataKey.ResolvedMsgParams(TMPL_MSG, out var tmpData, ent, key), e) {
      this.SetMsgData(tmpData);
    }
  }

  /// <summary>Data entity not found exception.</summary>
  public class DataEntityNotFoundException<T> : DataEntityNotFoundException {
    /// <summary>Ctor from <paramref name="key"/>.</summary>
    public DataEntityNotFoundException(object key) : base(typeof(T).Name, key) { }
    /// <summary>Ctor from <paramref name="key"/> and inner exception <paramref name="e"/>.</summary>
    public DataEntityNotFoundException(object key, Exception e) : base(typeof(T).Name, key, e) { }
  }

}