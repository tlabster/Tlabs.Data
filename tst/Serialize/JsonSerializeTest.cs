using System;
using System.IO;
using System.Text;
using DMProg.Test.Common;
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
        DateProp= new DateTime(2018,12,01)
      };
      ser.WriteObj(strm, obj);
      strm.Position= 0;
      var json= Encoding.UTF8.GetString(strm.ToArray());
      Assert.Contains("\"StrProp\"", json);
      Assert.Contains("\"NumProp\"", json);
      Assert.Contains("\"DateProp\"", json);
      Assert.Contains("1543618800000", json);

      dynamic obj2= ser.LoadObj(strm, ((object)obj).GetType());
      Assert.Equal(obj.StrProp, obj2.StrProp);
      Assert.Equal(obj.NumProp, obj2.NumProp);
      Assert.Equal(obj.DateProp, obj2.DateProp);
    }

  }
}
