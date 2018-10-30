using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Tlabs.Config;
using System;

namespace Tlabs.Data.Serialize.Json {

  ///<summary>Json schema based serialization format.</summary>
  public class JsonSchemaFormat<S> : JsonFormat where S : SerializationSchema {

    ///<summary>Create an untyped <see cref="JsonFormat.DynamicSerializer"/>.</summary>
    public static SchemaSerializer CreateSerializer(S schema) => new SchemaSerializer(new JsonSchemaFormat<S>(schema));

    ///<summary>Ctor from contract-resolver.</summary>
    public JsonSchemaFormat(S schema) {
      var settings= JsonFormat.NewtonJsonSingleton.BuildSettings();
        settings.ContractResolver= new SchemaBasedPropertyResolver<S>(schema, settings.ContractResolver as DefaultContractResolver);

      this.json= JsonSerializer.Create(settings);
    }

    ///<summary>Json format schema based serializer.</summary>
    public class SchemaSerializer : JsonFormat.DynamicSerializer {
      ///<summary>Ctor from <paramref name="format"/>.</summary>
      public SchemaSerializer(JsonSchemaFormat<S> format) : base(format) { }
    }
  }

  ///<summary>Json schema based serialization format, with sensitive attribute pseudonymisation</summary>
  public class SensitiveJsonSchemaFormat<S> : JsonFormat where S : SerializationSchema {
    ///<summary>Ctor from contract-resolver.</summary>
    public SensitiveJsonSchemaFormat(S schema, ILogger<JsonFormat> log) : base() {
      var settings= JsonFormat.NewtonJsonSingleton.BuildSettings();
        settings.ContractResolver= new SensitiveSchemaBasedPropertyResolver<S>(schema, settings.ContractResolver as DefaultContractResolver);

      this.json= JsonSerializer.Create(settings);
    }

    ///<summary>Json format schema based serializer.</summary>
    public class SchemaSerializer : JsonFormat.DynamicSerializer {
      ///<summary>Ctor from <paramref name="format"/>.</summary>
      public SchemaSerializer(SensitiveJsonSchemaFormat<S> format) : base(format) { }
    }
  }
}