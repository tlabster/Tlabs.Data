using System;
using System.Reflection;

using Tlabs.Misc;

namespace Tlabs.Data.Event {

  ///<summary>Static class to register on data store events.</summary>
  ///<remarks>
  /// Note: Event handlers registered on an entity base class are also being fired for any derived entity.
  ///</remarks>
  ///<example>
  /// Register for 'before' events to supplement or validate an entity object before it is getting stored:
  /// <code>
  /// DataStoreEvent&lt;MyEntity&gt;.Updating+= e => e.Entity.Modified= AppGlobals.TimeInfo.Now;
  /// </code>
  ///</example>
  public static class DataStoreEvent<T> {

    private static Action<IBeforeEvent<T>> insertingEnv;
    private static Action<IBeforeChangeEvent<T>> updatingEnv;
    private static Action<IBeforeChangeEvent<T>> deletingEnv;

    private static Action<IEvent<T>> insertedEnv;
    private static Action<IEvent<T>> updatedEnv;
    private static Action<IEvent<T>> deletedEnv;

    private static Action<IFailedEvent<T>> insertFailedEnv;
    private static Action<IChangeFailedEvent<T>> updateFailedEnv;
    private static Action<IChangeFailedEvent<T>> deleteFailedEnv;

    #region Before events
    ///<summary>Event raised (before) inserting entity.</summary>
    public static event Action<IBeforeEvent<T>> Inserting {
      add { insertingEnv+= value.Invoke; }
      remove { insertingEnv-= value.Invoke; }
    }

    ///<summary>Event raised (before) updating entity.</summary>
    public static event Action<IBeforeChangeEvent<T>> Updating {
      add { updatingEnv+= value.Invoke; }
      remove { updatingEnv-= value.Invoke; }
    }

    ///<summary>Event raised (before) deleting entity.</summary>
    public static event Action<IBeforeChangeEvent<T>> Deleting {
      add { deletingEnv+= value.Invoke; }
      remove { deletingEnv-= value.Invoke; }
    }
    #endregion

    #region After events
    ///<summary>Event raised (after) entity inserted.</summary>
    public static event Action<IEvent<T>> Inserted {
      add { insertedEnv+= value.Invoke; }
      remove { insertedEnv-= value.Invoke; }
    }

    ///<summary>Event raised (after) entity updated.</summary>
    public static event Action<IEvent<T>> Updated {
      add { updatedEnv+= value.Invoke; }
      remove { updatedEnv-= value.Invoke; }
    }

    ///<summary>Event raised (after) entity deleted.</summary>
    public static event Action<IEvent<T>> Deleted {
      add { deletedEnv+= value.Invoke; }
      remove { deletedEnv-= value.Invoke; }
    }
    #endregion

    #region Failed events
    ///<summary>Event raised when entity insert failed.</summary>
    public static event Action<IFailedEvent<T>> InsertFailed {
      add { insertFailedEnv+= value.Invoke; }
      remove { insertFailedEnv-= value.Invoke; }
    }

    ///<summary>Event raised when entity update failed.</summary>
    public static event Action<IChangeFailedEvent<T>> UpdatFailed {
      add { updateFailedEnv+= value.Invoke; }
      remove { updateFailedEnv-= value.Invoke; }
    }

    ///<summary>Event raised when entity delete failed.</summary>
    public static event Action<IChangeFailedEvent<T>> DeleteFailed {
      add { deleteFailedEnv+= value.Invoke; }
      remove { deleteFailedEnv-= value.Invoke; }
    }
    #endregion

    #region inner event impl.
    class Event : IEvent<T> {
      protected T entity;
      public Event(T entity) { this.entity= entity; }
      public T Entity => this.entity;
    }

    class ChangeEvent : Event, IChangeEvent<T> {
      protected Lazy<T> lazyOrig;

      public ChangeEvent(T ent, Func<T> resOrg) : base(ent) {
        this.lazyOrig= new Lazy<T>(() => (T)resOrg());
      }
      public T Original => lazyOrig.Value;
    }

    class BeforeEvent : Event, IBeforeEvent<T> {
      private bool cancel;
      public BeforeEvent(T entity) : base(entity) { }
      public bool Cancel {
        get => cancel;
        set => cancel= value;
      }
    }

    class BeforeChangeEvent : ChangeEvent, IBeforeChangeEvent<T> {
      private bool cancel;
      public BeforeChangeEvent(T entity, Func<T> resOrg) : base(entity, resOrg) { }
      public bool Cancel {
        get => cancel;
        set => cancel= value;
      }
    }

    class FailedEvent : Event, IFailedEvent<T> {
      protected Exception ex;
      protected bool swallow;
      public FailedEvent(T ent, Exception ex) : base(ent) {
        this.ex= ex;
      }
      public Exception Exception => ex;
      public bool Swallow {
        get => swallow;
        set => swallow= value;
      }
    }

    class ChangeFailedEvent : FailedEvent, IChangeFailedEvent<T> {
      protected Lazy<T> lazyOrg;
      public ChangeFailedEvent(T ent, Func<T> resolveOrg, Exception ex) : base(ent, ex) {
        this.lazyOrg= new Lazy<T>(() => (T)resolveOrg());
      }
      public T Original => lazyOrg.Value;
    }

    #endregion

    #region inner Invoker
    internal class Trigger<B> : DataStoreEvent.ITrigger {
      public bool RaiseInserting(object entity) {
        bool cancelled= (baseTrigger?.RaiseInserting(entity) ?? false);
        if (!cancelled) {
          var ev= new BeforeEvent((T)entity);
          DataStoreEvent<B>.insertingEnv?.Invoke((IBeforeEvent<B>)ev);
          cancelled= ev.Cancel;
        }
        return cancelled;
      }

      public bool RaiseUpdating(object entity, Func<object> obtainOrg) {
        bool cancelled= (baseTrigger?.RaiseUpdating(entity, obtainOrg) ?? false);
        if (!cancelled) {
          Func<T> resOrg= () => (T)obtainOrg();
          var ev= new BeforeChangeEvent((T)entity, resOrg);
          DataStoreEvent<B>.updatingEnv?.Invoke((IBeforeChangeEvent<B>)ev);
          cancelled= ev.Cancel;
        }
        return cancelled;
      }

      public bool RaiseDeleting(object entity, Func<object> obtainOrg) {
        bool cancelled= (baseTrigger?.RaiseDeleting(entity, obtainOrg) ?? false);
        if (!cancelled) {
          Func<T> resOrg= () => (T)obtainOrg();
          var ev= new BeforeChangeEvent((T)entity, resOrg);
          DataStoreEvent<B>.deletingEnv?.Invoke((IBeforeChangeEvent<B>)ev);
          cancelled= ev.Cancel;
        }
        return cancelled;
      }

      public void RaiseInserted(object entity) {
        var entityT= (T)entity;
        baseTrigger?.RaiseInserted(entityT);
        DataStoreEvent<B>.insertedEnv?.Invoke((IEvent<B>)new BeforeEvent(entityT));
      }

      public void RaiseUpdated(object entity) {
        baseTrigger?.RaiseUpdated(entity);
        DataStoreEvent<B>.updatedEnv?.Invoke((IEvent<B>)new Event((T)entity));
      }

      public void RaiseDeleted(object entity) {
        baseTrigger?.RaiseDeleted(entity);
        DataStoreEvent<B>.deletedEnv?.Invoke((IEvent<B>)new Event((T)entity));
      }

      public bool RaiseInsertFailed(object entity, Exception ex) {
        var entityT= (T)entity;
        bool wasSwallowed= (baseTrigger?.RaiseInsertFailed(entityT, ex) ?? false);
        var ev= new FailedEvent(entityT, ex);
        DataStoreEvent<B>.insertedEnv?.Invoke((IFailedEvent<B>)ev);
        return wasSwallowed || ev.Swallow;
      }

      public bool RaiseUpdateFailed(object entity, Func<object> obtainOrg, Exception ex) {
        bool wasSwallowed= (baseTrigger?.RaiseUpdateFailed(entity, obtainOrg, ex) ?? false);
        if (!wasSwallowed) {
          var ev= new ChangeFailedEvent((T)entity, (() => (T)obtainOrg()), ex);
          DataStoreEvent<B>.updateFailedEnv?.Invoke((IChangeFailedEvent<B>)ev);
          wasSwallowed= ev.Swallow;
        }
        return wasSwallowed;
      }

      public bool RaiseDeleteFailed(object entity, Func<object> obtainOrg, Exception ex) {
        bool wasSwallowed= (baseTrigger?.RaiseDeleteFailed(entity, obtainOrg, ex) ?? false);
        if (!wasSwallowed) {
          var ev= new ChangeFailedEvent((T)entity, (() => (T)obtainOrg()), ex);
          DataStoreEvent<B>.deleteFailedEnv?.Invoke((IChangeFailedEvent<B>)ev);
          wasSwallowed= ev.Swallow;
        }
        return wasSwallowed;
      }

      private DataStoreEvent.ITrigger baseTrigger {
        get {
          Type baseType= typeof(B).GetTypeInfo().BaseType;
          if (null == baseType) return null;
          return DataStoreEvent.Trigger(typeof(T), baseType);
        }
      }
    }
    #endregion

  }//class DataStoreEvent<T>

}
