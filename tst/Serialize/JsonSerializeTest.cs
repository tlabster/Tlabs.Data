using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using Tlabs.Misc;

using Xunit;

namespace Tlabs.Data.Serialize.Tests {
  public class JsonSerializeTest {

    [Fact]
    public void BasicJsonSerializeTest() {
      IDynamicSerializer ser= Json.JsonFormat.CreateDynSerializer();

      var strm= new MemoryStream();
      var d0=  DateTime.Parse("2018-12-01T09:00:00.000000Z", null, System.Globalization.DateTimeStyles.RoundtripKind);
      var originalObj= new {
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

      TestClass deserializedObj= ser.LoadObj(strm, typeof(TestClass)) as TestClass;
      Assert.Equal(originalObj.StrProp, deserializedObj.StrProp);
      Assert.Equal(originalObj.NumProp, deserializedObj.NumProp);
      Assert.Equal(originalObj.DateProp, deserializedObj.DateProp);

      // De-serializes from UTC in application time zone
      TestClass deserializedFromUtc= ser.LoadObj("{\"dateProp\": \"1996-12-19T16:39:57.0000000Z\"}", typeof(TestClass)) as TestClass;
      var d=  DateTime.Parse("1996-12-19T16:39:57.000000Z", null, System.Globalization.DateTimeStyles.RoundtripKind);
      Assert.Equal(d, deserializedFromUtc.DateProp);

      // Serializes it back again into UTC TZ
      strm= new MemoryStream();
      ser.WriteObj(strm, deserializedFromUtc);
      strm.Position= 0;
      json= Encoding.UTF8.GetString(strm.ToArray());
      Assert.Contains("1996-12-19T16:39:57.0000000Z", json);

      // De-serializes from -06:00 in application time zone
      TestClass deserializedFromTZ= ser.LoadObj("{\"dateProp\": \"1996-12-19T16:39:57.0000000-06:00\"}", typeof(TestClass)) as TestClass;
      var d4=  DateTime.Parse("1996-12-19T22:39:57.0000000Z", null, System.Globalization.DateTimeStyles.RoundtripKind);
      Assert.Equal(App.TimeInfo.ToAppTime(d4), deserializedFromTZ.DateProp);

      // Allows timestamp
      TestClass deserializedFromTS= ser.LoadObj("{\"dateProp\": 1563197860271}", typeof(TestClass)) as TestClass;
      var d5=  DateTime.Parse("2019-07-15T13:37:40.2710000Z", null, System.Globalization.DateTimeStyles.RoundtripKind);
      Assert.Equal(Tlabs.App.TimeInfo.ToAppTime(d5), deserializedFromTS.DateProp);

      // Rejects only date
      Assert.Throws<JsonException>(() => ser.LoadObj("{\"dateProp\": \"2019-20-10\"}", typeof(TestClass)));
      Assert.Throws<JsonException>(() => ser.LoadObj("{\"dateProp\": 1.2E3}", typeof(TestClass)));
    }

    [Fact]
    public void ReadOnlySequenceDerializeTest() {
      var json= Json.JsonFormat.CreateSerializer<TestClassCover>();
      var d0=  DateTime.Parse("2018-12-01T09:00:00.000000Z", null, System.Globalization.DateTimeStyles.RoundtripKind);
      var obj= new TestClass {
        StrProp= $"abc: {new string('.', 4096 + 1234)}",
        NumProp= 2.718281828,
        DateProp= Tlabs.App.TimeInfo.ToAppTime(d0)
      };
      var cover= new TestClassCover { Data= EnumerableUtil.One(obj) };
      var strm= new MemoryStream();
      json.WriteObj(strm, cover);
      strm.Position= 0;

      using var buf= new SegmentSequenceBuffer(strm, 1789);
      while (!buf.IsEndOfStream) buf.Expand();
      var cover2= json.LoadObj(buf.Sequence);
      Assert.Equal(cover.Data.Single(), cover2.Data.Single());
    }

    [Fact]
    public void EnumerationTest() {
      var json= Json.JsonFormat.CreateSerializer<TestClassCover>();
      var strm = new MemoryStream();
      var cover= new TestClassCover { Data= new TestClassEnum() };
      json.WriteObj(strm, cover);
      var jsonStr= Encoding.UTF8.GetString(strm.ToArray());
      Assert.Contains("prop#001", jsonStr);
    }

    public class TestClass {
      public string StrProp { get; set; }
      public double NumProp { get; set; }
      public DateTime DateProp { get; set; }
      public override bool Equals(object obj) {
        if (obj is not TestClass other) return false;
        return    this.StrProp == other.StrProp
               && this.NumProp == other.NumProp
               && this.DateProp == other.DateProp;
      }

      public override int GetHashCode() {
        return HashCode.Combine(StrProp, NumProp, DateProp);
      }
    }

    public class TestClassCover {
      public IEnumerable<TestClass> Data;
    }

    public class TestClassEnum : IEnumerable<TestClass> {
      public IEnumerator<TestClass> GetEnumerator() {
        for (var l= 0; l <= 3; ++l) {
          yield return new TestClass {
            StrProp= $"prop#{l:D03}",
            NumProp= l,
            DateProp= App.TimeInfo.Now.AddDays(l),
          };
        }
      }

      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
  }
}
