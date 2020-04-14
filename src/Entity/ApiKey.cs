
using System;
using System.Collections.Generic;
using Tlabs.Data.Entity.Intern;

namespace Tlabs.Data.Entity {
  ///<summary>Entity class for an API Key</summary>
  public class ApiKey : EditableEntity {
    ///<summary>Unique token name</summary>
    public string TokenName { get; set; }

    ///<summary>Hashed API Key</summary>
    public string Hash { get; set; }

    ///<summary>Description</summary>
    public string Description { get; set; }

    ///<summary>Valid from (inclusive)</summary>
    public DateTime ValidFrom { get; set; }

    ///<summary>Valid until (exclusive), unlimited if null</summary>
    public DateTime? ValidUntil { get; set; }

    ///<summary>Validity state of the API Key</summary>
    private Status currentState;

    ///<summary>Current validity status (informational).</summary>
    ///<remarks>
    /// After limiting the validity by setting <see cref="ValidUntil"/> to e.g. NOW, the status could also be set to 'deleted'.
    ///</remarks>
    public string ValidityState {
      get => currentState.ToString();
      set => Enum.TryParse<Status>(value, true, out currentState);
    }

    ///<summary>Validity status</summary>
    public enum Status {
      ///<summary>active</summary>
      ACTIVE,
      ///<summary>inactive, planed for deletion</summary>
      INACTIVE,
      ///<summary>deleted</summary>
      DELETED
    }

    ///<summary>Associated roles</summary>
    public List<RoleRef> Roles { get; set; }

    ///<summary>Reference between roles and API Keys</summary>
    public class RoleRef {

      ///<summary>Role</summary>
      public Role Role { get; set; }

      ///<summary>API Key</summary>
      public ApiKey ApiKey { get; set; }
    }
  }
}