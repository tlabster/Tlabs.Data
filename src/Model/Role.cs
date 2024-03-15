#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;

namespace Tlabs.Data.Model {

  public partial class Role {
    public Role() { }

    public Role(Tlabs.Data.Entity.Role r) {
      this.Modified= r.Modified;
      this.Key= r.Name;
      this.Description= r.Description;
      this.AllowedRoutes= r.AllowAccessPattern?.Split(';');
      this.DeniedRoutes= r.DenyAccessPattern?.Split(';');
      this.EnforcedFilters= r.EnforcedFilters?.Split(';');
    }

    public Tlabs.Data.Entity.Role CopyTo(Tlabs.Data.Entity.Role ent) {
      ent.Modified= default(DateTime) != this.Modified ? this.Modified : ent.Modified;
      ent.Name= this.Key ?? ent.Name;
      ent.Description= this.Description ?? ent.Description;
      ent.AllowAccessPattern= null != this.AllowedRoutes ? string.Join(";", this.AllowedRoutes) : ent.AllowAccessPattern;
      ent.DenyAccessPattern= null != this.DeniedRoutes ? string.Join(";", this.DeniedRoutes) : ent.DenyAccessPattern;
      ent.EnforcedFilters= null != this.EnforcedFilters ? string.Join(";", this.EnforcedFilters) : ent.EnforcedFilters;
      return ent;
    }
    public Tlabs.Data.Entity.Role AsEntity() {
      return new Tlabs.Data.Entity.Role {
        Modified= this.Modified,
        Name= this.Key,
        Description= this.Description,
        AllowAccessPattern= null != this.AllowedRoutes ? string.Join(";", this.AllowedRoutes) : null,
        DenyAccessPattern= null != this.DeniedRoutes ? string.Join(";", this.DeniedRoutes) : null,
        EnforcedFilters= null != this.EnforcedFilters ? string.Join(";", this.EnforcedFilters) : null,
      };
    }
    public DateTime Modified;
    public string? Key;
    public string? Description;
    public string[]? AllowedRoutes;
    public string[]? DeniedRoutes;
    public string[]? EnforcedFilters;

    ///<summary>Check if action is allowed</summary>
    public bool AllowsAction(string method, string route) {
      if (null == allowPolicies) {
        var alwPol= (AllowedRoutes ?? Enumerable.Empty<string>()).Select(r => new AccessPolicy(r)).ToList();
        Interlocked.CompareExchange(ref allowPolicies, alwPol, null);
      }
      if (null == denyPolicies) {
        var denPol= (DeniedRoutes ?? Enumerable.Empty<string>()).Select(r => new AccessPolicy(r)).ToList();
        Interlocked.CompareExchange(ref denyPolicies, denPol, null);
      }

      bool denied= null != denyPolicies && denyPolicies.Any(x => x.Matches(method, route));
      return !denied && allowPolicies.Any(x => x.Matches(method, route));
    }
    ///<summary>Returns the enforced parameters for an action</summary>
    public EnforcedParameter? ParamsForAction(string route) {
      if (null == enforcedParams) {
        var enfPar= (EnforcedFilters ?? Enumerable.Empty<string>()).Select(f => new EnforcedParameter(f)).ToList();
        Interlocked.CompareExchange(ref enforcedParams, enfPar, null);
      }
      return enforcedParams.FirstOrDefault(x => x.RouteRegex.IsMatch(route));
    }


    private List<AccessPolicy>? allowPolicies;
    private List<AccessPolicy>? denyPolicies;
    private List<EnforcedParameter>? enforcedParams;
    private static readonly Regex FILTER_REGEX= FILTERregex();
    [GeneratedRegex(@"^(?<position>\d)>(?<route>.+)\[(?<params>#.+)\]$")]
    private static partial Regex FILTERregex();

    ///<summary>Route access policy</summary>
    class AccessPolicy {
      public AccessPolicy(string route) {
        var components= route.Split(':');
        if (2 != components.Length) throw new FormatException($"Invalid access pattern '{route}'");
        this.Method= components[0];
        this.RouteRegex= new Regex(components[1], RegexOptions.Compiled);
      }

      ///<summary>Check if action is allowed</summary>
      public string Method { get; set; }
      ///<summary>Check if action is allowed</summary>
      public Regex RouteRegex { get; set; }
      ///<summary>Check if action is allowed</summary>
      public bool Matches(string method, string route) {
        if (!method.Equals(this.Method, StringComparison.OrdinalIgnoreCase) && !Regex.IsMatch(method, Method))
          return false;

        return RouteRegex.IsMatch(route);
      }
    }

    ///<summary>Enforced filter parameter</summary>
    public class EnforcedParameter {
      public EnforcedParameter(string pattern) {
        var enforcementParts= FILTER_REGEX.Match(pattern);
        if (!enforcementParts.Success || 4 != enforcementParts.Groups.Count) throw new FormatException($"Invalid enforced filter '{pattern}'");

        this.Position= Int32.Parse(enforcementParts.Groups["position"].Value, App.DfltFormat);
        this.RouteRegex= new Regex(enforcementParts.Groups["route"].Value);
        this.Values= new Dictionary<string, string>();

        foreach (var g in enforcementParts.Groups["params"].Value.Split('#')) {
          if (0 == g.Length) continue;
          var kv= g.Split("=");
          this.Values.Add(kv[0], kv[1]);
        }
      }
      ///<summary>Check if action is allowed</summary>
      public Regex RouteRegex { get; set; }
      ///<summary>Check if action is allowed</summary>
      public int Position { get; set; }
      ///<summary>Check if action is allowed</summary>
      public Dictionary<string, string> Values { get; set; }
    }
  }
}