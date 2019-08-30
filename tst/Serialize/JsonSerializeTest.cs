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
      dynamic obj= new {
        StrProp= "abc",
        NumProp= 2.718281828,
        DateProp= new DateTime(2018,12,01,10,0,0)
      };
      ser.WriteObj(strm, obj);
      strm.Position= 0;
      var json= Encoding.UTF8.GetString(strm.ToArray());
      Assert.Contains("\"strProp\"", json);
      Assert.Contains("\"numProp\"", json);
      Assert.Contains("\"dateProp\"", json);
      // Returns UTC
      Assert.Contains("2018-12-01T09:00:00.0000000Z", json);

      dynamic obj2= ser.LoadObj(strm, ((object)obj).GetType());
      Assert.Null(obj2.xyz_udefined);
      Assert.Equal(obj.StrProp, obj2.StrProp);
      Assert.Equal(obj.NumProp, obj2.NumProp);
      Assert.Equal(obj.DateProp, obj2.DateProp);

      // De-serializes in application time zone
      dynamic obj3= ser.LoadObj("{\"dateProp\": \"1996-12-19T16:39:57-08:00\"}", ((object)obj).GetType());
      Assert.Equal(new DateTime(1996, 12, 20, 01, 39, 57), obj3.DateProp);

      // Allows timestamp
      dynamic obj4= ser.LoadObj("{\"dateProp\": 1563197860271}", ((object)obj).GetType());
      Assert.Equal(new DateTime(1996, 12, 20, 01, 39, 57), obj3.DateProp);

      // Rejects only date
      Assert.Throws<JsonSerializationException>(() => ser.LoadObj("{\"dateProp\": \"2019-20-10\"}", ((object)obj).GetType()));
    }

  }
}
