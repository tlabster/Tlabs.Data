using System;
using System.IO;

using Tlabs.Data.Serialize;
using Tlabs.Data.Entity;
using Tlabs.Data.Store;

namespace Tlabs.Data.Repo {

  ///<summary>Schema definitions from serialized <see cref="Stream"/>(s).</summary>
  public sealed class SchemaDefinitionStreams : IDisposable {
    ///<summary>Default ctor.</summary>
    public SchemaDefinitionStreams() { }
    ///<summary>Ctor from <paramref name="schema"/>.</summary>
    public SchemaDefinitionStreams(DocumentSchema schema, Stream schemaStream) {
      this.Schema= schemaStream;
      this.Form= null != schema.FormData ? new MemoryStream(schema.FormData, writable: false) : null;
      this.Style= null != schema.FormStyleData ? new MemoryStream(schema.FormStyleData, writable: false) : null;
      this.CalcModel= schema.CalcModelStream;
    }
    ///<summary>Schema definition (required).</summary>
    public Stream Schema { get; set; }
    ///<summary>Schema calculation model definition (optional).</summary>
    public Stream CalcModel { get; set; }
    ///<summary>Schema form definition (optional).</summary>
    public Stream Form { get; set; }
    ///<summary>Schema style definition (optional).</summary>
    public Stream Style { get; set; }

    ///<summary>Dispose all <see cref="Stream"/>(s).</summary>
    public void Dispose() {
      Schema?.Dispose();
      Schema= null;
      CalcModel?.Dispose();
      CalcModel= null;
      Form?.Dispose();
      Form= null;
      Style?.Dispose();
      Style= null;
    }

    ///<summary>Enum of schema data.</summary>
    public enum Data {
      ///<summary>Markup form data.</summary>
      Markup,
      ///<summary>Style form data.</summary>
      Style,
      ///<summary>Calc. model data.</summary>
      CalcModel,
      ///<summary>Schema data.</summary>
      Schema
    }
  }
}