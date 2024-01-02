using System;

using Tlabs.Misc;

namespace Tlabs.Data.Event {

  ///<summary>Static class to trigger/raise data store events.</summary>
  public static class DataStoreEvent {
    private static readonly BasicCache<BaseTypePair, ITrigger> cache= new BasicCache<BaseTypePair, ITrigger>();

    ///<summary>Returns a <see ref="ITrigger"/> for given <paramref name="entityType"/>.</summary>
    public static ITrigger Trigger(Type entityType, Type baseType = null) {
      var tp= new BaseTypePair(entityType, baseType);
      ITrigger triggerCreator() {
        var triggerType = typeof(DataStoreEvent<>.Trigger<>).MakeGenericType(tp.EntType, tp.BaseType);
        return (ITrigger)Activator.CreateInstance(triggerType);
      }
      return cache[tp, triggerCreator ];
    }


    ///<summary>Interface of a data store trigger to raise events.</summary>
    public interface ITrigger {
      ///<summary>Raise (before) inserting <paramref name="e"/>> event.</summary>
      bool RaiseInserting(object e);
      ///<summary>Raise (before) updating <paramref name="e"/>> event.</summary>
      bool RaiseUpdating(object e, Func<object> obtainOrg);
      ///<summary>Raise (before) deleting <paramref name="e"/>> event.</summary>
      bool RaiseDeleting(object e, Func<object> obtainOrg);

      ///<summary>Raise (after) inserted <paramref name="e"/>> event.</summary>
      void RaiseInserted(object e);
      ///<summary>Raise (after) updated <paramref name="e"/>> event.</summary>
      void RaiseUpdated(object e);
      ///<summary>Raise (after) deleted <paramref name="e"/>> event.</summary>
      void RaiseDeleted(object e);

      ///<summary>Raise insert failed <paramref name="e"/>> event.</summary>
      bool RaiseInsertFailed(object e, Exception ex);
      ///<summary>Raise update failed <paramref name="e"/>> event.</summary>
      bool RaiseUpdateFailed(object e, Func<object> obtainOrg, Exception ex);
      ///<summary>Raise delete failed <paramref name="e"/>> event.</summary>
      bool RaiseDeleteFailed(object e, Func<object> obtainOrg, Exception ex);
    }

    struct BaseTypePair {
      public Type EntType, BaseType;
      public BaseTypePair(Type type, Type baseType) {
        if (null == (this.EntType= type)) throw new ArgumentNullException(nameof(type));
        this.BaseType= baseType ?? type;
      }
      public override readonly bool Equals(object o) {
        if (o == null || GetType() != o.GetType()) return false;
        var tp= (BaseTypePair)o;
        return EntType.Equals(tp.EntType) && BaseType.Equals(tp.BaseType);
      }

      public override readonly int GetHashCode() {
        return EntType.GetHashCode() ^ BaseType.GetHashCode();
      }
    }

  }
}
