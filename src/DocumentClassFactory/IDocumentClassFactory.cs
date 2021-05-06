using System;
using Tlabs.Data.Entity;

namespace Tlabs.Data {
  /// <summary>
  /// Factory to generate dynamic classes from a document schema
  /// </summary>
  public interface IDocumentClassFactory {

    /// <summary>Create the body type class for <paramref name="documentSchema"/>.</summary>
    ///<remarks>Replaces any already cached type class.</remarks>
    /// <param name="documentSchema">Document Schema</param>
    /// <returns></returns>
    Type CreateBodyType(DocumentSchema documentSchema);

    /// <summary>Gets the body type for a document schema with <paramref name="sid"/> using a cached version or compiling a new class.</summary>
    Type GetBodyType(string sid);

  }
}