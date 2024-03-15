using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Serialization;

using Tlabs.Misc;
namespace Tlabs.Data.Serialize.Xml {

  ///<summary>Enum with <see cref="System.Xml.Serialization.XmlEnumAttribute"/> helper.</summary>
  public static class XmlEnum {

    ///<summary>Try parse <paramref name="s"/> into enum with <paramref name="targetType"/> using <see cref="XmlEnumAttribute"/>.</summary>
    public static bool TryParse(string s, Type targetType, [MaybeNullWhen(false)] out Enum enm) {
      enm= default;
      if (null == s || !targetType.IsEnum) return false;
      if (Enum.TryParse(targetType, s, ignoreCase: true, out var o))
        enm= o as Enum;
      return null != enm;
    }

    ///<summary>Try parse <paramref name="s"/> into <paramref name="enm"/> using <see cref="XmlEnumAttribute"/>.</summary>
    public static bool TryParse<T>(string s, [MaybeNullWhen(false)] out T enm) where T : Enum {
      enm= default;
      if (null == s || !typeof(T).IsEnum) return false;
      return Singleton<EnumTraits<T>>.Instance.NamedValues.TryGetValue(s, out enm);
    }

    ///<summary>Transalte enum value into name from XmlEnumAttribute.</summary>
    public static string XmlEnumAttributeValue<T>(this T value) where T : Enum {
      var enumName= value?.ToString();
      if (null == enumName) return string.Empty;
      var member= typeof(T).GetMember(enumName).FirstOrDefault();
      var attribute= member?.GetCustomAttributes(typeof(XmlEnumAttribute), inherit: true)
                            .Cast<XmlEnumAttribute>()
                            .FirstOrDefault();
      return attribute?.Name ?? enumName;
    }
  }


  class EnumTraits<T> where T : Enum {
    public EnumTraits() {
      var enumValues= (T[])Enum.GetValues(typeof(T));
      this.NamedValues= enumValues.ToDictionary(e => e.XmlEnumAttributeValue(), StringComparer.OrdinalIgnoreCase);
    }
    public IReadOnlyDictionary<string, T> NamedValues { get; }
  }
}