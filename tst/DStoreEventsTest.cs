using System;
using System.Linq;

using Tlabs.Data.Entity;
using Tlabs.Data.Entity.Intern;

using Xunit;
using Xunit.Abstractions;

namespace Tlabs.Data.Event.Tests {
  public class StoreEventTest : IDisposable {
    ITestOutputHelper tstout;

    int sStrEventsCnt= 0;
    int objEventsCnt= 0;
    bool cancelObjEvent;
    Action<IBeforeEvent<string>> strEv;
    Action<IBeforeEvent<object>> objEv;

    public StoreEventTest(ITestOutputHelper tstout) {
      this.tstout= tstout;
      DataStoreEvent<string>.Inserting+= this.strEv= p => tstout.WriteLine($"str: #{++sStrEventsCnt} {p.Entity.GetType()} {p.Entity}");
      DataStoreEvent<object>.Inserting+= this.objEv= p => {
        tstout.WriteLine($"obj: #{++objEventsCnt} {p.Entity.GetType()} {p.Entity.ToString()}"); p.Cancel= cancelObjEvent;
      };
    }

    public void Dispose() {
      DataStoreEvent<string>.Inserting-= strEv;
      DataStoreEvent<object>.Inserting-= objEv;
    }

    [Fact]
    public void BasicDataStoreEventTest() {
      Assert.Equal(0, sStrEventsCnt + objEventsCnt);

      // while (!System.Diagnostics.Debugger.IsAttached) System.Threading.Thread.Sleep(500);
      var entity= "Hello action";
      DataStoreEvent.Trigger(entity.GetType()).RaiseInserting(entity);
      Assert.Equal(1, sStrEventsCnt);
      Assert.Equal(1, objEventsCnt);

      var objEnt= new object();
      DataStoreEvent.Trigger(objEnt.GetType()).RaiseInserting(objEnt);
      Assert.Equal(1, sStrEventsCnt);
      Assert.Equal(2, objEventsCnt);

      DataStoreEvent.Trigger(entity.GetType()).RaiseUpdating(entity, ()=> entity);
      DataStoreEvent.Trigger(entity.GetType()).RaiseDeleting(entity, () => entity);

      DataStoreEvent.Trigger(entity.GetType()).RaiseInserted(entity);
      DataStoreEvent.Trigger(entity.GetType()).RaiseDeleted(entity);
      DataStoreEvent.Trigger(entity.GetType()).RaiseUpdated(entity);

      DataStoreEvent.Trigger(entity.GetType()).RaiseInsertFailed(entity, new Exception());
      DataStoreEvent.Trigger(entity.GetType()).RaiseUpdateFailed(entity, () => entity, new Exception());
      DataStoreEvent.Trigger(entity.GetType()).RaiseDeleteFailed(entity, () => entity, new Exception());
    }

    [Fact]
    public void CancelledDataStoreEventTest() {
      Assert.Equal(0, sStrEventsCnt + objEventsCnt);

      // while (!System.Diagnostics.Debugger.IsAttached) System.Threading.Thread.Sleep(500);
      var entity= "Hello action";
      cancelObjEvent= true;
      DataStoreEvent.Trigger(entity.GetType()).RaiseInserting(entity);
      Assert.Equal(1, objEventsCnt);
      Assert.Equal(0, sStrEventsCnt); //not invoked after objEv cancelled
    }

  }
}
