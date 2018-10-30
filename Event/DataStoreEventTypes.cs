using System;

namespace Tlabs.Data.Event {

  ///<summary>Interface of a data store base event for an entity of <typeparamref name="T"/> .</summary>
  public interface IEvent<out T> {
    ///<summary>Event source entity.</summary>
    T Entity { get; }
  }

  ///<summary>Interface of a data store (after) change event for an entity of <typeparamref name="T"/>.</summary>
  public interface IChangeEvent<out T> : IEvent<T> {
    ///<summary>Event original (before change) source entity.</summary>
    T Original { get; }
  }

  ///<summary>Interface of a data store before event for an entity of <typeparamref name="T"/>.</summary>
  public interface IBeforeEvent<out T> : IEvent<T> {
    ///<summary>Set to true to cancel store operation.</summary>
    bool Cancel { get; set; }
  }

  ///<summary>Interface of a data store failed event for an entity of <typeparamref name="T"/>.</summary>
  public interface IFailedEvent<out T> : IEvent<T> {
    ///<summary>Failure exeption.</summary>
    Exception Exception { get; }
    ///<summary>Set to true to swallow (not throw) the failure exception.</summary>
    bool Swallow { get; set; }
  }

  ///<summary>Interface of a data store change failed event for an entity of <typeparamref name="T"/>.</summary>
  public interface IChangeFailedEvent<out T> : IFailedEvent<T>, IChangeEvent<T> { }

  ///<summary>Interface of a data store before change event for an entity of <typeparamref name="T"/>.</summary>
  public interface IBeforeChangeEvent<out T> : IBeforeEvent<T>, IChangeEvent<T> { }

}