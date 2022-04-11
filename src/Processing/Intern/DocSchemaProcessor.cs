using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Logging;

using Tlabs.Data.Entity;
using Tlabs.Data.Entity.Intern;
using Tlabs.Data.Serialize;
using Tlabs.Dynamic;

namespace Tlabs.Data.Processing.Intern {

  /// <summary><see cref="DocumentSchema"/> processor.</summary>
  public class DocSchemaProcessor : IDocSchemaProcessor {
    /// <summary><see cref="ILogger"/>.</summary>
    protected static readonly ILogger<DocSchemaProcessor> log= App.Logger<DocSchemaProcessor>();

    /// <summary>Schema id.</summary>
    protected ICompiledDocSchema compSchema;
    readonly IDynamicSerializer docSeri;


    /// <summary>Ctor from <paramref name="compSchema"/> and <paramref name="docSeri"/>.</summary>
    public DocSchemaProcessor(ICompiledDocSchema compSchema, IDynamicSerializer docSeri) {
      if (null == (this.compSchema= compSchema)) throw new ArgumentNullException(nameof(compSchema));
      if (null == (this.docSeri= docSeri)) throw new ArgumentNullException(nameof(docSeri));
      log.LogDebug("Created new DocSchemaProcessor({sid}) for bodyType: {bodyType}.", compSchema.Sid, compSchema.BodyType.Name);
    }

    ///<inherit/>
    public DocumentSchema Schema => compSchema.Schema;
    ///<inherit/>
    public string Sid => compSchema.Sid;
    ///<inherit/>
    public Type BodyType => compSchema.BodyType;
    ///<inherit/>
    public DynamicAccessor BodyAccessor => compSchema.BodyAccessor;
    ///<inherit/>
    public IReadOnlyDictionary<string, Type> EvalTypeIndex => compSchema.EvalTypeIdx;
    ///<inherit/>
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

    ///<inherit/>
    public object LoadBodyObject<DocT>(DocT doc) where DocT : BaseDocument<DocT> {
      checkDocument(doc);
      return doc.GetBodyObject((body)
        =>   null != body.Data
           ? docSeri.LoadObj(new MemoryStream(body.Data), BodyType)
           : EmptyBody
      );
    }

    ///<inherit/>
    public IDictionary<string, object> LoadBodyProperties<TDoc>(TDoc doc) where TDoc : BaseDocument<TDoc>
      => BodyAccessor.ToDictionary(LoadBodyObject(doc));

    ///<inherit/>
    public object UpdateBodyObject<DocT>(DocT doc, object bodyObj, Func<object, IDictionary<string, object>> setupData= null, int bufSz= 10*1024) where DocT : BaseDocument<DocT> {
      checkDocument(doc);

      var body= doc.Body;

      if (!BodyType.GetTypeInfo().IsAssignableFrom(bodyObj.GetType())) {
        /* coerce obj into bodyType !!!
          */
        using var strm= new MemoryStream(bufSz);
        docSeri.WriteObj(strm, bodyObj);
        body.Data= strm.ToArray();
        body.Encoding= docSeri.Encoding;

        strm.Position= 0;
        bodyObj= docSeri.LoadObj(strm, BodyType);
      }

      processBodyObject(bodyObj, setupData);

      using (var strm= new MemoryStream(bufSz)) {
        strm.SetLength(0);
        strm.Position= 0;
        docSeri.WriteObj(strm, bodyObj);
        body.Data= strm.ToArray();
        body.Encoding= docSeri.Encoding;
      }

      // Touch doc modified date
      doc.Modified= App.TimeInfo.Now;

      return doc.SetBodyObject(bodyObj);
    }

    ///<inheritdoc/>
    public object MergeBodyProperties<TDoc>(TDoc doc, IEnumerable<KeyValuePair<string, object>> props, ISchemaEvalContext cx= null) where TDoc : BaseDocument<TDoc> {
      if (null == props) return props;
      var body= LoadBodyObject(doc);
      if (body.GetType() != BodyType) log.LogWarning("Doc. body type {bt} not matching processor type: {pt}", body.GetType(), BodyType);
      var bodyProps= BodyAccessor.ToDictionary(body);
      log.LogDebug("BodyAccessor.ToDictionary({bt}) succeeded.", body.GetType());
      foreach (var pair in props) try{
        bodyProps[pair.Key]= pair.Value;
      }
      catch (Exception e) { log.LogDebug(e, "Failed to assign prop: {pname}, (type: {type})", pair.Key, pair.Value?.GetType()); throw;}

      if (cx is not NoEvaluationContext) {
        var ecx= cx ?? new DefaultSchemaEvalContext(body);
        ecx.SetBody(body);
        if (!CheckValidation(body, ecx, out var offendingRule))
          throw new FieldValidationException(offendingRule);
        EvaluateComputedFields(ecx);
      }
      return UpdateBodyObject(doc, body);
    }
    private void checkDocument<DocT>(DocT doc) where DocT : BaseDocument<DocT> {
      if (null == doc) throw new ArgumentNullException(nameof(doc));
      if (null == doc.Body) throw new ArgumentException(nameof(doc.Body));
      if (this.Sid != doc.Sid) throw new ArgumentException(nameof(doc.Sid));
    }

    ///<inherit/>
    protected virtual object processBodyObject(object bodyObj, Func<object, IDictionary<string, object>> setupData) => bodyObj;

    ///<inherit/>
    public void ApplyValidation<DocT>(DocT doc, ISchemaEvalContext ecx, out object body) where DocT : BaseDocument<DocT> {
      body= LoadBodyObject(doc);
      doc.StatusDetails= null;
      doc.Status= BaseDocument<DocT>.State.VALID.ToString();
      if (!compSchema.CheckValidation(body, ecx, out var rule)) {
        doc.StatusDetails= $"{rule.Key} - {rule.Description}";
        doc.Status= BaseDocument<DocT>.State.IMPLAUSIBLE.ToString();
      }
    }

    ///<inherit/>
    public bool CheckValidation<DocT>(DocT doc, ISchemaEvalContext ecx, out DocumentSchema.ValidationRule rule) where DocT : BaseDocument<DocT> {
      return compSchema.CheckValidation(LoadBodyObject(doc), ecx, out rule);
    }

    ///<inherit/>
    public bool CheckValidation(object body, ISchemaEvalContext ecx, out DocumentSchema.ValidationRule rule) => compSchema.CheckValidation(body, ecx, out rule);

    ///<inherit/>
    public void EvaluateComputedFields(ISchemaEvalContext ecx) => compSchema.ComputeFieldFormulas(ecx);
  }

}