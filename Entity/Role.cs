#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tlabs.Data.Entity {

  ///<summary>Insured person.</summary>
  public class Role : Intern.EditableEntity {
    public enum RoleType {
      BASIC,
      ADMIN,
      PROTECTED
    }
    private static string defaultPaths => @"(^api/.*|^usr/)";

    public static Dictionary<RoleType, Regex> DefaultAuthorizedActions {
      get {
        return new Dictionary<RoleType, Regex> {
          { RoleType.BASIC, new Regex(defaultPaths, RegexOptions.Compiled) }
        };
      }
    }

    public static Dictionary<RoleType, Regex> DefaultDeniedActions {
      get {
        return new Dictionary<RoleType, Regex> {
          { RoleType.BASIC, null }
        };
      }
    }

    ///<summary>Role name</summary>
    public string Name { get; set; }

    ///<summary>Role description</summary>
    public string Description { get; set; }
    public string NormalizedRoleName { get; set; }

    ///<summary>Relationship with users</summary>
    public List<User.RoleRef> Users { get; set; }

    public static readonly IReadOnlyList<Role> DefaultRoles= new List<Role> {
      new Role() {
        Name= RoleType.ADMIN.ToString(),
        Description= "Administrator Role"
      },
      new Role() {
        Name= RoleType.BASIC.ToString(),
        Description= "Sachbearbeiter Role"
      },
      new Role() {
        Name= RoleType.PROTECTED.ToString(),
        Description= "Sachbearbeiter für Geschützten Personen"
      }
    };
  }
}