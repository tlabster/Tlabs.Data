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
  public class DocSchemaProcessorTest {
    public class Document : BaseDocument<Document> {
    }

    private static readonly JsonFormat.DynamicSerializer tstSer= JsonFormat.CreateDynSerializer();

    public static DocumentSchema CreateTestSchema() {
      return new DocumentSchema {
        TypeName= "TST-DOC",
        TypeVers= "0",
        Comment= "comment text",

        Fields= new List<DocumentSchema.Field> {
          new DocumentSchema.Field { Name= "txtProp", TypeName= "TEXT" },
          new DocumentSchema.Field { Name= "txtLstProp", TypeName= "TEXT[]" },
          new DocumentSchema.Field { Name= "numProp", TypeName= "NUMBER" }
        },

        Validations= new List<DocumentSchema.ValidationRule> {
          new DocumentSchema.ValidationRule { Key= "v01", Code= "{d.txtProp.Length > 0}", Description= "must not be empty" },
          new DocumentSchema.ValidationRule { Key= "v02", Code= "{d.txtLstProp.Count > 0}", Description= "must not be empty" },
          new DocumentSchema.ValidationRule { Key= "v03", Code= "{d.txtLstProp.Contains(\"tstText01\")}", Description= "must contain tstText01" },
          new DocumentSchema.ValidationRule { Key= "v03", Code= "{NOT d.txtLstProp.Contains(\"xyz\")}", Description= "must NOT contain xyz" }
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

    public static IDocSchemaProcessor CreateDocSchemaProcessor(DocumentSchema schema) {
      var dynSerializer= JsonFormat.CreateDynSerializer();
      return new Processing.Intern.DocSchemaProcessor(schema, new DocumentClassFactory(null), dynSerializer);
    }

    [Fact]
    void CompiledValidationTest() {
      var dynSerializer= JsonFormat.CreateDynSerializer();

      var proc= CreateDocSchemaProcessor(CreateTestSchema());

      dynamic bodyObj= proc.EmptyBody;
      DocumentSchema.ValidationRule rule;
      Assert.False(proc.CheckValidation((object)bodyObj, out rule));

      bodyObj.txtProp= "tstText";
      bodyObj.txtLstProp= new List<string>{ "tstText01", "tstText02" };
      var doc= new Document();
      doc.Sid= proc.Sid;
      proc.UpdateBodyObject(doc, bodyObj);
      Assert.True(proc.CheckValidation<Document>(doc, out rule), rule?.Description ?? "RULE???");

      dynamic bodyObj2= proc.LoadBodyObject(doc);
      Assert.Equal(bodyObj.txtProp, bodyObj2.txtProp);
      Assert.NotEmpty(bodyObj2.txtProp);
      Assert.True(bodyObj2.txtLstProp.Contains(bodyObj.txtLstProp[0]));
      Assert.True(bodyObj2.txtLstProp.Contains(bodyObj.txtLstProp[1]));
      Assert.False(bodyObj2.txtLstProp.Contains("TSTText01"));  //check case
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
  }
}
