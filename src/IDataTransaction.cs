using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Tlabs.Data {

  ///<summary>Interface of an abstract data transaction.</summary>
  public interface IDataTransaction : IDisposable {

    ///<summary>Opaque (but unique) transaction id.</summary>
    object Id { get; }

    ///<summary>Commit all pending changes of this transaction.</summary>
    void Commit();
    ///<summary>Cancel (rollback) all pending changes of this transaction.</summary>
    void Cancel();
  }
}