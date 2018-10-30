using System;
using Tlabs.Data.Event;

namespace Tlabs.Data.Entity.Intern {

  ///<summary>Common base of an editiable entity.</summary>
  public abstract class EditableEntity : BaseEntity {

    static EditableEntity() {
      DataStoreEvent<EditableEntity>.Inserting+= setModified;
      DataStoreEvent<EditableEntity>.Updating+= setModified;
    }

    private static void setModified(Event.IEvent<EditableEntity> ev) {
      EditableEntity ent= ev.Entity;
      ent.Modified= App.TimeInfo.Now;
      var usr= (Identity.IIdentityAccessor)App.ServiceProv.GetService(typeof(Identity.IIdentityAccessor));
      ent.Editor= usr.Id;
    }

    ///<summary>Editor user id.</summary>
    public int Editor { get; set; }

    ///<summary>Last modified date.</summary>
    public DateTime Modified { get; set; }
  }

}