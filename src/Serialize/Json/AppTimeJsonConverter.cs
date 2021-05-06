using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tlabs.Data.Serialize.Json {
  ///<summary>Json serialization converter to handle convertion from/into AppTime.</summary>
  public class AppTimeJsonConverter : JsonConverter<DateTime> {
    ///<inheritdoc/>
    public override DateTime Read(ref Utf8JsonReader json, Type typeToConvert, JsonSerializerOptions options) {
      if (JsonTokenType.String == json.TokenType) {
        var txtVal= json.GetString();
        //if (DateTime.TryParseExact(txtVal, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
        if (DateTime.TryParse(txtVal, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
          return App.TimeInfo.ToAppTime(dt);
        throw EX.New<JsonException>("Can not convert '{txt}' into DateTime value.", txtVal);
      }

      if (JsonTokenType.Number == json.TokenType && json.TryGetInt64(out var jsmsec))
        return jsmsec.FromJsMsecToDateTime();
      
      throw EX.New<JsonException>("Unexpected token '{txt}' for DateTime value.", json.TokStr());
    }

    ///<inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateTime dateTimeValue, JsonSerializerOptions options)
      => writer.WriteStringValue(App.TimeInfo.ToUtc(dateTimeValue).ToString("O", CultureInfo.InvariantCulture));
  }
}