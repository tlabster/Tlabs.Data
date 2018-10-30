using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Tlabs.Data.Serialize.Json {

  ///<summary>Converts a <see cref="DateTime"/> to and from a JS <c>Date.getTime()</c> (UTC).</summary>
  public class JsDateTimeConverter : JsonConverter {
    ///<summary>Check if <paramref name="objType"/> is <see cref="DateTime"/>.</summary>
    public override bool CanConvert(Type objType) {
      return typeof(DateTime) == objType || typeof(DateTime?) == objType;
    }

    ///<summary>Reads a integer value (from <c>Date.getTime()</c>) and converts this into a <see cref="DateTime"/> value.</summary>
    public override object ReadJson(JsonReader reader, Type objType, object existingValue, JsonSerializer serializer) {
      if (JsonToken.Null == reader.TokenType) { //handle null value
        if (typeof(DateTime?) != objType) throw new JsonSerializationException($"Can't convert null to {objType?.Name}.");
        return null;
      }

      if (JsonToken.StartConstructor == reader.TokenType)
        return readFromDateCtor(reader);

      if (JsonToken.Integer != reader.TokenType) throw new JsonSerializationException($"Can't convert {reader.TokenType} into {nameof(DateTime)}.");

      long jsMsec= (long)reader.Value;
      return jsMsec.FromJsMsecToDateTime();
    }

    private object readFromDateCtor(JsonReader reader) {
      DateTime dt;
      if ("Date" != reader.Value.ToString()) throw new JsonSerializationException($"Unexpected token or value when parsing date. (token: {reader.TokenType}, value: {reader.Value} )");
      reader.Read();

      dt=   (JsonToken.Integer == reader.TokenType)
          ? ((long)reader.Value).FromJsMsecToDateTime()
          : DateTime.Parse(reader.Value.ToString(), DateTimeFormatInfo.InvariantInfo); // assume date/time is given in application time-zone  //, DateTimeStyles.AssumeLocal).ToUniversalTime();

      reader.Read();
      if (JsonToken.EndConstructor != reader.TokenType) throw new JsonSerializationException($"Unexpected token {reader.TokenType} while parsing date.");

      // if (DateTimeKind.Utc != dt.Kind) throw new JsonSerializationException("UTC???");
      return dt;
    }

    ///<summary>Writes a <see cref="DateTime"/> value as integer value (like <c>Date.getTime()</c>).</summary>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
      var dt= (DateTime)value;
      writer.WriteValue(dt.JsTotalMsec());
    }
  }

}