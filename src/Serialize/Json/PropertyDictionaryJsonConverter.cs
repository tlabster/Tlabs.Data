using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tlabs.Data.Serialize.Json {
  ///<summary>Converter to add support for deserialization into a <c>IDictionary&lt;string, object></c>.</summary>
  public class PropertyDictionaryJsonConverter : BasePropDictionaryConverter<IDictionary<string, object?>> {
    ///<inheritdoc/>
    public override bool CanConvert(Type type) => typeof(IDictionary<string, object?>).IsAssignableFrom(type);
  }

  ///<summary>Converter to add support for deserialization into a <c>IReadOnlyDictionary&lt;string, object></c>.</summary>
  public class PropertyReadOnlyDictionaryJsonConverter : BasePropDictionaryConverter<IReadOnlyDictionary<string, object>> {
    ///<inheritdoc/>
    public override bool CanConvert(Type type) => typeof(IReadOnlyDictionary<string, object?>).IsAssignableFrom(type);
  }

  ///<summary>Converter to add support for deserialization into a <c>IDictionary&lt;string, object></c>.</summary>
  [SuppressMessage("performance", "CA1859")]
  public abstract class BasePropDictionaryConverter<T> : JsonConverter<T> {
    ///<inheritdoc/>
    public override T Read(ref Utf8JsonReader json, Type typeToConvert, JsonSerializerOptions options) {
      if (JsonTokenType.StartObject != json.TokenType) throw EX.New<JsonException>("Unexpected token '{tok}' at object start", json.TokStr());

      IDictionary<string, object?> dict= new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
      while (json.Read()) {
        if (JsonTokenType.EndObject == json.TokenType) return (T)dict;

        if (JsonTokenType.PropertyName != json.TokenType) throw EX.New<JsonException>("Token '{tok}' is no valid property name", json.TokStr());
        var propName= json.GetString();
        if (string.IsNullOrWhiteSpace(propName)) throw EX.New<JsonException>("No valid property name: ''");
        json.Read();
        dict.Add(propName, getObject(ref json, options));
      }
      throw EX.New<JsonException>("Missing token '}'");
    }

    object? getObject(ref Utf8JsonReader json, JsonSerializerOptions options)
      => json.TokenType switch {
        JsonTokenType.String      => json.TryGetDateTime(out var dt) ? dt : json.GetString(),
        JsonTokenType.Number      => json.TryGetInt64(out var i64) ? i64 : json.GetDecimal(),
        JsonTokenType.True        => true,
        JsonTokenType.False       => false,
        JsonTokenType.StartObject => this.Read(ref json, typeof(IDictionary<string, object>), options),
        JsonTokenType.StartArray  => getArray(ref json, options),
        JsonTokenType.Null        => null,
        _                         => throw EX.New<JsonException>("Invalid token: '{tok}'", json.TokStr())
      };

    List<object?> getArray(ref Utf8JsonReader json, JsonSerializerOptions options) {
      var lst= new List<object?>();
      while (json.Read() && JsonTokenType.EndArray != json.TokenType)
        lst.Add(getObject(ref json, options));
      return lst;
    }

    ///<inheritdoc/>
    public override void Write(Utf8JsonWriter writer, T dict, JsonSerializerOptions options)
      => JsonSerializer.Serialize(writer, dict, removeThisConverter(options));  //remove our self from an options copy to avoid getting called recursively...

    JsonSerializerOptions removeThisConverter(JsonSerializerOptions options) {
      var retOpt= options;
      var cnv= options.Converters;
      for (int l= 0, n= cnv.Count; l < n; ++l) if (cnv[l].GetType() == this.GetType()) {
        if (retOpt == options) retOpt= new JsonSerializerOptions(options); //create copy
        retOpt.Converters.RemoveAt(l);
      }
      return retOpt;
    }
  }
}