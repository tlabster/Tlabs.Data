using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Tlabs.Test.Common;
using Xunit;

namespace Tlabs.Data.Serialize.Tests {
  [Collection("MemoryDB")]
  public class JsonSerializeTest {
    private MemoryDBEnvironment appTimeEnv;
    public JsonSerializeTest(MemoryDBEnvironment appTimeEnvironment) {
      this.appTimeEnv= appTimeEnvironment;
    }


    [Fact]
    public void BasicJsonSerializeTest() {
      IDynamicSerializer ser= Json.JsonFormat.CreateDynSerializer();

      var strm= new MemoryStream();
      var d0=  DateTime.Parse("2018-12-01T09:00:00.000000Z", null, System.Globalization.DateTimeStyles.RoundtripKind);
      dynamic originalObj= new {
        StrProp= "abc",
        NumProp= 2.718281828,
        DateProp= Tlabs.App.TimeInfo.ToAppTime(d0)
      };
      ser.WriteObj(strm, originalObj);
      strm.Position= 0;
      var json= Encoding.UTF8.GetString(strm.ToArray());
      Assert.Contains("\"strProp\"", json);
      Assert.Contains("\"numProp\"", json);
      Assert.Contains("\"dateProp\"", json);
      // Returns UTC
      Assert.Contains("2018-12-01T09:00:00.0000000Z", json);

      dynamic deserializedObj= ser.LoadObj(strm, ((object)originalObj).GetType());
      Assert.Equal(originalObj.StrProp, deserializedObj.StrProp);
      Assert.Equal(originalObj.NumProp, deserializedObj.NumProp);
      Assert.Equal(originalObj.DateProp, deserializedObj.DateProp);

      // De-serializes from UTC in application time zone
      dynamic deserializedFromUtc= ser.LoadObj("{\"dateProp\": \"1996-12-19T16:39:57.0000000Z\"}", ((object)originalObj).GetType());
      var d=  DateTime.Parse("1996-12-19T17:39:57.000000Z", null, System.Globalization.DateTimeStyles.RoundtripKind);
      Assert.Equal(d, deserializedFromUtc.DateProp);

      // Serializes it back again into UTC TZ
      strm= new MemoryStream();
      ser.WriteObj(strm, deserializedFromUtc);
      strm.Position= 0;
      json= Encoding.UTF8.GetString(strm.ToArray());
      Assert.Contains("1996-12-19T16:39:57.0000000Z", json);

      // De-serializes from -06:00 in application time zone
      dynamic deserializedFromTZ= ser.LoadObj("{\"dateProp\": \"1996-12-19T16:39:57.000000-06:00\"}", ((object)originalObj).GetType());
      var d4=  DateTime.Parse("1996-12-19T22:39:57.000000Z", null, System.Globalization.DateTimeStyles.RoundtripKind);
      Assert.Equal(App.TimeInfo.ToAppTime(d4), deserializedFromTZ.DateProp);

      // Allows timestamp
      dynamic deserializedFromTS= ser.LoadObj("{\"dateProp\": 1563197860271}", ((object)originalObj).GetType());
      var d5=  DateTime.Parse("2019-07-15T13:37:40.2710000Z", null, System.Globalization.DateTimeStyles.RoundtripKind);
      Assert.Equal(Tlabs.App.TimeInfo.ToAppTime(d5), deserializedFromTS.DateProp);

      // Rejects only date
      Assert.Throws<JsonSerializationException>(() => ser.LoadObj("{\"dateProp\": \"2019-20-10\"}", ((object)originalObj).GetType()));
    }

  }
}
