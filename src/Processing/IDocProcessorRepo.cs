using System;
using Microsoft.Extensions.Logging;

using Tlabs.Misc;
using Tlabs.Sync;
using Tlabs.Data.Entity;

namespace Tlabs.Data.Processing {

  ///<summary>Interface of a <see cref="IDocSchemaProcessor"/> repository.</summary>
  public interface IDocProcessorRepo {

    ///<summary>Returns a <see cref="IDocSchemaProcessor"/> for <paramref name="sid"/> (DocumentSchema.TypeId).</summary>
    IDocSchemaProcessor GetDocumentProcessorBySid<TDoc>(string sid) where TDoc : Entity.Intern.BaseDocument<TDoc>;

    ///<summary>Returns a <see cref="IDocSchemaProcessor"/> for <paramref name="sid"/> (DocumentSchema.TypeId) and template context objects.</summary>
    IDocSchemaProcessor GetDocumentProcessorBySid<TDoc, TVx, TCx>(string sid, TVx vx, TCx cx)
      where TDoc : Entity.Intern.BaseDocument<TDoc>
      where TVx : class, IExpressionCtx
      where TCx : class, IExpressionCtx;

    ///<summary>Returns a <see cref="IDocSchemaProcessor"/> for <see cref="DocumentSchema"/>'s alternate name.</summary>
    IDocSchemaProcessor GetDocumentProcessorByAltName<TDoc>(string altName) where TDoc : Entity.Intern.BaseDocument<TDoc>;

    ///<summary>Returns a <see cref="IDocSchemaProcessor"/> for <see cref="DocumentSchema"/>'s alternate name and template context objects.</summary>
    IDocSchemaProcessor GetDocumentProcessorByAltName<TDoc, TVx, TCx>(string altName, TVx vx, TCx cx)
      where TDoc : Entity.Intern.BaseDocument<TDoc>
      where TVx : class, IExpressionCtx
      where TCx : class, IExpressionCtx;

    ///<summary>Returns a <see cref="IDocSchemaProcessor"/> for <paramref name="doc"/>.</summary>
    IDocSchemaProcessor GetDocumentProcessor<TDoc>(TDoc doc) where TDoc : Entity.Intern.BaseDocument<TDoc>;

    ///<summary>Returns a <see cref="IDocSchemaProcessor"/> for <paramref name="doc"/> and template context objects.</summary>
    IDocSchemaProcessor GetDocumentProcessor<TDoc, TVx, TCx>(TDoc doc, TVx vx, TCx cx)
      where TDoc : Entity.Intern.BaseDocument<TDoc>
      where TVx : class, IExpressionCtx
      where TCx : class, IExpressionCtx;

    ///<summary>Return <paramref name="doc"/>'s Body as object (according to its <see cref="DocumentSchema"/>).</summary>
    object LoadDocumentBodyObject<TDoc>(TDoc doc) where TDoc : Entity.Intern.BaseDocument<TDoc>;

    ///<summary>Update <paramref name="doc"/>'s body with <paramref name="bodyObj"/>.</summary>
    object UpdateDocumentBodyObject<TDoc>(TDoc doc, object bodyObj) where TDoc : Entity.Intern.BaseDocument<TDoc>;

    ///<summary>Create a new <see cref="IDocSchemaProcessor"/> for <paramref name="schema"/>.</summary>
    ///<exception cref="Tlabs.Dynamic.ExpressionSyntaxException">Thrown if any syntax error(s) in the validation code are detected.</exception>
    IDocSchemaProcessor CreateDocumentProcessor<TDoc>(DocumentSchema schema) where TDoc : Entity.Intern.BaseDocument<TDoc>;

    ///<summary>Create a new <see cref="IDocSchemaProcessor"/> for <paramref name="schema"/> and template context objects..</summary>
    ///<exception cref="Tlabs.Dynamic.ExpressionSyntaxException">Thrown if any syntax error(s) in the validation code are detected.</exception>
    IDocSchemaProcessor CreateDocumentProcessor<TDoc, TVx, TCx>(DocumentSchema schema, TVx vx, TCx cx)
      where TDoc : Entity.Intern.BaseDocument<TDoc>
      where TVx : class, IExpressionCtx
      where TCx : class, IExpressionCtx;
  }


}