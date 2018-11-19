using System;
using System.IO;
using System.Text;

using Xunit;

namespace Tlabs.Data.Serialize.Tests {
  public class JsonSerializeTest {
    public JsonSerializeTest() {
    }


    [Fact]
    public void BasicJsonSerializeTest() {
      IDynamicSerializer ser= Json.JsonFormat.CreateDynSerializer();
      var strm= new MemoryStream();
      dynamic obj= new {
        StrProp= "abc",
        NumProp= 2.718281828
      };
      ser.WriteObj(strm, obj);
      strm.Position= 0;
      var json= Encoding.UTF8.GetString(strm.ToArray());
      Assert.Contains("\"StrProp\"", json);
      Assert.Contains("\"NumProp\"", json);

      dynamic obj2= ser.LoadObj(strm, ((object)obj).GetType());
      Assert.Equal(obj.StrProp, obj2.StrProp);
      Assert.Equal(obj.NumProp, obj2.NumProp);
    }

  }
}
