using System;
using Microsoft.Extensions.Logging;

using Tlabs.Misc;
using Tlabs.Sync;
using Tlabs.Data.Entity;

namespace Tlabs.Data.Processing {

  ///<summary>Interface of a <see cref="IDocSchemaProcessor"/> repository.</summary>
  public interface IDocProcessorRepo {

    ///<summary>Returns a <see cref="IDocSchemaProcessor"/> for <paramref name="sid"/> (DocumentSchema.TypeId).</summary>
    IDocSchemaProcessor GetDocumentProcessorBySid<DocT>(string sid) where DocT : Entity.Intern.BaseDocument<DocT>;

    ///<summary>Returns a <see cref="IDocSchemaProcessor"/> for <see cref="DocumentSchema"/>'s alternate name.</summary>
    IDocSchemaProcessor GetDocumentProcessorByAltName<DocT>(string altName) where DocT : Entity.Intern.BaseDocument<DocT>;

    ///<summary>Returns a <see cref="IDocSchemaProcessor"/> for <paramref name="doc"/>.</summary>
    IDocSchemaProcessor GetDocumentProcessor<DocT>(DocT doc) where DocT : Entity.Intern.BaseDocument<DocT>;

    ///<summary>Return <paramref name="doc"/>'s Body as object (according to its <see cref="DocumentSchema"/>).</summary>
    object LoadDocumentBodyObject<DocT>(DocT doc) where DocT : Entity.Intern.BaseDocument<DocT>;

    ///<summary>Update <paramref name="doc"/>'s body with <paramref name="bodyObj"/>.</summary>
    object UpdateDocumentBodyObject<DocT>(DocT doc, object bodyObj) where DocT : Entity.Intern.BaseDocument<DocT>;

    ///<summary>Create a new <see cref="IDocSchemaProcessor"/> for <paramref name="schema"/>.</summary>
    ///<exception cref="Tlabs.Dynamic.ExpressionSyntaxException">Thrown if any syntax error(s) in the validation code are detected.</exception>
    IDocSchemaProcessor CreateDocumentProcessor<DocT>(DocumentSchema schema) where DocT : Entity.Intern.BaseDocument<DocT>;

  }


}