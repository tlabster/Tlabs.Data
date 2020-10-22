using System;
using System.Collections.Generic;
using System.Dynamic;

using Tlabs.Data.Entity;
using Tlabs.Data.Entity.Intern;
using Tlabs.Data.Serialize.Json;
using Tlabs.Dynamic;
using Xunit;

namespace Tlabs.Data.Processing.Tests {
  public class DocSchemaProcessorTest {
    public class TstDocument : BaseDocument<TstDocument> {
      public TstDocument(string sid) { this.Sid= sid; }
      public override object SetBodyObject(object bodyObj) {
        var body= new DynamicAccessor(bodyObj.GetType()).ToDictionary(bodyObj);
        StatusDetails=    body["TxtProp01"] as string
                       ?? StatusDetails;
        return bodyObj;
      }
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
          new DocumentSchema.Field { Name= "numProp", TypeName= "NUMBER" },
          new DocumentSchema.Field { Name= "autoTxt", TypeName= "TEXT", CalcFormula= "d.txtProp + \": \" + DateTime.Now.ToString()" },
          new DocumentSchema.Field { Name= "dateNow", TypeName= "DATE", CalcFormula= "DateTime.Now" },
          new DocumentSchema.Field { Name= "compDate", TypeName= "DATE", CalcFormula= "d.dateNow" }
        },

        Validations= new List<DocumentSchema.ValidationRule> {
          new DocumentSchema.ValidationRule { Key= "v01", Code= "{d.txtProp.Length > 0}", Description= "must not be empty" },
          new DocumentSchema.ValidationRule { Key= "v02", Code= "{d.txtLstProp.Count > 0}", Description= "must not be empty" },
          new DocumentSchema.ValidationRule { Key= "v03", Code= "{d.txtLstProp.Contains(\"tstText01\")}", Description= "must contain tstText01" },
          new DocumentSchema.ValidationRule { Key= "v04", Code= "{NOT d.txtLstProp.Contains(\"xyz\")}", Description= "must NOT contain xyz" },
          new DocumentSchema.ValidationRule { Key= "v05", Code= "{d.autoTxt.StartsWith(d.txtProp)}", Description= "must start with <textProp>" },
          new DocumentSchema.ValidationRule { Key= "v06", Code= "{d.dateNow == d.compDate}", Description= "it must d.dateNow == compDate" }
        }
      };
    }

    static readonly List<DocumentSchema.Field> SAMPLE_SCHEMA_FIELDS= new List<DocumentSchema.Field>() {
      new DocumentSchema.Field {Name= "TxtProp01", TypeName= "TEXT", ExtMappingInfo = "path=PID/01"},
      new DocumentSchema.Field {Name= "NumProp01", TypeName= "NUMBER"},
      new DocumentSchema.Field {Name= "DateProp01", TypeName= "DATETIME"},
      new DocumentSchema.Field {Name= "BoolProp01", TypeName= "BooleAN"},  //must be case insensitive
      new DocumentSchema.Field {Name= "TxtLst01", TypeName= "TEXT[]"},
      new DocumentSchema.Field {Name= "NumLst01", TypeName= "NUMBER[]"},
      new DocumentSchema.Field {Name= "CmpTxt01", TypeName= "TEXT", CalcFormula= "d.TxtProp01 + \"-\" + DateTime.Now.Ticks.ToString()" }
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
          Description= "string prop length validation failed",
          Code= "{ d.TxtProp01.length > 2 }"
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
      return new Processing.Intern.DocSchemaProcessor(
        new Processing.Intern.CompiledDocSchema<DefaultExpressionContext, DefaultExpressionContext>(schema, new DocumentClassFactory(null), null, null), dynSerializer
      );
    }

    [Fact]
    void ComputedValidationTest() {
      var dynSerializer= JsonFormat.CreateDynSerializer();

      var proc= CreateDocSchemaProcessor(CreateTestSchema());
      dynamic bodyObj= proc.EmptyBody;
      var vcx= new DefaultExpressionContext(bodyObj);
      DocumentSchema.ValidationRule rule;
      Assert.False(proc.CheckValidation((object)bodyObj, vcx, out rule));

      bodyObj.txtProp= "tstText";
      bodyObj.txtLstProp= new List<string>{ "tstText01", "tstText02" };
      var doc= new TstDocument(proc.Sid);
      dynamic bodyObj2= proc.UpdateBodyObject(doc, bodyObj);
      vcx= new DefaultExpressionContext(bodyObj2);

      var cx= new DefaultExpressionContext(bodyObj2);
      proc.EvaluateComputedFields(cx);
      Assert.True(proc.CheckValidation<TstDocument>(doc, vcx, out rule), rule?.Description ?? "RULE???");

      Assert.Equal(bodyObj.txtProp, bodyObj2.txtProp);
      Assert.NotEmpty(bodyObj2.txtProp);
      Assert.True(bodyObj2.txtLstProp.Contains(bodyObj.txtLstProp[0]));
      Assert.True(bodyObj2.txtLstProp.Contains(bodyObj.txtLstProp[1]));
      Assert.False(bodyObj2.txtLstProp.Contains("TSTText01"));  //check case

      //computed fields:
      Assert.NotEmpty(bodyObj2.autoTxt);
      Assert.IsType<DateTime>(bodyObj2.dateNow);
      Assert.True((DateTime.Now - (DateTime?)bodyObj2.dateNow).Value.TotalMilliseconds < 1000);
      Assert.Equal(bodyObj2.dateNow, bodyObj.compDate);
    }

    [Fact]
    public void SchemaValidationTest() {
      var docSchema= CreateDocSchema();
      var proc= CreateDocSchemaProcessor(docSchema);

      DocumentSchema.ValidationRule rule;
      object bodyObj= proc.EmptyBody;
      var body= new DynamicAccessor(bodyObj.GetType()).ToDictionary(bodyObj);
      body["TxtProp01"]= "_TXT_";
      var vcx= new DefaultExpressionContext(bodyObj);
      var res= proc.CheckValidation(bodyObj, vcx, out rule);
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
      var vcx= new DefaultExpressionContext(obj);

      DocumentSchema.ValidationRule rule;
      Assert.True(proc.CheckValidation((object) obj, vcx, out rule));
      Assert.Null(rule);

    }

    [Fact]
    public void UpdateBodyObjectTest() {
      var docSchema= CreateDocSchema();
      var proc= CreateDocSchemaProcessor(docSchema);
      var doc= new TstDocument(docSchema.TypeId);

      /* 1. Update with object of exact proc.BodyType:
       */
      dynamic obj= proc.EmptyBody;
      var txtVal= obj.TxtProp01= "TST";
      var numVal= obj.NumProp01= 2.7182818285M;
      proc.UpdateBodyObject(doc, obj);
      dynamic body= proc.LoadBodyObject(doc);
      Assert.IsType(proc.BodyType,body);
      Assert.Equal(txtVal, body.TxtProp01);
      Assert.Equal(numVal, body.NumProp01);

      /* 2. Update with object of 'similar' type:
       */
      obj= new {
        TxtProp01= "TST",
        NumProp01= 2.7182818285M
      };

      var origModified= doc.Modified;
      Assert.NotNull(origModified);
      Assert.True(origModified < App.TimeInfo.Now);

      proc.UpdateBodyObject(doc, obj);

      Assert.True(origModified < doc.Modified);

      body= proc.LoadBodyObject(doc);
      Assert.IsType(proc.BodyType, body);
      Assert.Equal(txtVal, body.TxtProp01);
      Assert.Equal(numVal, body.NumProp01);

      /* 3. Update with dictionary:
       */
      var dict= new Dictionary<string, object> {
        ["TxtProp01"] = "TST",
        ["NumProp01"]= 2.7182818285M
      };
      proc.UpdateBodyObject(doc, dict);
      body= proc.LoadBodyObject(doc);
      Assert.IsType(proc.BodyType, body);
      Assert.Equal(txtVal, body.TxtProp01);
      Assert.Equal(numVal, body.NumProp01);
    }

    [Fact]
    public void MergeBodyPropertiesTest() {
      var docSchema= CreateDocSchema();
      var proc= CreateDocSchemaProcessor(docSchema);
      var doc= new TstDocument(docSchema.TypeId);

      // dynamic obj= Activator.CreateInstance(proc.BodyType);
      // obj.TxtProp01= "TST0";
      // obj.NumProp01= 27.182818285M;
      // proc.UpdateBodyObject(doc, obj);  //set some initial body properties

      var txtLst= new string[] {"1", "2"};
      var props= new Dictionary<string, object> {
        ["TxtProp01"] = "TST",          //overwrite
        ["NumProp01"]= 2.7182818285M,   //overwrite
        ["TxtLst01"]= txtLst,
        ["ToBeIgnored"]= true
      };
      dynamic body= proc.MergeBodyProperties(doc, props);
      Assert.Equal(props["TxtProp01"], body.TxtProp01);
      Assert.Equal(props["NumProp01"], body.NumProp01);
      Assert.Equal(txtLst.Length, body.TxtLst01.Count);
      IDictionary<string, object> dict= proc.BodyAccessor.ToDictionary(body);
      Assert.False(dict.ContainsKey("ToBeIgnored"));
      Assert.StartsWith("TST-", body.CmpTxt01);
    }
  }
}
