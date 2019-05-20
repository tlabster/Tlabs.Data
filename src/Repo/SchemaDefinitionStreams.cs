using System;
using System.IO;

using Tlabs.Data.Serialize;
using Tlabs.Data.Entity;
using Tlabs.Data.Store;

namespace Tlabs.Data.Repo {

  ///<summary>Schema definitions from serialized <see cref="Stream"/>(s).</summary>
  public class SchemaDefinitionStreams : IDisposable {
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
  }
}