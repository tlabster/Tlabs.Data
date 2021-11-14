using System.Buffers;
using System.Text.Json;

namespace Tlabs.Data.Serialize.Json {
  ///<summary><see cref="Utf8JsonReader"/> extension.</summary>
  public static class JsonReaderExtension {
    ///<summary>Return current token value as string.</summary>
    public static string TokStr(this ref Utf8JsonReader json)
      => System.Text.Encoding.UTF8.GetString(  json.HasValueSequence
                                             ? json.ValueSequence.ToArray()
                                             : json.ValueSpan.ToArray());
  }
}