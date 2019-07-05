using System;

namespace Tlabs.Data {

  /// <summary>Data persistence exception.</summary>
  public class DataPersistenceException : GeneralException {
    /// <summary>Ctor from inner exception <paramref name="e"/>.</summary>
    public DataPersistenceException(Exception e) : base(e.Message, e) { }

    /// <summary>Ctor from <paramref name="msg"/> and inner exception <paramref name="e"/>.</summary>
    public DataPersistenceException(string msg, Exception e) : base(msg, e) { }
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
    static string notFoundMsg(string ent, string key) => $"No valid {ent} with key: '{key}'";
    /// <summary>Ctor from <paramref name="ent"/> and <paramref name="key"/>.</summary>
    public DataEntityNotFoundException(string ent, string key) : base(notFoundMsg(ent, key)) { }
    /// <summary>Ctor from <paramref name="ent"/>, <paramref name="key"/> and inner exception <paramref name="e"/>.</summary>
    public DataEntityNotFoundException(string ent, string key, Exception e) : base(notFoundMsg(ent, key), e) { }
  }

  /// <summary>Data entity not found exception.</summary>
  public class DataEntityNotFoundException<T> : DataEntityNotFoundException {
    /// <summary>Ctor from <paramref name="key"/>.</summary>
    public DataEntityNotFoundException(string key) : base(typeof(T).Name, key) { }
    /// <summary>Ctor from <paramref name="key"/> and inner exception <paramref name="e"/>.</summary>
    public DataEntityNotFoundException(string key, Exception e) : base(typeof(T).Name, key, e) { }
  }

}