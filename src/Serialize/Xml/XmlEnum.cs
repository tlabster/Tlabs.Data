using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

using Tlabs.Misc;
namespace Tlabs.Data.Serialize.Xml {

  ///<summary>Enum with <see cref="System.Xml.Serialization.XmlEnumAttribute"/> helper.</summary>
  public static class XmlEnum {
    internal static readonly LookupTable<Type, IReadOnlyDictionary<string, Enum>> enumMap= new (t => Enum.GetValues(t).Cast<Enum>().ToDictionary(e => e.XmlEnumAttributeValue()));

    ///<summary>Try parse <paramref name="s"/> into enum with <paramref name="targetType"/> using <see cref="XmlEnumAttribute"/>.</summary>
    public static bool TryParse(string s, Type targetType, out Enum enm) {
      lock (enumMap) {
        enm= null;
        if (! targetType.IsEnum || null == s) return false;
        if (enumMap[targetType].TryGetValue(s, out enm)) return true;
        if (Enum.TryParse(targetType, s, ignoreCase: true, out var o)) {
          enm= o as Enum;
          return true;
        }
        return false;
      }
    }

    ///<summary>Try parse <paramref name="s"/> into <paramref name="enm"/> using <see cref="XmlEnumAttribute"/>.</summary>
    public static bool TryParse<T>(string s, out T enm) where T : Enum {
      lock (enumMap) {
        Enum e;
        var t= typeof(T);
        var ret= TryParse(s, t, out e);
        enm= (T)(e ?? default(T));
        return ret;
      }
    }

    ///<summary>Transalte enum value into name from XmlEnumAttribute.</summary>
    public static string XmlEnumAttributeValue<T>(this T value) where T : Enum {
      var enumName= value?.ToString();
      if (null == enumName) return string.Empty;
      var member= value.GetType().GetMember(enumName).FirstOrDefault();
      var attribute= member.GetCustomAttributes(typeof(XmlEnumAttribute), inherit: true)
                           .Cast<XmlEnumAttribute>()
                           .FirstOrDefault();
      return attribute?.Name ?? enumName;
    }
  }


}