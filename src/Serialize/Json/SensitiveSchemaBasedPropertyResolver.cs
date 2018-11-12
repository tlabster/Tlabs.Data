using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Tlabs.Data.Serialize.Json {

  ///<summary><see cref="ISerializationSchema{T}"/> based Json property resolver.</summary>
  public class SensitiveSchemaBasedPropertyResolver<S> : SchemaBasedPropertyResolver<S> where S : SerializationSchema {
    ///<summary>Ctor from <paramref name="schema"/> and optional <paramref name="prevCr"/>.</summary>
    public SensitiveSchemaBasedPropertyResolver(S schema, DefaultContractResolver prevCr= null ) : base(schema, prevCr) {}

    ///<inherit/>
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) {
      var properties = base.CreateProperties(type, memberSerialization);
      // For all properties that are marked as 'Sensible' with the schema
      // attach an SensibleDataValueProvider instance to them
      foreach (JsonProperty p in properties) {
        var ptype= p.DeclaringType;
        if (schema.GetAttributes(ptype, p.UnderlyingName).HasFlag(SerializationSchema.PropertyAttribs.Sensible))
          p.ValueProvider= new SensitiveDataValueProvider(ptype.GetProperty(p.UnderlyingName));
      }

       return properties;
    }

    private class SensitiveDataValueProvider : IValueProvider {
      PropertyInfo targetProperty;

      public SensitiveDataValueProvider(PropertyInfo targetProperty) {
        this.targetProperty = targetProperty;
      }

      // GetValue is called by Json.Net during serialization.
      // The target parameter has the object from which to read the unchanged string
      // the return value is a pseudonymised string that gets written to the JSON
      public object GetValue(object target) {
        object value= targetProperty.GetValue(target);

        if(value == null) {
          return null;
        }

        Random rnd= new Random();

        Type propertyType = targetProperty.PropertyType;

        if(propertyType == typeof(DateTime)) {
          var yearSpan= App.TimeInfo.Now.Year - 2014;
          return new DateTime(2014, 1, 1).AddDays(new Random().Next(365 * yearSpan));
        }
        if (propertyType == typeof(string))
          return targetProperty.Name + " " + rnd.Next(1000000000).ToString();

        return value;
      }

      // SetValue gets called by Json.Net during deserialization.
      // The value parameter has the encrypted value read from the JSON;
      // target is the object on which to set the decrypted value.
      public void SetValue(object target, object value) {
        targetProperty.SetValue(target, value);
      }
    }
  }
}