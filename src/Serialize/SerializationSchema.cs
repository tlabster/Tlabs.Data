﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Tlabs.Data.Serialize {

  ///<summary>Serialization schema with special property attributes of type <typeparamref name="T"/>.</summary>
  public interface ISerializationSchema<T> {
    ///<summary>Return property attributes.</summary>
    T GetAttributes(Type declType, string name);
  }

  ///<summary>Serialization schema to define special property attributes.</summary>
  public class SerializationSchema : ISerializationSchema<SerializationSchema.PropertyAttribs> {
    ///<summary>Serialization schema property attributes.</summary>
    [Flags]
    public enum PropertyAttribs {
      ///<summary>No attribute.</summary>
      None = 0x00,
      ///<summary>property to be ignored.</summary>
      Ignore = 0x01,
      ///<summary>property with sensible data.</summary>
      Sensible = 0x02
    }

    ///<summary>Property attribute setter helper.</summary>
    public struct PropAttribSetter<T> {
      ///<summary>Property map.</summary>
      public Dictionary<string, PropertyAttribs>? pMap;
      ///<summary>Attribute setter.</summary>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0251:Member can be made 'readonly'", Justification = "Setter not readonly.")]
      public PropAttribSetter<T> Set<P>(Expression<Func<T, P>> ex, PropertyAttribs attr) {
        if (ex.Body is not MemberExpression prop) throw new ArgumentException("No member access expression");
        if (null == pMap) throw new InvalidOperationException("No declaring type specified");
        pMap[prop.Member.Name]= attr;
        return this;
      }
    }

    readonly Dictionary<Type, Dictionary<string, PropertyAttribs>> typeMap= new();

    ///<summary>Specify properties declaring type.</summary>
    public PropAttribSetter<T> For<T>() {
      PropAttribSetter<T> setter;
      if (!this.typeMap.TryGetValue(typeof(T), out setter.pMap))
        setter.pMap= typeMap[typeof(T)]= new Dictionary<string, PropertyAttribs>();
      return setter;
    }
    ///<inheritdoc/>
    public PropertyAttribs GetAttributes(Type declType, string name) {
      var attr= PropertyAttribs.None;
      if (this.typeMap.TryGetValue(declType, out var pMap))
        pMap.TryGetValue(name, out attr);
      return attr;
    }
  }

}