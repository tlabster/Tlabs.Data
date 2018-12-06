using System;
using System.Linq;
using Xunit;

using Tlabs.Data.Entity;
using Tlabs.Data.Serialize.Xml;

namespace Tlabs.Data.Serialize.Tests {
  using XmlSchemaFormat = XmlFormat<DocumentSchema, Entity.Intern.DocXmlSchema>;

  public class XmlSchemaSerialzeTest {
    public static readonly ISerializer<DocumentSchema> DocSchemaSerializer= new XmlSchemaFormat.Serializer(new XmlSchemaFormat(Tlabs.App.Logger<XmlSchemaFormat>()));


    [Fact]
    public void BasicSchemaSerializeTest() {
      var schema= DocSchemaSerializer.LoadObj(SIMPLE_XML);

      Assert.Equal("SOME-TYPE", schema.TypeName);
      Assert.Equal(123, Convert.ToInt32(schema.TypeVers));
      Assert.True(schema.Comment.Length > 0, "Schema comment expected");
    }

    [Fact]
    public void SchemaDeserializationTest() {
      var docSchema= DocSchemaSerializer.LoadObj(TST_SCHEMA_XML);

      Assert.NotEmpty(docSchema.Validations);
      foreach (var valid in docSchema.Validations) {
        Assert.NotEmpty(valid.Key);
        Assert.NotEmpty(valid.Description);
        Assert.NotEmpty(valid.Code);
      }

      Assert.Equal("MY_FORM", docSchema.TypeName);
      Assert.Equal(1, Convert.ToInt32(docSchema.TypeVers));
      Assert.True(docSchema.Comment.Length > 0, "Schema comment expected");
      Assert.NotEmpty(docSchema.Fields);
      DocumentSchema.Field field= docSchema.Fields.Where(f => f.Name == "Kassenname").SingleOrDefault();
      Assert.NotNull(field);
      Assert.True(field.ExtMappingInfo.Length > 0);
      Assert.NotEmpty(field.MappingInfo);

      string mapping= null;
      Assert.True(field.MappingInfo.TryGetValue("edifact-path", out mapping));
      Assert.NotNull(mapping);
      Assert.True(field.MappingInfo.TryGetValue("edifact-mandatory", out mapping));
      Assert.True(field.MappingInfo.TryGetValue("edifact-condition", out mapping));
      Assert.True(field.MappingInfo.TryGetValue("import-field", out mapping));
      Assert.Equal("HealthInsurance", mapping);
    }


    const string SIMPLE_XML= @"<?xml version=""1.0"" encoding=""UTF-8""?>
<form name=""SOME-TYPE"" version=""123"" generator=""Dok-Gen-Tool V1.x""/>";

    //***TODO: XML must be updated to reflect our own document definition...*/
    const string TST_SCHEMA_XML= @"<?xml version=""1.0"" encoding=""UTF-8""?>
<form name=""MY_FORM"" version=""1"" generator=""tool-o-mat v3.5.1"" >
	<source>DB1-ED.xls</source>
	<validations>
		<rule id=""R17C41"" desc=""Diabetis Symptome nicht eindeutig"">{true &amp;&amp; @OneOf2(d.symptomsTrue, d.symptomsFalse) || 1 &lt; 2}</rule>
		<rule id=""R29C41"" desc=""Insulin Therapie nicht eindeutig"">{@OneOf2(d.InsuFalse, d.InsuYes)}</rule>
		<rule id=""R30C41"" desc=""Diagnose Datum ungültig"">{d.nextDocDate.HasValue and d.nextDocDate != null}</rule>
		<rule id=""R30C41"" desc=""Patient ist Männlich"">{i.Birth == d.birthday}</rule>
	</validations>
	<field name=""Kassenname"" type=""TEXT"">
		<cell row=""6"" col=""3""/>
		<edifact path=""DMP/0/1"" mandatory=""true"" condition=""123:DMP/0/0""/>
    <import field=""HealthInsurance""/>
		<input-Control/>
	</field>
	<field name=""Nachname"" type=""TEXT"">
		<cell row=""8"" col=""3""/>
		<input-Control/>
	</field>
	<field name=""Vorname"" type=""TEXT"">
		<cell row=""8"" col=""11""/>
		<input-Control/>
	</field>
	<field name=""KVNR"" type=""TEXT"">
		<cell row=""13"" col=""3""/>
		<input-Control/>
	</field>
	<field name=""VNR"" type=""TEXT"">
		<cell row=""13"" col=""6""/>
		<input-Control/>
	</field>
	<field name=""status"" type=""TEXT"">
		<cell row=""13"" col=""13""/>
		<input-Control/>
	</field>
	<field name=""ArztNr"" type=""TEXT"">
		<cell row=""15"" col=""3""/>
		<input-Control/>
	</field>
	<field name=""validUntil"" type=""DATETIME"">
		<cell row=""15"" col=""6""/>
		<input-Control/>
	</field>
	<field name=""birthday"" type=""DATETIME"">
		<cell row=""15"" col=""13""/>
		<input-Control/>
	</field>
	<field name=""insuYes"" type=""BOOLEAN"">
		<cell row=""30"" col=""29""/>
		<input-Control selection=""true""/>

  </field>

</form>
";

  }
}
