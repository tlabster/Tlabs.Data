using Xunit;
using Tlabs.Data.Entity;
using System;

namespace Tlabs.Data.Entity.Tests {
  public class RoleTest {
    [Fact]
    public void RoleBasicTest() {
      var role = new Role {
        AllowAccessPattern= @"GET:api/v\d+/memberships($|/[^/]+/(details$|accounts$|legalentity$|legalentityproperties$)|/form/[^/]+)",
        EnforcedFilters= @"1>api/v10/memberships[#type=NGA-Outlet#ccy=NGN]"
      };

      Assert.False(role.AllowsAction("GET", "api/v10/vouchers"));
      Assert.True(role.AllowsAction("GET", "api/v10/memberships"));
      Assert.False(role.AllowsAction("PUT", "api/v10/memberships"));
      Assert.False(role.AllowsAction("GET", "api/v10/reports"));
      Assert.False(role.AllowsAction("PUT", "api/v10/usr"));

      Assert.Contains("type", role.ParamsForAction("api/v10/memberships").Values.Keys);
      Assert.Contains("NGA-Outlet", role.ParamsForAction("api/v10/memberships").Values.Values);
      Assert.Null(role.ParamsForAction("api/v10/exports"));

      role = new Role {
        AllowAccessPattern= @".*:.*",
        DenyAccessPattern= @"GET:api/v\d+/exports;.*:api/v\d+/usr"
      };
      Assert.True(role.AllowsAction("GET", "api/v10/vouchers"));
      Assert.True(role.AllowsAction("GET", "api/v10/memberships"));
      Assert.True(role.AllowsAction("GET", "api/v10/reports"));
      Assert.True(role.AllowsAction("PUT", "api/v10/reports"));
      Assert.False(role.AllowsAction("GET", "api/v10/exports"));
      Assert.True(role.AllowsAction("PUT", "api/v10/exports"));
      Assert.False(role.AllowsAction("GET", "api/v10/usr"));
      Assert.False(role.AllowsAction("PUT", "api/v10/usr"));
    }

    [Fact]
    public void RoleValidationTest() {
      var role= new Role {
        AllowAccessPattern= @"invalidPattern",
        DenyAccessPattern= @"invalidPattern",
        EnforcedFilters= @"invalidPattern"
      };
      Assert.Throws<InvalidOperationException>(() => role.AllowsAction("GET", "api/v10/test"));
      Assert.Throws<InvalidOperationException>(() => role.ParamsForAction("api/v10/test"));

      // Does not thow exception on null
      role= new Role {
        AllowAccessPattern= null,
        DenyAccessPattern= null,
        EnforcedFilters= null
      };
      role.AllowsAction("GET", "api/v10/test");
      role.ParamsForAction("api/v10/test");
    }
  }
}
