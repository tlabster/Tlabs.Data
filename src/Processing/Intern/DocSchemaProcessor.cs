using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Logging;

using Tlabs.Data;
using Tlabs.Data.Entity;
using Tlabs.Data.Entity.Intern;
using Tlabs.Data.Serialize;
using Tlabs.Dynamic;

namespace Tlabs.Data.Processing.Intern {
  using DynamicExpression= DynamicExpression<DocSchemaProcessor.ValidationContext, bool>;

  /// <summary><see cref="DocumentSchema"/> processor.</summary>
  public class DocSchemaProcessor : IDocSchemaProcessor {
    /// <summary><see cref="ILogger"/>.</summary>
    public static readonly ILogger<DocSchemaProcessor> Log= App.Logger<DocSchemaProcessor>();

    /// <summary><see cref="DocumentSchema"/>.</summary>
    protected DocumentSchema schema;

    /// <summary>Schema id.</summary>
    protected string sid;

    private IDynamicSerializer docSeri;

    /// <summary>internal dynamic body accessor.</summary>
    protected DynamicAccessor bodyAccessor;

    private IDictionary<string, Type> ctxConverter;
    private CompiledValidation[] validationRules;

    /// <summary>Validation datat context type.</summary>
    public class ValidationContext {
      /// <summary>Docuemnt exposed as d.</summary>
      public object d { get; set; }
    }

    class CompiledValidation {
      public DocumentSchema.ValidationRule Rule { get; set; }
      public DynamicExpression Validator { get; set; }
    }

    /// <summary>Ctor from <paramref name="schema"/>, <paramref name="docClassFactory"/> and <paramref name="docSeri"/>.</summary>
    public DocSchemaProcessor(DocumentSchema schema, IDocumentClassFactory docClassFactory, IDynamicSerializer docSeri) {
      if (null == (this.schema= schema)) throw new ArgumentNullException(nameof(schema));
      this.sid= schema.TypeId;
      if (null == (this.schema.Fields)) throw new ArgumentException(nameof(schema.Fields));
      var validations= this.schema.Validations;
      if (null == (validations)) throw new ArgumentException(nameof(schema.Validations));

      this.BodyType= docClassFactory.GetBodyType(schema);
      this.ctxConverter= new Dictionary<string, Type> {
        [nameof(ValidationContext.d)]= this.BodyType
      };

      this.docSeri= docSeri;
      this.bodyAccessor= new DynamicAccessor(this.BodyType);

      this.validationRules= new CompiledValidation[validations.Count];
      var errors= new List<ExpressionSyntaxException>();
      for (var l= 0; l < validations.Count; ++l) try {
          var valid= validations[l];
          var exprCode= valid.Code;
          if (!exprCode.StartsWith("{") || !exprCode.EndsWith("}")) throw new ExpressionSyntaxException("Validation rule expession within {braces} expected.");
          exprCode= exprCode.Substring(1, exprCode.Length-2);
          this.validationRules[l]= new CompiledValidation {
            Rule= validations[l],
            Validator= new DynamicExpression(exprCode, ctxConverter)
          };
        }
        catch (ExpressionSyntaxException se) {
          errors.Add(se);
          if (errors.Count >= 10) break;
        }
      if (errors.Count > 0) throw new ExpressionSyntaxException(errors);  //error aggregate
    }

    ///<inherit/>>
    public DocumentSchema Schema => schema;
    ///<inherit/>>
    public string Sid => sid;
    ///<inherit/>>
    public Type BodyType { get; }
    ///<inherit/>>
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

    ///<inherit/>>
    public DynamicAccessor BodyAccessor => bodyAccessor;

    ///<inherit/>>
    public object LoadBodyObject<T>(T doc) where T : BaseDocument<T> {
      checkDocument(doc);
      return doc.GetBodyObject((body) => {
        var bodyData= body.BodyData;
        return   null != bodyData
               ? docSeri.LoadObj(new MemoryStream(body.BodyData), BodyType)
               : Activator.CreateInstance(BodyType);
      });
    }

    ///<inherit/>>
    public object UpdateBodyObject<T>(T doc, object bodyObj, Func<object, IDictionary<string, object>> setupData= null, int bufSz= 10*1024) where T : BaseDocument<T> {
      checkDocument(doc);

      var body= doc.Body;

      if (!BodyType.GetTypeInfo().IsAssignableFrom(bodyObj.GetType())) {
        /* coerce obj into bodyType !!!
          */
        using (var strm= new MemoryStream(bufSz)) {
          docSeri.WriteObj(strm, bodyObj);
          body.BodyData= strm.ToArray();
          body.Encoding= docSeri.Encoding;

          strm.Position= 0;
          bodyObj= docSeri.LoadObj(strm, BodyType);
        }
      }

      processBodyObject(bodyObj, setupData);

      using (var strm= new MemoryStream(bufSz)) {
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

    ///<inherit/>>
    protected virtual object processBodyObject(object bodyObj, Func<object, IDictionary<string, object>> setupData) => bodyObj;

    ///<inherit/>>
    public void ApplyValidation<T>(T doc/*, Insuree insuree, ISource src*/, out object body) where T : BaseDocument<T> {
      DocumentSchema.ValidationRule rule;
      body= LoadBodyObject(doc);
      doc.StatusDetails= null;
      doc.Status= BaseDocument<T>.State.VALID.ToString();
      if (!CheckValidation(body, out rule)) {
        doc.StatusDetails= $"{rule.Key} - {rule.Description}";
        doc.Status= BaseDocument<T>.State.IMPLAUSIBLE.ToString();
      }
    }


    ///<inherit/>>
    public bool CheckValidation<T>(T doc, out DocumentSchema.ValidationRule rule) where T : BaseDocument<T> {
      return CheckValidation(LoadBodyObject(doc), out rule);
    }

    ///<inherit/>>
    public bool CheckValidation(object body, out DocumentSchema.ValidationRule rule) {
      var ctx= new ValidationContext { d= body };
      rule= null;
      foreach (var v in validationRules) try {
          rule= v.Rule.NewCopy<DocumentSchema.ValidationRule>();
          if (!v.Validator.Evaluate(ctx))
            return false;
        }
        catch (Exception e) { throw new DocumentValidationException(rule, e); }
      rule= null;
      return true;
    }

  }


}