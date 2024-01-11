using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Tlabs.Data.Serialize.Json.Tests {
  public class JsonStreamEnumeratorTest {
    const int JSON_CNT= 5;

    public interface ITstClassInit { object Init(int i, Random rnd); }
    public class SimpleTestClass : ITstClassInit {
      const string chars= "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      static string randomString(Random rnd, int length) {
        return new string(Enumerable.Range(1, length)
                                    .Select(_ => chars[rnd.Next(chars.Length)])
                                    .ToArray());
      }
      public string StrProp { get; set; }
      public double NumProp { get; set; }
      public DateTime DateProp { get; set; }
      public IList<string> ListProp { get; set; }
      public string[] AryProp { get; set; }
      public virtual object Init(int i, Random rnd) {
        var len= rnd.Next(11, 137);
        var numStart= len * rnd.NextDouble();
        this.StrProp= randomString(rnd, len);
        this.NumProp= numStart + i;
        this.DateProp= new DateTime((long)(this.NumProp * 111000));
        this.ListProp= Enumerable.Range(1, 7)
                                 .Select(_ => randomString(rnd, 87))
                                 .ToArray();
        this.AryProp= Enumerable.Range(1, 5)
                                .Select(_ => randomString(rnd, 87))
                                .ToArray();
        return this;
      }
      public SimpleTestClass() { }
      public SimpleTestClass(SimpleTestClass parent) {
        this.StrProp= "__" + parent.StrProp;
        this.NumProp= parent.NumProp + parent.NumProp * 0.000001;
        this.DateProp= parent.DateProp.AddTicks((long)parent.NumProp);
      }
    }
    public class TestClass : SimpleTestClass {
      public override object Init(int i, Random rnd) {
        base.Init(i, rnd);
        this.TstProp= new SimpleTestClass(this);
        return this;
      }

      public SimpleTestClass TstProp { get; set; }
    }

    public class Enum<T> : IEnumerable<T> where T : ITstClassInit, new() {
      public Enum(int cnt = 34567) => this.cnt= cnt;
      readonly int cnt;
      public IEnumerator<T> GetEnumerator() {
        var rnd= new Random();
        for (var l = 0; l < cnt; ++l)
          yield return (T)new T().Init(l, rnd);
      }
      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class ReusableStream : MemoryStream {
      protected override void Dispose(bool disposing) {
        this.Position= 0;   //do not really dispose
      }
    }

    ITestOutputHelper tstout;
    public JsonStreamEnumeratorTest(ITestOutputHelper tstout) => this.tstout= tstout;

    public static Stream JsonStream<T>(int cnt) where T : ITstClassInit, new() {
      var strm= new ReusableStream();

      var json= JsonFormat.CreateSerializer<IEnumerable<T>>();
      json.WriteObj(strm, new Enum<T>(cnt));
      strm.Position= 0;

      // tstout.WriteLine("json to be parsed:");
      // tstout.WriteLine(Encoding.UTF8.GetString(strm.ToArray()));
      // strm.Position= 0;

      return strm;
    }

    [Fact]
    public void EmptyTest() {
      Action<TestClass> noTest= (obj) => { };
      var strm= new MemoryStream(Encoding.UTF8.GetBytes("   \t[ \n\n]  "));
      serializewithBufSize(strm, 0, 111, noTest);

      strm= new MemoryStream(Encoding.UTF8.GetBytes("   \t\n\t  "));
      Assert.ThrowsAny<JsonException>(() => serializewithBufSize(strm, 0, 55, noTest));
    }

    [Fact]
    public void BasicTest() {
      Action<TestClass> test= (obj) =>  {
        Assert.NotNull(obj.StrProp);
        Assert.NotEmpty(obj.ListProp);
        Assert.NotEmpty(obj.AryProp);
      };
      var strm= JsonStream<TestClass>(JSON_CNT);

      serializewithBufSize<TestClass>(strm, JSON_CNT, 17, test);
      serializewithBufSize<TestClass>(strm, JSON_CNT, 256, test);
      serializewithBufSize<TestClass>(strm, JSON_CNT, 4096, test);
    }

    [Fact]
    public void PropDictionaryTest() {
      Action<Dictionary<string, object>> test= (dict) =>  {
        Assert.NotNull(dict["StrProp"]);
        Assert.True(object.ReferenceEquals(dict["StrProp"], dict["strProp"]));    //non case sesitive
        Assert.NotEmpty(dict["ListProp"] as IEnumerable);
      };
      var strm= JsonStream<TestClass>(JSON_CNT);

      serializewithBufSize<Dictionary<string, object>>(strm, JSON_CNT, 17, test);
      serializewithBufSize<Dictionary<string, object>>(strm, JSON_CNT, 256, test);
      serializewithBufSize<Dictionary<string, object>>(strm, JSON_CNT, 4096, test);
    }

    private void serializewithBufSize<T>(Stream strm, int objCnt, int bufSz, Action<T> test) {
      strm.Position= 0;
      tstout.WriteLine("*** bufSz: {0:D}", bufSz);
      JsonStreamEnumerator<T>.Deserializer.StreamState state= default;
      try {
        state= new JsonStreamEnumerator<T>.Deserializer(strm, JsonFormat.DefaultOptions, bufSz).InternalState;

        var jsonStreamReader= new JsonStreamEnumerator<T>.Deserializer(state);
        tstout.WriteLine("at: " + jsonStreamReader.currToken.ToString());
        // jsonStreamReader.Read();
        var cnt= 0;
        while (jsonStreamReader.MoveNext()) {
          tstout.WriteLine($"#{++cnt} at: {jsonStreamReader.currToken.ToString()}");

          T tstObj= jsonStreamReader.Current;
          Assert.NotNull(tstObj);
          test(tstObj);
          jsonStreamReader= new JsonStreamEnumerator<T>.Deserializer(state= jsonStreamReader.InternalState);
        }
        Assert.Equal(objCnt, cnt);
      }
      finally {
        state.Dispose();
      }
    }

    [Fact]
    public void EnumeratorTest() {
      var strm= JsonStream<TestClass>(JSON_CNT);
      var enumer= new JsonStreamEnumerator<TestClass>(strm, JsonFormat.DefaultOptions);

      Assert.Equal(JSON_CNT, enumer.Count());
      enumer.Reset();
      var l= 0;
      foreach (var el in enumer) {
        Assert.NotNull(el);
        Assert.NotNull(el.StrProp);
        ++l;
      }
      Assert.Equal(JSON_CNT, l);
    }
  }
}
