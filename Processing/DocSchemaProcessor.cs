using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

using Microsoft.Extensions.Logging;

using Tlabs.Data;
using Tlabs.Data.Entity;
using Tlabs.Data.Entity.Intern;
using Tlabs.Data.Serialize;
using Tlabs.Dynamic;
using Tlabs.CalcNgn;

namespace Tlabs.Data.Processing {

  /// <summary><see cref="DocumentSchema"/> processor.</summary>
  public partial class DocSchemaProcessor {
    /// <summary><see cref="ILogger"/>.</summary>
    public static readonly ILogger<DocSchemaProcessor> Log= App.Logger<DocSchemaProcessor>();
    private DocumentSchema schema;
    private string sid;
    private IDynamicSerializer docSeri;
    private Calculator.Model calcNgnModel;
    private DynamicAccessor bodyAccessor;
    private ParameterExpression[] typedValidationParms;
    private CompiledValidation[] validationRules;

    /// <summary>Ctor from <paramref name="schema"/>, <paramref name="docClassFactory"/> and <paramref name="docSeri"/>.</summary>
    public DocSchemaProcessor(DocumentSchema schema, IDocumentClassFactory docClassFactory, IDynamicSerializer docSeri, Calculator calcNgn) {
      if (null == (this.schema= schema)) throw new ArgumentNullException(nameof(schema));
      this.sid= schema.TypeId;
      if (null == (this.schema.Fields)) throw new ArgumentException(nameof(schema.Fields));
      var validations= this.schema.Validations;
      if (null == (validations)) throw new ArgumentException(nameof(schema.Validations));

      this.BodyType= docClassFactory.GetBodyType(schema);
      this.typedValidationParms= new ParameterExpression[] {
        Expression.Parameter(BodyType, "d"),
        // Expression.Parameter(typeof(Insuree), "i"),
        // Expression.Parameter(typeof(ISource), "p")
      };

      this.docSeri= docSeri;
      if (schema.HasCalcModel) {
        // using (var strm= new FileStream("D:\\tmp\\calcModel.xls", FileMode.Create)) {
        //   schema.CalcModelStream.CopyTo(strm);
        //   schema.CalcModelStream.Position= 0;
        // }
        this.calcNgnModel= calcNgn.LoadModel(schema.CalcModelStream);
        var impCnt= calcNgnModel.Definition.Imports.Count;
        var expCnt= calcNgnModel.Definition.Exports.Count;
        Log.LogDebug("{schema} has {imp} import(s) and {exp} export(s).", schema.TypeId, impCnt, expCnt);
        if (0 == impCnt + expCnt) Log.LogWarning("No data import/export definition found in calcModel of {schema}.", schema.TypeId);
      }
      bodyAccessor= new DynamicAccessor(this.BodyType);

      this.validationRules= new CompiledValidation[validations.Count];
      var errors= new List<CodeSyntaxException>();
      for (var l = 0; l < validations.Count; ++l) try {
          this.validationRules[l]= new CompiledValidation(this, validations[l]);
        }
        catch (CodeSyntaxException se) {
          errors.Add(se);
          if (errors.Count >= 10) break;
        }
      if (errors.Count > 0) throw new CodeSyntaxException(errors);  //error aggregate
    }

    /// <summary><see cref="DocumentSchema"/>.</summary>
    public DocumentSchema Schema => schema;
    /// <summary>Schema type Id.</summary>
    public string Sid => sid;
    /// <summary><see cref="BaseDocument{T}"/>'s Body type resulting from <see cref="DocumentSchema"/>.</summary>
    public Type BodyType { get; }
    /// <summary>Empty body object of Type: <see cref="BodyType"/>.</summary>
    public object EmptyBody {
      get {
        var emptyBody= Activator.CreateInstance(BodyType);
        var bodyTypeInfo= BodyType.GetTypeInfo();
        foreach (var strFld in schema.Fields.Where(fld => fld.Type == typeof(string))) {
          var prop= bodyTypeInfo.GetDeclaredProperty(strFld.Name);
          prop.SetValue(emptyBody, string.Empty);
        }
        return emptyBody;
      }
    }

    ///<summary><see cref="DynamicAccessor"/> to the body type.</summary>
    public DynamicAccessor BodyAccessor => bodyAccessor;

    ///<summary>Return <paramref name="doc"/>'s Body as object (according to its <see cref="DocumentSchema"/>).</summary>
    public object LoadBodyObject<T>(T doc) where T : BaseDocument<T> {
      checkDocument(doc);
      return doc.GetBodyObject((body) => {
        var bodyData= body.BodyData;
        return   null != bodyData
               ? docSeri.LoadObj(new MemoryStream(body.BodyData), BodyType)
               : Activator.CreateInstance(BodyType);
      });
    }

    ///<summary>Set <paramref name="doc"/>'s Body to <paramref name="bodyObj"/>.</summary>
    /// <remarks>
    /// By specifying a <paramref name="setupData"/> delegate the caller can provide a custom dictionary of data beeing imported into
    /// the CalcNgn model. (Defaults to a dictionary representing all public properties of the <paramref name="bodyObj"/>.)
    /// </remarks>
    public object UpdateBodyObject<T>(T doc, object bodyObj, Func<object, IDictionary<string, object>> setupData= null, int bufSz = 10*1024) where T : BaseDocument<T> {
      checkDocument(doc);

      var body= doc.Body;

      if (!BodyType.GetTypeInfo().IsAssignableFrom(bodyObj.GetType())) {
        /* coerce obj into bodyType !!!
          */
        using (var strm = new MemoryStream(bufSz)) {
          docSeri.WriteObj(strm, bodyObj);
          body.BodyData= strm.ToArray();
          body.Encoding= docSeri.Encoding;

          strm.Position= 0;
          bodyObj= docSeri.LoadObj(strm, BodyType);
        }
      }

      computeBodyObject(bodyObj, setupData);

      using (var strm = new MemoryStream(bufSz)) {
        strm.SetLength(0);
        strm.Position= 0;
        docSeri.WriteObj(strm, bodyObj);
        body.BodyData= strm.ToArray();
        body.Encoding= docSeri.Encoding;
      }
      return doc.SetBodyObject(bodyObj);
    }

    private void checkDocument<T>(T doc) where T : BaseDocument<T> {
      if (null == doc) throw new ArgumentNullException(nameof(doc));
      if (null == doc.Body) throw new ArgumentException(nameof(doc.Body));
      if (this.schema.TypeId != doc.Sid) throw new ArgumentException(nameof(doc.Sid));
    }

    private object computeBodyObject(object bodyObj, Func<object, IDictionary<string, object>> setupData) {
      if (null == calcNgnModel) return bodyObj;
      var model=   null != setupData
                 ? setupData(bodyObj)
                 : bodyAccessor.ToDictionary(bodyObj);
#if DEBUG
      SaveModel(Path.Combine(Path.GetDirectoryName(App.MainEntryPath), "calcNgnModel", schema.TypeName + "0.xls"));
#endif
      calcNgnModel.Compute(model);
#if DEBUG
      SaveModel(Path.Combine(Path.GetDirectoryName(App.MainEntryPath), "calcNgnModel", schema.TypeName + ".xls"));
#endif
      return bodyObj;
    }

    private void SaveModel(string path) {
      new DirectoryInfo(Path.GetDirectoryName(path)).Create();  //ensure path
      using (var strm = File.Create(path)) {
        calcNgnModel.Definition.WriteStream(strm);
      }
    }

    ///<summary>Check <paramref name="doc"/> against the validation rules and applies the result to the document status properties.</summary>
    public void ApplyValidation<T>(T doc/*, Insuree insuree, ISource src*/, out object body) where T : BaseDocument<T> {
      DocumentSchema.ValidationRule rule;
      body= LoadBodyObject(doc);
      doc.StatusDetails= null;
      doc.Status= BaseDocument<T>.State.VALID.ToString();
      if (!CheckValidation(body/*, insuree, src*/, out rule)) {
        doc.StatusDetails= $"{rule.Key} - {rule.Description}";
        doc.Status= BaseDocument<T>.State.IMPLAUSIBLE.ToString();
      }
    }


    ///<summary>Check <paramref name="doc"/> against the validation rules.</summary>
    ///<returns>true if valid. If invalid (false) the offending rule is set in <paramref name="rule"/>.</returns>
    public bool CheckValidation<T>(T doc/*, Insuree insuree, ISource src*/, out DocumentSchema.ValidationRule rule) where T : BaseDocument<T> {
      return CheckValidation(LoadBodyObject(doc)/*, insuree, src*/, out rule);
    }

    ///<summary>Check <paramref name="body"/> object against the validation rules.</summary>
    ///<returns>true if valid. If invalid (false) the offending rule is set in <paramref name="rule"/>.</returns>
    public bool CheckValidation(object body/*, Insuree insuree, ISource src*/, out DocumentSchema.ValidationRule rule) {
      rule= null;
      foreach (var v in validationRules) try {
          rule= v.Rule.NewCopy<DocumentSchema.ValidationRule>();
          if (!v.IsValid(body/*, insuree, src*/))
            return false;
        }
        catch (Exception e) { throw new ValidationException(rule, e); }
      rule= null;
      return true;
    }

  }


}