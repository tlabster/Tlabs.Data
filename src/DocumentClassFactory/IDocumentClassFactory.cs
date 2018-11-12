using System;
using Tlabs.Data.Entity;

namespace Tlabs.Data {
  /// <summary>
  /// Factory to generate dynamic classes from a document schema
  /// </summary>
  public interface IDocumentClassFactory {

    /// <summary>
    /// Gets the Type for a document schema using the already instantiated schema object
    /// </summary>
    /// <param name="documentSchema">Document Schema</param>
    /// <returns></returns>
    Type GetBodyType(DocumentSchema documentSchema);


    /// <summary>
    /// Gets the Type for a document schema using the cached version of a schema or querying the database
    /// </summary>
    /// <param name="typeId">Unique identifier of the schema type</param>
    /// <returns></returns>
    Type GetBodyType(string typeId);

    ///<summary>Creates a new empty instance of a document body.</summary>
    ///<remarks>
    ///All fields of the document body instance are initialized with their default value, except for fields of
    ///type string, which get initialized with <see cref="System.String.Empty"/> (instead of null).
    ///</remarks>
    object CreateEmptyBody(DocumentSchema documentSchema);
  }
}