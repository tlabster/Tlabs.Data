using System;

using Microsoft.Extensions.DependencyInjection;

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
      var usr= (Identity.IIdentityAccessor?)App.ServiceProv.GetService<Identity.IIdentityAccessor>();
      ent.Editor= string.IsNullOrEmpty(usr?.Name) ? "Anonymous" : usr.AuthenticationType + "/" + usr.Name; //Identity.IIdentityAccessor service could be missing
    }

    ///<summary>Editor user identifier.</summary>
    public string? Editor { get; set; }

    ///<summary>Last modified date.</summary>
    public DateTime Modified { get; set; }
  }

}