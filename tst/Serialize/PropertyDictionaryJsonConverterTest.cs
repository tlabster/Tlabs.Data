using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

using Tlabs.Dynamic;

namespace Tlabs.Data.Serialize.Json.Tests {

  public class PropertyDictionaryJsonConverterTest {

    [Fact]
    public void BasicTest() {
      const int CNT= 5;
      var strm= JsonStreamEnumeratorTest.JsonStream<JsonStreamEnumeratorTest.SimpleTestClass>(CNT);

      var json= JsonFormat.CreateSerializer<IList<IDictionary<string, object>>>();
      var lst= json.LoadObj(strm);

      Assert.Equal(CNT, lst.Count);
      foreach (var dict in lst) {
        Assert.NotNull(dict["StrProp"]);
        Assert.True(object.ReferenceEquals(dict["StrProp"], dict["strProp"]));    //non case sesitive
        Assert.NotEmpty(dict["ListProp"] as IEnumerable);

        var dyn= new DynamicAccessor(typeof(JsonStreamEnumeratorTest.TestClass));
        var tstObj= new JsonStreamEnumeratorTest.TestClass();
        foreach (var prop in dict)
          dyn[prop.Key].Set(tstObj, prop.Value);

        Assert.Equal(dict["strProp"], tstObj.StrProp);
        Assert.Equal(((IList)dict["listProp"]).Count, tstObj.ListProp.Count);
        Assert.Equal(((IList)dict["aryProp"]).Count, tstObj.AryProp.Length);
      }
    }

    [Fact]
    public void NestedObjectTest() {
      var strm= new MemoryStream(Encoding.UTF8.GetBytes(NESTEDOBJ));

      var str= Encoding.UTF8.GetString(strm.ToArray());
      strm.Position= 0;

      var json= JsonFormat.CreateSerializer<IDictionary<string, IDictionary<string, object>>>();
      var mapMap= json.LoadObj(strm);

      Assert.NotEmpty(mapMap);
      Assert.Equal("2_1", mapMap["obj2"]["p2_1"]);
      Assert.IsAssignableFrom<IList>(mapMap["obj3"]["p3_1"]);
    }

    [Fact]
    public void TestReadOnlyDict() {
      string t = "{\"Dict\": {\"test\" : \"test1\", \"test2\": [1,2,3], \"1\": \"323\" }}";
      var json= JsonFormat.CreateSerializer<TestReadOnlyDictObj>();
      var obj= json.LoadObj(t);
      obj.ToString();
    }

    public class TestReadOnlyDictObj {
      public IReadOnlyDictionary<string, object> Dict { get; set; }
    }

    const string NESTEDOBJ= @"
{
  ""obj1"": { ""p1_1"": 1, ""p1_2"": 2},
  ""obj2"": { ""p2_1"": ""2_1"", ""p2_2"": 2.2},
  ""obj3"": { ""p3_1"": [1, 2, 3], ""p3_2"": ""3_2""}
}
    ";
  }
}