using System;
using System.Linq;

using Xunit;
using Tlabs.Data.Entity;
using Tlabs.Data.Entity.Intern;
using Tlabs.Data.Serialize.Xml;
using Tlabs.Data.Serialize;
using Tlabs.Data.Serialize.Json;
using Tlabs.Misc;
using System.Collections.Generic;
using System.Dynamic;

namespace Tlabs.Data.Processing.Tests {
  using XmlFormater = XmlFormat<DocumentSchema, DocXmlSchema>;
  public class DocSchemaProcessorTest {
    public class TestSerializer {
      public readonly ISerializer<DocumentSchema> DocSchemaSerializer;
      public readonly IDynamicSerializer JsonSerializer;
      public TestSerializer() {
        this.DocSchemaSerializer= new XmlFormater.Serializer(new XmlFormater(App.Logger<XmlFormater>()));
        this.JsonSerializer= JsonFormat.CreateDynSerializer();
      }
    }

    public class Document : BaseDocument<Document> {
    }

    private TestSerializer tstSer= Singleton<TestSerializer>.Instance;

    public static DocumentSchema CreateTestSchema() {
      return new DocumentSchema {
        TypeName= "TST-DOC",
        TypeVers= "0",
        Comment= "comment text",

        Fields= new List<DocumentSchema.Field> {
          new DocumentSchema.Field { Name= "txtProp", TypeName= "TEXT" },
          new DocumentSchema.Field { Name= "numProp", TypeName= "NUMBER" }
        },

        Validations= new List<DocumentSchema.ValidationRule> {
          new DocumentSchema.ValidationRule { Key= "v01", Code= "{d.txtProp.Length > 0}", Description= "must not be empty" }
        }
      };
    }

    static readonly List<DocumentSchema.Field> SAMPLE_SCHEMA_FIELDS= new List<DocumentSchema.Field>() {
      new DocumentSchema.Field() {Name= "TxtProp01", TypeName= "TEXT", ExtMappingInfo = "path=PID/01"},
      new DocumentSchema.Field() {Name= "NumProp01", TypeName= "NUMBER"},
      new DocumentSchema.Field() {Name= "DateProp01", TypeName= "DATETIME"},
      new DocumentSchema.Field() {Name= "BoolProp01", TypeName= "BooleAN"}  //must be case insensitive
    };

    public static List<DocumentSchema.ValidationRule> CreateRulesList(DocumentSchema schema) {
      return new List<DocumentSchema.ValidationRule>() {
        new DocumentSchema.ValidationRule() {
          Schema= schema,
          Key= "#01",
          Description= "String validation failed",
          Code= "{\"Test string\".Length > 5 && \"Test text\".EndsWith(\"text\") && \"text here\".Contains(\"x\") && 0 = string.Compare(\"W\", \"w\", true)}"
        },
        new DocumentSchema.ValidationRule() {
          Schema= schema,
          Key= "#02",
          Description= "@oneOf validation failed",
          Code= "{ @oneOf3(false, true, @oneof2(true, true)) }"
        },
        new DocumentSchema.ValidationRule() {
          Schema= schema,
          Key= "#03",
          Description= "Explicit boolean convertion failed.",
          Code= "{ Convert.ToInt32(true) + Convert.ToInt32(true) = 2 }"
        },
        new DocumentSchema.ValidationRule() {
          Schema= schema,
          Key= "#03.1",
          Description= "Decimal compare test failed.",
          Code= "{ 2.5 > 2 && 2.5 < 3 }"
        },
        new DocumentSchema.ValidationRule() {
          Schema= schema,
          Key= "#04",
          Description= "@Age validation failed",
          Code= "{ @AgeAt(DateTime(2000, 2, 22), DateTime(2017, 5, 15)) == 17 }"
        },
        new DocumentSchema.ValidationRule() {
          Schema= schema,
          Key= "#05",
          Description= "@AtMostOne validation failed",
          Code= "{ @AtMostOne3(false, true, @atmostone4(false, true, false, true)) }"
        },
        new DocumentSchema.ValidationRule() {
          Schema= schema,
          Key= "#06",
          Description= "empty string prop validation failed",
          Code= "{ d.TxtProp01.length == 0 }"
        }
      };
    }

    public static DocumentSchema CreateDocSchema() {
      var schema= new DocumentSchema() {
        TypeName= "basic-x",
        TypeVers= "123",
        Comment= "test",
        Fields= SAMPLE_SCHEMA_FIELDS
      };
      schema.Validations= CreateRulesList(schema);
      return schema;
    }

    public static DocSchemaProcessor CreateDocSchemaProcessor(DocumentSchema schema) {
      var dynSerializer= JsonFormat.CreateDynSerializer();
      return new DocSchemaProcessor(schema, new DocumentClassFactory(null), dynSerializer);
    }

    [Fact]
    void CompiledValidationTest() {
      var dynSerializer= JsonFormat.CreateDynSerializer();

      var proc= CreateDocSchemaProcessor(CreateTestSchema());
      
      DocumentSchema.ValidationRule rule;
      Assert.False(proc.CheckValidation(proc.EmptyBody, out rule));

    }

    [Fact]
    public void SchemaValidationTest() {
      var docSchema= CreateDocSchema();
      var proc= CreateDocSchemaProcessor(docSchema);
      
      DocumentSchema.ValidationRule rule;
      var res= proc.CheckValidation(proc.EmptyBody, out rule);
      Assert.True(res);
      Assert.Null(rule);
    }

    [Fact]
    public void FieldTypesTest() {
      var docSchema= CreateDocSchema();
      docSchema.Validations= new List<DocumentSchema.ValidationRule>() {
        new DocumentSchema.ValidationRule() {
          Schema= docSchema,
          Key= "#01",
          Description= "NUMBER type test failed",
          Code= "{ d.NumProp01.HasValue && (d.NumProp01 -100) = 23 }"
        },
        new DocumentSchema.ValidationRule() {
          Schema= docSchema,
          Key= "#02",
          Description= "DATETIME type test failed",
          Code= "{ not d.DateProp01.HasValue && d.DateProp01 != DateTime.MinValue }"
        }
      };
      var proc= CreateDocSchemaProcessor(docSchema);
      dynamic obj= proc.EmptyBody;
      obj.NumProp01= 123;

      DocumentSchema.ValidationRule rule;
      Assert.True(proc.CheckValidation((object) obj, out rule));
      Assert.Null(rule);

    }

    [Fact]
    public void SimpleDeserializationTest() {
      var docSchema= tstSer.DocSchemaSerializer.LoadObj(SIMPLE_XML);

      Assert.Equal("DB1-ED", docSchema.TypeName);
      Assert.Equal(123, Convert.ToInt32(docSchema.TypeVers));
      Assert.True(docSchema.Comment.Length > 0, "Schema comment expected");
    }

    [Fact]
    public void SchemaDeserializationTest() {
      var docSchema= tstSer.DocSchemaSerializer.LoadObj(DB1_ED_XML);

      Assert.NotEmpty(docSchema.Validations);
      foreach (var valid in docSchema.Validations) {
        Assert.NotEmpty(valid.Key);
        Assert.NotEmpty(valid.Description);
        Assert.NotEmpty(valid.Code);
      }

      Assert.Equal("DB1-ED", docSchema.TypeName);
      Assert.Equal(1, Convert.ToInt32(docSchema.TypeVers));
      Assert.True(docSchema.Comment.Length > 0, "Schema comment expected");
      Assert.NotEmpty(docSchema.Fields);
      DocumentSchema.Field field= docSchema.Fields.Where(f => f.Name == "Kassenname").SingleOrDefault();
      Assert.NotNull(field);
      Assert.True(field.ExtMappingInfo.Length > 0);



      var proc= CreateDocSchemaProcessor(docSchema);
      Assert.IsAssignableFrom(typeof(Type), proc.BodyType);
      var docBody= Activator.CreateInstance(proc.BodyType);
      Assert.NotNull(docBody);

      DocumentSchema.ValidationRule rule;
      var res= proc.CheckValidation(proc.EmptyBody, out rule);
      Assert.False(res);  //Must fail with EmptyBody...
      Assert.NotNull(rule);
    }

    [Fact]
    public void ValidateSid() {
      var docSchema= CreateDocSchema();
      var proc= CreateDocSchemaProcessor(docSchema);
      var doc= new Document();
      doc.Sid= "basic-x:133";

      dynamic dyn= new Object();
      Exception ex= Assert.Throws<ArgumentException>(() => proc.UpdateBodyObject(doc, dyn));
      Assert.Contains(nameof(doc.Sid), ex.Message);
    }

    [Fact]
    public void UpdateBodyObjectTest() {
      var docSchema= CreateDocSchema();
      var proc= CreateDocSchemaProcessor(docSchema);
      var doc= new Document();
      doc.Sid= "basic-x:123";

      dynamic dyn= new ExpandoObject();
      proc.UpdateBodyObject(doc, dyn);
    }

    const string SIMPLE_XML= @"<?xml version=""1.0"" encoding=""UTF-8""?>
<form name=""DB1-ED"" version=""123"" generator=""Dok-Gen-Tool V1.x""/>";

    //***TODO: XML must be updated to reflect our own document definition...*/
    const string DB1_ED_XML= @"<?xml version=""1.0"" encoding=""UTF-8""?>
<form name=""DB1-ED"" version=""1"" generator=""Dok-O-Mat RC5"" >
  <source>DB1-ED.xls</source>
  <validations>
    <rule id=""R17C41"" desc=""Diabetis Symptome nicht eindeutig"">{true &amp;&amp; @OneOf2(d.symptomsTrue, d.symptomsFalse) || 1 &lt; 2}</rule>
    <rule id=""R29C41"" desc=""Insulin Therapie nicht eindeutig"">{@OneOf2(d.InsuFalse, d.InsuYes)}</rule>
    <rule id=""R30C41"" desc=""Diagnose Datum ungültig"">{d.nextDocDate.HasValue and d.nextDocDate != null}</rule>
  </validations>
  <field name=""Kassenname"" type=""TEXT"">
    <cell row=""6"" col=""3""/>
    <edifact path=""DMP/0/1"" mandatory=""true"" condition=""123:DMP/0/0""/>
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

  <field name=""insuFalse"" type=""BOOLEAN"">
    <cell row=""30"" col=""32""/>
    <input-Control selection=""true""/>

  </field>

  <field name=""symptomsTrue"" type=""BOOLEAN"">
    <cell row=""35"" col=""11""/>
    <input-Control selection=""true""/>

  </field>

  <field name=""symptomsFalse"" type=""BOOLEAN"">
    <cell row=""35"" col=""14""/>
    <input-Control selection=""true""/>

  </field>

  <field name=""smokerTrue"" type=""BOOLEAN"">
    <cell row=""41"" col=""35""/>
    <input-Control selection=""true""/>

  </field>

  <!-- ... -->

  <field name=""nextDocDate"" type=""DATETIME"">
    <cell row=""103"" col=""25""/>
    <input-Control/>
  </field>
  <field name=""dokIntervallQuart"" type=""BOOLEAN"">
    <cell row=""105"" col=""31""/>
    <input-Control selection=""true""/>

  </field>

  <field name=""dokIntervallSecondQuart"" type=""BOOLEAN"">
    <cell row=""107"" col=""31""/>
    <input-Control selection=""true""/>

  </field>

  <field name=""docDate"" type=""DATETIME"">
    <cell row=""109"" col=""25""/>
    <input-Control/>
  </field>
</form>
";

  }
}
