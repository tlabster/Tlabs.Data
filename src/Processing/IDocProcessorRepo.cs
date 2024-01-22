using System.Collections.Generic;
using Tlabs.Data.Entity;

namespace Tlabs.Data.Processing {

  ///<summary>Interface of a <see cref="IDocSchemaProcessor"/> repository.</summary>
  public interface IDocProcessorRepo {

    ///<summary><see cref="Repo.IDocSchemaRepo"/>.</summary>
    Repo.IDocSchemaRepo SchemaRepo { get; }

    ///<summary>Returns a <see cref="IDocSchemaProcessor"/> for <paramref name="sid"/> (DocumentSchema.TypeId).</summary>
    IDocSchemaProcessor GetDocumentProcessorBySid(string sid);

    ///<summary>Returns a <see cref="IDocSchemaProcessor"/> for <see cref="DocumentSchema"/>'s alternate name.</summary>
    IDocSchemaProcessor GetDocumentProcessorByAltName(string altName);

    ///<summary>Returns a <see cref="IDocSchemaProcessor"/> for <paramref name="doc"/>.</summary>
    IDocSchemaProcessor GetDocumentProcessor<TDoc>(TDoc doc) where TDoc : Entity.Intern.BaseDocument<TDoc>;

    ///<summary>Returns a <see cref="SchemaEvalCtxProcessor"/> for <paramref name="sid"/> (DocumentSchema.TypeId).</summary>
    SchemaEvalCtxProcessor GetSchemaEvalCtxProcessor(string sid);

    ///<summary>Return <paramref name="doc"/>'s Body as object (according to its <see cref="DocumentSchema"/>).</summary>
    object LoadDocumentBodyObject<TDoc>(TDoc doc) where TDoc : Entity.Intern.BaseDocument<TDoc>;

    ///<summary>Return <paramref name="doc"/> body's properties (according to its <see cref="DocumentSchema"/>).</summary>
    IDictionary<string, object?> LoadBodyProperties<TDoc>(TDoc doc) where TDoc : Entity.Intern.BaseDocument<TDoc>;

    ///<summary>Update <paramref name="doc"/>'s body with <paramref name="bodyObj"/>.</summary>
    object UpdateDocumentBodyObject<TDoc>(TDoc doc, object bodyObj) where TDoc : Entity.Intern.BaseDocument<TDoc>;

    ///<summary>Create a new <see cref="IDocSchemaProcessor"/> for <paramref name="schema"/> (with optional expression eval. contexts).</summary>
    ///<exception cref="Tlabs.Dynamic.ExpressionSyntaxException">Thrown if any syntax error(s) in the validation code are detected.</exception>
    IDocSchemaProcessor CreateDocumentProcessor(DocumentSchema schema);

  }

}