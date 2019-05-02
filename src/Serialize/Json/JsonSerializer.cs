using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Tlabs.Misc;
using Tlabs.Config;

namespace Tlabs.Data.Serialize.Json {

  ///<summary>Json format serialization.</summary>
  public class JsonFormat {

    ///<summary>Singleton Newton JsonSerializer.</summary>
    public sealed class NewtonJsonSingleton {
      ///<summary>Common settings.</summary>
      public static readonly JsonSerializerSettings Settings= BuildSettings();
      ///<summary>Build common settings.</summary>
      public static JsonSerializerSettings BuildSettings() {
        var s= new JsonSerializerSettings();
        var cr= new DefaultContractResolver();
        cr.NamingStrategy= new DefaultNamingStrategy();
        s.ContractResolver= cr;
        s.NullValueHandling= NullValueHandling.Ignore;
        s.Converters.Add(new JsDateTimeConverter());
        s.DateFormatHandling= DateFormatHandling.IsoDateFormat;
        s.ReferenceLoopHandling= ReferenceLoopHandling.Ignore;
        s.Formatting= Formatting.Indented;
        return s;
      }
      NewtonJsonSingleton() { } //private ctor
      ///<summary>Singleton instance</summary>
      public static JsonSerializer Instance {
        get { return Lazy.instance; }
      }
      class Lazy {
        static Lazy() { } //Explicit static ctor for *NOT* to marking type with beforefieldinit
        internal static readonly JsonSerializer instance= JsonSerializer.Create(Settings);
      }
    }

    ///<summary>Logger.</summary>
    protected ILogger<JsonFormat> log;
    ///<summary>Text encoding.</summary>
    protected static readonly Encoding encoding= new UTF8Encoding(false); //no BOM!
    ///<summary>internal JsonSerializer.</summary>
    protected JsonSerializer json;

    ///<summary>Default Ctor.</summary>
    public JsonFormat() {
      this.json= JsonFormat.NewtonJsonSingleton.Instance;   //use ONE central JsonSerializer !
      this.log= App.Logger<JsonFormat>();
    }

    ///<summary>Create a <see cref="JsonFormat.Serializer{T}"/> for <typeparamref name="T"/>.</summary>
    public static Serializer<T> CreateSerializer<T>() where T : class, new() => new JsonFormat.Serializer<T>(Singleton<JsonFormat>.Instance);

    ///<summary>Create an untyped <see cref="JsonFormat.DynamicSerializer"/>.</summary>
    public static DynamicSerializer CreateDynSerializer() => new JsonFormat.DynamicSerializer(Singleton<JsonFormat>.Instance);

    ///<summary>Json format serializer for <typeparamref name="T"/>.</summary>
    public class Serializer<T> : ISerializer<T> where T : class, new() {
      private JsonFormat format;

      ///<summary>Ctor from <paramref name="format"/>.</summary>
      public Serializer(JsonFormat format) {
        this.format= format;
      }

      ///<inherit/>
      public string Encoding => "Json";

      ///<summary>Load object from JSON <paramref name="strm"/>.</summary>
      public T LoadObj(Stream strm) {
        using (var sr= new StreamReader(strm, JsonFormat.encoding, true)) {
          using (var rd= new JsonTextReader(sr)) {
            return format.json.Deserialize<T>(rd);
          }
        }
      }

      ///<summary>Load object from JSON <paramref name="text"/>.</summary>
      public T LoadObj(string text) {
        using (var rd= new JsonTextReader(new StringReader(text))) {
          return format.json.Deserialize<T>(rd);
        }
      }

      ///<summary>Write object to JSON <paramref name="strm"/>.</summary>
      public void WriteObj(Stream strm, T obj) {
        using (var sw= new StreamWriter(strm, JsonFormat.encoding, 2*1024, true)) {
          using (var wr= new JsonTextWriter(sw)) {
            format.json.Serialize(wr, obj, typeof(T));
          }
        }
      }

      ///<inherit/>
      public IEnumerable<T> LoadIEnumerable(Stream stream) {
        using (var sr = new StreamReader(stream, JsonFormat.encoding, true)) {
          using (var rd = new JsonTextReader(sr)) {
            while (rd.Read()) {
              if (rd.TokenType == JsonToken.StartObject) {
                var deserializedItem= format.json.Deserialize<T>(rd);
                yield return deserializedItem;
              }
            }
          }
        }
      }

      /// <inherit/>
      public void WriteIEnumerable(Stream strm, IEnumerable<T> itemsToSerialize, ElementCallback<T> callback) {
        using (var sw = new StreamWriter(strm, JsonFormat.encoding, 2*1024, true)) {
          using (var wr = new JsonTextWriter(sw)) {
            wr.WriteStartArray();
            foreach (T item in itemsToSerialize) {
              format.json.Serialize(wr, callback(item), typeof(T));
            }
            wr.WriteEndArray();
          }
        }
      }

    } //class Serializer<T>

    ///<summary>Json format serializer for dynamic types known only during runtime.</summary>
    public class DynamicSerializer : IDynamicSerializer {
      private JsonFormat format;

      ///<summary>Ctor from <paramref name="format"/>.</summary>
      public DynamicSerializer(JsonFormat format) {
        this.format= format;
      }

      ///<inherit/>
      public string Encoding => "Json";

      ///<inherit/>
      public object LoadObj(Stream strm, Type type) {
        using (var sr = new StreamReader(strm, JsonFormat.encoding, true)) {
          using (var rd = new JsonTextReader(sr)) {
            return format.json.Deserialize(rd, type);
          }
        }
      }

      ///<inherit/>
      public object LoadObj(string text, Type type) {
        using (var rd = new JsonTextReader(new StringReader(text))) {
          return format.json.Deserialize(rd, type);
        }
      }

      ///<inherit/>
      public void WriteObj(Stream strm, object obj) {
        using (var sw = new StreamWriter(strm, JsonFormat.encoding, 2*1024, true)) {
          using (var wr = new JsonTextWriter(sw)) {
            format.json.Serialize(wr, obj, obj.GetType());
          }
        }
      }

      ///<inherit/>
      public IEnumerable LoadIEnumerable(Stream stream) {
        using (var sr = new StreamReader(stream, JsonFormat.encoding, true)) {
          using (var rd = new JsonTextReader(sr)) {
            while (rd.Read()) {
              if (rd.TokenType == JsonToken.StartObject) {
                var deserializedItem= format.json.Deserialize(rd);
                yield return deserializedItem;
              }
            }
          }
        }
      }

      /// <inherit/>
      public void WriteIEnumerable(Stream strm, IEnumerable itemsToSerialize, ElementCallback callback) {
        using (var sw = new StreamWriter(strm, JsonFormat.encoding, 2*1024, true)) {
          using (var wr = new JsonTextWriter(sw)) {
            wr.WriteStartArray();
            foreach(var item in itemsToSerialize) {
              format.json.Serialize(wr, callback(item));
            }
            wr.WriteEndArray();
          }
        }
      }
    } //class DynamicSerializer
  }

  ///<summary>Json serializer configurator.</summary>
  public class SerializationConfigurator : IConfigurator<IServiceCollection> {
    ///<summary>Add Json serializer to <paramref name="target"/>.</summary>
    public void AddTo(IServiceCollection target, IConfiguration cfg) {
      target.AddSingleton<JsonFormat>(Singleton<JsonFormat>.Instance);
      target.AddSingleton<IDynamicSerializer, JsonFormat.DynamicSerializer>();
      target.AddSingleton(typeof(ISerializer<>), typeof(JsonFormat.Serializer<>));
      //Do we need this: target.AddSingleton(typeof(JsonFormat.Serializer<>));
    }
  }

}