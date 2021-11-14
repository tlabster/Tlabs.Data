#pragma warning disable CS1591

using System;
using System.Collections.Generic;

namespace Tlabs.Data.Entity.Intern {

  public abstract class BaseDocument<T> : Intern.EditableEntity where T : BaseDocument<T> {

    public enum State : uint {
      DISABLED= 0,    // Document to be ignored (and the default State!)
      IMPLAUSIBLE= 1,
      VALID= 2
    }
    private State state;
    private BodyData docBody;
    private object cachedBodyObj;

    public virtual string Title { get; set; }
    public virtual string Summary { get; set; }
    ///<summary>Schema TypeId</summary>
    public string Sid { get; set; }
    public virtual string Status {
      get { return state.ToString(); }
      set {
        if (!Enum.TryParse<State>(value, out this.state)) this.state= default(State);
        Validated= State.VALID == state ? App.TimeInfo.Now : default(DateTime?);
      }
    }
    public string StatusDetails { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Validated { get; set; }
    public BodyData Body {
      get => docBody??= new BodyData { Document= (T)this };
      set => docBody= value;
    }
    public object GetBodyObject(Func<BodyData, object> loadObj) => cachedBodyObj ?? cacheBodyObj(loadObj(this.Body));

    public virtual object SetBodyObject(object bodyObj) => cacheBodyObj(bodyObj);

    private object cacheBodyObj(object bdy) => this.cachedBodyObj= bdy;

    //implicitly not mapped
    public bool IsValid {
      get { return State.VALID == state && null != Validated; } // && Validated >= this.Modified; }
    }

    public class BodyData : Intern.BaseEntity {
      private Enc enc;

      public T Document { get; set; }
      public byte[] Data { get; set; }
      public string Encoding {
        get => enc.ToString();
        set => Enc.TryParse<Enc>(value, true, out enc);
      }
 
      public enum Enc {
        Undefined = 0,
        Json,
        XML,
        BSON,
        ProtoBuf,
        JSV
      }

    }

  }

}