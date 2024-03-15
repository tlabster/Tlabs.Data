using System.Collections.Generic;

namespace Tlabs.Data.Entity {

  ///<summary>Insured person.</summary>
  public class Role : Intern.EditableEntity {
    ///<summary>Role name</summary>
    public string? Name { get; set; }
    ///<summary>Role description</summary>
    public string? Description { get; set; }
    ///<summary>Normalized role name</summary>
    public string? NormalizedRoleName { get; set; }
    ///<summary>Pattern containing a regex definition of the allowed paths for this role</summary>
    public string? AllowAccessPattern { get; set; }
    ///<summary>Pattern containing a regex definition of the denied paths for this role</summary>
    public string? DenyAccessPattern { get; set; }
    ///<summary>Pattern containing one or more enforced filters for API calls</summary>
    public string? EnforcedFilters { get; set; }
    ///<summary>Relationship with users</summary>
    public List<User.RoleRef>? Users { get; set; }
    ///<summary>Relationship with users</summary>
    public List<ApiKey.RoleRef>? ApiKeys { get; set; }
  }
}