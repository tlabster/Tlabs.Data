using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Tlabs.Misc;
using Tlabs.Config;

namespace Tlabs.Data.Serialize.Json {

  ///<summary>Json format serialization.</summary>
  public class JsonFormat {

    ///<summary>Json format default options.</summary>
    public static JsonSerializerOptions DefaultOptions { get; }
    // static readonly ILogger<JsonFormat> log= App.Logger<JsonFormat>();

    static JsonFormat() {
      DefaultOptions= new JsonSerializerOptions();
      ApplyDefaultOptions(DefaultOptions);
    }

    ///<summary>Apply Json format default options to <paramref name="opt"/>.</summary>
    public static void ApplyDefaultOptions(JsonSerializerOptions opt) {
      opt.IncludeFields= true;
      opt.DefaultIgnoreCondition= System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
      opt.ReadCommentHandling= JsonCommentHandling.Skip;
      opt.PropertyNameCaseInsensitive= true;
      opt.PropertyNamingPolicy= JsonNamingPolicy.CamelCase;
      opt.AllowTrailingCommas= true;
      opt.Converters.Add(new AppTimeJsonConverter());
      opt.Converters.Add(new PropertyDictionaryJsonConverter());
      opt.Converters.Add(new PropertyReadOnlyDictionaryJsonConverter());
      opt.WriteIndented= true;
    }

    ///<summary>Default ctor</summary>
    public JsonFormat() { }


    ///<summary>Create a <see cref="JsonFormat.Serializer{T}"/> for <typeparamref name="T"/>.</summary>
    public static Serializer<T> CreateSerializer<T>() => new Serializer<T>(Singleton<JsonFormat>.Instance);

    ///<summary>Create an untyped <see cref="DynamicSerializer"/>.</summary>
    public static DynamicSerializer CreateDynSerializer() => new DynamicSerializer(Singleton<JsonFormat>.Instance);

    ///<summary>Json format serializer for <typeparamref name="T"/>.</summary>
    public class Serializer<T> : ISerializer<T> {
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0052:Remove unread private member", Justification = "Might be needed in future")]
      readonly JsonFormat format;

      ///<summary>Ctor from <paramref name="format"/>.</summary>
      public Serializer(JsonFormat format) {
        this.format= format;
      }
      ///<inheritdoc/>
      public string Encoding => "Json";

      ///<inheritdoc/>
      public IEnumerable<T> LoadIEnumerable(Stream strm) => new JsonStreamEnumerator<T>(strm, DefaultOptions);

      ///<inheritdoc/>
      public T LoadObj(byte[] utf8Json) => JsonSerializer.Deserialize<T>(utf8Json, DefaultOptions);

      ///<inheritdoc/>
      public T LoadObj(Stream strm) => JsonSerializer.DeserializeAsync<T>(strm, DefaultOptions).AsTask().GetAwaiter().GetResult();

      ///<inheritdoc/>
      public T LoadObj(string text) => JsonSerializer.Deserialize<T>(text, DefaultOptions);

      // ///<inheritdoc/>
      // public void WriteIEnumerable(Stream strm, IEnumerable<T> itemsToSerialize, ElementCallback<T> callback) {
      //   throw new NotImplementedException();
      // }

      ///<inheritdoc/>
      public byte[] WriteObj(T obj) => JsonSerializer.SerializeToUtf8Bytes<T>(obj, DefaultOptions);

      ///<inheritdoc/>
      public void WriteObj(Stream strm, T obj) => JsonSerializer.SerializeAsync<T>(strm, obj, DefaultOptions).GetAwaiter().GetResult();
    }

    ///<summary>Json format serializer for dynamic types known only during runtime.</summary>
    public class DynamicSerializer : IDynamicSerializer {
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0052:Remove unread private member", Justification = "Might be needed in future")]
      readonly JsonFormat format;

      ///<summary>Ctor from <paramref name="format"/>.</summary>
      public DynamicSerializer(JsonFormat format) {
        this.format= format;
      }

      ///<inheritdoc/>
      public string Encoding => "Json";

      ///<inheritdoc/>
      public IEnumerable LoadIEnumerable(Stream strm) {
        throw new NotImplementedException();
      }

      ///<inheritdoc/>
      public object LoadObj(byte[] utf8Json, Type type) => JsonSerializer.Deserialize(utf8Json, type, DefaultOptions);

      ///<inheritdoc/>
      public object LoadObj(Stream strm, Type type) => JsonSerializer.DeserializeAsync(strm, type, DefaultOptions).AsTask().GetAwaiter().GetResult();

      ///<inheritdoc/>
      public object LoadObj(string text, Type type) => JsonSerializer.Deserialize(text, type, DefaultOptions);

      ///<inheritdoc/>
      public void WriteIEnumerable(Stream strm, IEnumerable itemsToSerialize, ElementCallback callback) {
        throw new NotImplementedException();
      }

      ///<inheritdoc/>
      public byte[] WriteObj(object obj) => JsonSerializer.SerializeToUtf8Bytes(obj, obj.GetType(), DefaultOptions);

      ///<inheritdoc/>
      public void WriteObj(Stream strm, object obj) => JsonSerializer.SerializeAsync(strm, obj, obj.GetType(), DefaultOptions).GetAwaiter().GetResult();

    }
    ///<summary>Json serializer configurator.</summary>
    public class Configurator : IConfigurator<IServiceCollection> {
      ///<summary>Add Json serializer to <paramref name="target"/>.</summary>
      public void AddTo(IServiceCollection target, IConfiguration cfg) {
        target.AddSingleton<JsonFormat>(Singleton<JsonFormat>.Instance);
        target.AddSingleton<IDynamicSerializer, JsonFormat.DynamicSerializer>();
        target.AddSingleton(typeof(ISerializer<>), typeof(JsonFormat.Serializer<>));
      }
    }

  }
}