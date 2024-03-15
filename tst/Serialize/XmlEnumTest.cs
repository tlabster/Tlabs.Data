using System;
using System.Xml.Serialization;
using Xunit;

using Tlabs.Data.Serialize.Xml;

namespace Tlabs.Data.Serialize.Tests {

  public class XmlEnumTest {

    public enum TestEnum {
      [XmlEnum("0")]
      Zero= 0,

      [XmlEnum("1")]
      One,

      [XmlEnum("2")]
      Two,
      NoName
    }

    [Fact]
    public void ConversionTest() {
      Assert.False(typeof(Enum).IsEnum);    //oh yes...

      Assert.Equal(((int)TestEnum.Zero).ToString(), TestEnum.Zero.XmlEnumAttributeValue());
      Assert.Equal(((int)TestEnum.One).ToString(), TestEnum.One.XmlEnumAttributeValue());
      Assert.NotEqual(((int)TestEnum.Two).ToString(), TestEnum.Zero.XmlEnumAttributeValue());
      Assert.Equal(TestEnum.NoName.ToString(), TestEnum.NoName.XmlEnumAttributeValue());
      Assert.Empty(default(Enum).XmlEnumAttributeValue());

      TestEnum tstEnm;
      Assert.False(XmlEnum.TryParse(null, out tstEnm));
      Assert.True(XmlEnum.TryParse<TestEnum>(((int)TestEnum.Zero).ToString(), out tstEnm));
      Assert.Equal(TestEnum.Zero, tstEnm);

      Assert.True(XmlEnum.TryParse(((int)TestEnum.One).ToString(), out tstEnm));
      Assert.NotEqual(TestEnum.Two, tstEnm);

      Assert.False(XmlEnum.TryParse("x", out tstEnm));

      var e0= default(Enum);
      Assert.False(XmlEnum.TryParse("x", typeof(Enum), out e0));
      Assert.False(XmlEnum.TryParse("x", out e0));
      Assert.Equal(default(Enum), e0);

      Assert.True(XmlEnum.TryParse(((int)TestEnum.One).ToString(), typeof(TestEnum), out var enm));
      Assert.Equal(TestEnum.One, enm);

      Assert.False(XmlEnum.TryParse(((int)TestEnum.One).ToString(), typeof(char), out e0));
      Assert.Equal(default(Enum), e0);

      Assert.False(XmlEnum.TryParse(null, typeof(char), out var _));

      Assert.True(XmlEnum.TryParse(TestEnum.NoName.ToString(), out tstEnm));
      Assert.Equal(TestEnum.NoName, tstEnm);
    }

    [Fact]
    public void EnumCastTest() {
      Enum e= TestEnum.One;
      Assert.Equal(1, Convert.ToInt32(e));
    }
  }
}