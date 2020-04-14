using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using Tlabs.Misc;

namespace Tlabs.Data.Entity {

  ///<summary>Insured person.</summary>
  public class Role : Intern.EditableEntity {
    ///<summary>Role cache</summary>
    public static readonly BasicCache<string, Role> Cache= new BasicCache<string, Role>();
    ///<summary>Role name</summary>
    public string Name { get; set; }
    ///<summary>Role description</summary>
    public string Description { get; set; }
    ///<summary>Normalized role name</summary>
    public string NormalizedRoleName { get; set; }
    ///<summary>Pattern containing a regex definition of the allowed paths for this role</summary>
    public string AllowAccessPattern { get; set; }
    ///<summary>Pattern containing a regex definition of the denied paths for this role</summary>
    public string DenyAccessPattern { get; set; }
    ///<summary>Pattern containing one or more enforced filters for API calls</summary>
    public string EnforcedFilters { get; set; }
    ///<summary>Relationship with users</summary>
    public List<User.RoleRef> Users { get; set; }
    ///<summary>Relationship with users</summary>
    public List<ApiKey.RoleRef> ApiKeys { get; set; }
    ///<summary>Check if action is allowed</summary>
    public bool AllowsAction(string method, string route) {
      bool denied= null != denyPolicies && denyPolicies.Any(x => x.Matches(method, route));
      return !denied && allowPolicies.Any(x => x.Matches(method, route));
    }
    ///<summary>Returns the enforced parameters for an action</summary>
    public EnforcedParameter ParamsForAction(string route) {
      if (null == enforcedParams) return null;
      return enforcedParams.FirstOrDefault(x => x.RouteRegex.IsMatch(route));
    }
    private List<RoleAccessPolicy> allowPolicies => _allowPolicies ?? (_allowPolicies= policyFromPattern(AllowAccessPattern));
    private List<RoleAccessPolicy> _allowPolicies;
    private List<RoleAccessPolicy> denyPolicies => _denyPolicies ?? (_denyPolicies= policyFromPattern(DenyAccessPattern));
    private List<RoleAccessPolicy> _denyPolicies;
    private List<EnforcedParameter> enforcedParams => _enforcedParams ??  (_enforcedParams= paramsFromPattern(EnforcedFilters));
    private List<EnforcedParameter> _enforcedParams;
    private static readonly Regex FILTER_REGEX = new Regex(@"^(?<position>\d)>(?<route>.+)\[(?<params>#.+)\]$", RegexOptions.Compiled);
    private List<RoleAccessPolicy> policyFromPattern(string pattern) {
      if (null == pattern) return new List<RoleAccessPolicy>();

      var patterns= pattern.Split(";");

      return patterns.Select(x => {
        var components= x.Split(':');
        if (components.Count() != 2) throw new FormatException($"Invalid access pattern '{pattern}'");
        return new RoleAccessPolicy {
          Method= components[0],
          RouteRegex= new Regex(components[1], RegexOptions.Compiled)
        };
      }).ToList();
    }
    private List<EnforcedParameter> paramsFromPattern(string pattern) {
      if (null == pattern) return null;

      var patterns= pattern.Split(";");
      return patterns.Select(x => {
        var enforcementParts= FILTER_REGEX.Match(x);
        if (!enforcementParts.Success || enforcementParts.Groups.Count != 4) throw new FormatException($"Invalid enforced filter '{pattern}'");

        var param= new EnforcedParameter {
          Position= Int32.Parse(enforcementParts.Groups["position"].Value),
          RouteRegex= new Regex(enforcementParts.Groups["route"].Value),
        };
        param.Values= new Dictionary<string, string>();

        foreach (var g in enforcementParts.Groups["params"].Value.Split('#')) {
          if (!g.Any()) continue;
          var kv= g.Split("=");
          param.Values.Add(kv[0], kv[1]);
        }
        return param;
      }).ToList();
    }
    ///<summary>Check if action is allowed</summary>
    public class RoleAccessPolicy {
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

    ///<summary>Check if action is allowed</summary>
    public class EnforcedParameter {
      ///<summary>Check if action is allowed</summary>
      public Regex RouteRegex { get; set; }
      ///<summary>Check if action is allowed</summary>
      public int Position { get; set; }
      ///<summary>Check if action is allowed</summary>
      public Dictionary<string, string> Values { get; set; }
    }
  }
}