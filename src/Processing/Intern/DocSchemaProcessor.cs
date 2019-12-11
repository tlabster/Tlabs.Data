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

  /// <summary><see cref="DocumentSchema"/> processor.</summary>
  public class DocSchemaProcessor : IDocSchemaProcessor {
    /// <summary><see cref="ILogger"/>.</summary>
    public static readonly ILogger<DocSchemaProcessor> Log= App.Logger<DocSchemaProcessor>();

    /// <summary>Schema id.</summary>
    protected ICompiledDocSchema compSchema;
    private IDynamicSerializer docSeri;


    /// <summary>Ctor from <paramref name="compSchema"/> and <paramref name="docSeri"/>.</summary>
    public DocSchemaProcessor(ICompiledDocSchema compSchema, IDynamicSerializer docSeri) {
      if (null == (this.compSchema= compSchema)) throw new ArgumentNullException(nameof(compSchema));
      if (null == (this.docSeri= docSeri)) throw new ArgumentNullException(nameof(docSeri));
    }

    ///<inherit/>>
    public DocumentSchema Schema => compSchema.Schema;
    ///<inherit/>>
    public string Sid => compSchema.Sid;
    ///<inherit/>>
    public Type BodyType => compSchema.BodyType;
    ///<inherit/>>
    public DynamicAccessor BodyAccessor => compSchema.BodyAccessor;
    ///<inherit/>>
    public object EmptyBody {
      get {
        var emptyBody= Activator.CreateInstance(BodyType);
        var bodyTypeInfo= BodyType.GetTypeInfo();
        foreach (var strFld in Schema.Fields.Where(fld => fld.Type == typeof(string))) {
          var prop= bodyTypeInfo.GetDeclaredProperty(strFld.Name);
          prop.SetValue(emptyBody, string.Empty);
        }
        return emptyBody;
      }
    }

    ///<inherit/>>
    public object LoadBodyObject<DocT>(DocT doc) where DocT : BaseDocument<DocT> {
      checkDocument(doc);
      return doc.GetBodyObject((body) => {
        var bodyData= body.BodyData;
        return   null != bodyData
               ? docSeri.LoadObj(new MemoryStream(body.BodyData), BodyType)
               : EmptyBody;
      });
    }

    ///<inherit/>>
    public object UpdateBodyObject<DocT>(DocT doc, object bodyObj, Func<object, IDictionary<string, object>> setupData= null, int bufSz= 10*1024) where DocT : BaseDocument<DocT> {
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

    ///<inheritdoc/>
    public object MergeBodyProperties<TDoc>(TDoc doc, IEnumerable<KeyValuePair<string, object>> props) where TDoc : BaseDocument<TDoc> {
      if (null == props) return props;
      var body= LoadBodyObject(doc);
      var bodyProps= BodyAccessor.ToDictionary(body);
      foreach (var pair in props)
        bodyProps[pair.Key]= pair.Value;
      return UpdateBodyObject(doc, body);
    }

    private void checkDocument<DocT>(DocT doc) where DocT : BaseDocument<DocT> {
      if (null == doc) throw new ArgumentNullException(nameof(doc));
      if (null == doc.Body) throw new ArgumentException(nameof(doc.Body));
      if (this.Sid != doc.Sid) throw new ArgumentException(nameof(doc.Sid));
    }

    ///<inherit/>>
    protected virtual object processBodyObject(object bodyObj, Func<object, IDictionary<string, object>> setupData) => bodyObj;

    ///<inherit/>>
    public void ApplyValidation<DocT>(DocT doc, object vx, out object body) where DocT : BaseDocument<DocT> {
      DocumentSchema.ValidationRule rule;
      body= LoadBodyObject(doc);
      doc.StatusDetails= null;
      doc.Status= BaseDocument<DocT>.State.VALID.ToString();
      if (!compSchema.CheckValidation(body, vx, out rule)) {
        doc.StatusDetails= $"{rule.Key} - {rule.Description}";
        doc.Status= BaseDocument<DocT>.State.IMPLAUSIBLE.ToString();
      }
    }


    ///<inherit/>
    public bool CheckValidation<DocT>(DocT doc, object vx, out DocumentSchema.ValidationRule rule) where DocT : BaseDocument<DocT> {
      return compSchema.CheckValidation(LoadBodyObject(doc), vx, out rule);
    }

    ///<inherit/>
    public bool CheckValidation(object body, object vx, out DocumentSchema.ValidationRule rule) => compSchema.CheckValidation(body, vx, out rule);

    ///<inherit/>
    public void EvaluateComputedFields<TCx>(TCx cx) where TCx : class, IExpressionCtx {
      compSchema.ComputeFieldFormulas(cx);
    }
  }

}