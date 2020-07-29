using System;
using System.Collections.Generic;

namespace Tlabs.Data.Entity {
  ///<summary>User account profile.</summary>
  public class User : Intern.EditableEntity {
    ///<summary>Account state.</summary>
    public enum State : uint {
      ///<summary>ACTIVE (default)</summary>
      ACTIVE= 0,        //default ACTIVE
      ///<summary>DEACTIVATED (login disabled)</summary>
      DEACTIVATED= 1    //login disabled
    }

    private State state;

    ///<summary>Account status</summary>
    public string Status {
      get => state.ToString();
      set { if (!Enum.TryParse<State>(value, out this.state)) this.state= default(State); }
    }

    ///<summary>Username</summary>
    public string UserName { get; set; }

    ///<summary>Normalized username</summary>
    public string NormalizedUserName { get; set; }

    ///<summary>Optional pw hash for manual authentication</summary>
    public string PasswordHash { get; set; }

    ///<summary>Optional email address</summary>
    public string Email { get; set; }

    ///<summary>Normalized email</summary>
    public string NormalizedEmail { get; set; }

    ///<summary>Confirmed email</summary>
    public bool EmailConfirmed { get; set; }

    ///<summary>First name </summary>
    public string Firstname { get; set; }

    ///<summary>Last name</summary>
    public string Lastname { get; set; }

    ///<summary>Assigned user roles</summary>
    public List<RoleRef> Roles { get; set; }

    ///<summary>Number of failed login attempts</summary>
    public int AccessFailedCount { get; set; }

    ///<summary>User preferred locale</summary>
    public Locale Locale { get; set; }

    ///<summary>Role reference</summary>
    public class RoleRef {
      ///<summary>Assigned user role</summary>
      public Role Role { get; set; }

      ///<summary>Assigned user</summary>
      public User User { get; set; }
    }
  }
}