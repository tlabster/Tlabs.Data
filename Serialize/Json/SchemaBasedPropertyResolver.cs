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
  public class SchemaBasedPropertyResolver<S> : DefaultContractResolver where S : SerializationSchema {
    /// <summary>Schema to define how each attribute is to be serialized</summary>
    protected readonly S schema;

    ///<summary>Ctor from <paramref name="schema"/> and optional <paramref name="prevCr"/>.</summary>
    public SchemaBasedPropertyResolver(S schema, DefaultContractResolver prevCr= null ) : base() {
      this.schema= schema;
      if (null != prevCr)
        this.NamingStrategy= prevCr.NamingStrategy;
    }

    ///<inherit/>
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) {
      var props= base.CreateProperties(type, memberSerialization)
                     .Where(p => !schema.GetAttributes(p.DeclaringType, p.UnderlyingName).HasFlag(SerializationSchema.PropertyAttribs.Ignore) )
                     .ToList();
      return props;
    }
  }
}