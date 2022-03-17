using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Tlabs.Data.Serialize;
using Tlabs.Data.Serialize.Xml;
using Tlabs.Data.Entity;
using Tlabs.Data.Processing.Tests;
using Xunit;
using Moq;

namespace Tlabs.Data.Repo.Tests {
  using XmlSchemaFormat = XmlFormat<DocumentSchema, Entity.Intern.DocXmlSchema>;

  public class DocSchemaRepoTest : IClassFixture<DocSchemaRepoTest.TestFixture> {
    TestFixture fix;
    public DocSchemaRepoTest(TestFixture fix) => this.fix= fix;
    public static readonly ISerializer<DocumentSchema> DocSchemaSerializer= new XmlSchemaFormat.Serializer(new XmlSchemaFormat());

    [Fact]
    void SchemaByTypeIdTest() {
      var schRepo= new DocSchemaRepo(fix.DataStore, DocSchemaSerializer);
      var schema= schRepo.GetByTypeId("basic-x:123");
      Assert.Equal("basic", schema.BaseType);
    }

    [Fact]
    void StreamsByTypeIdTest() {
      var schRepo= new DocSchemaRepo(fix.DataStore, DocSchemaSerializer);
      Assert.NotEmpty(schRepo.AllUntracked.ToList());
      var streams= schRepo.StreamsByTypeId("TST-DOC:0", false);
      Assert.NotNull(streams.Form);
      Assert.NotNull(streams.Style);
    }

    public class TestFixture {

      public IList<DocumentSchema> TestSchemas;
      public IDataStore DataStore;

      public TestFixture() {
        this.TestSchemas= new List<DocumentSchema> {
          DocSchemaProcessorTest.CreateTestSchema(),
          DocSchemaProcessorTest.CreateDocSchema()
        };

        var schemaFieldsQuery= new EagerLoadedQueryable<DocumentSchema, List<DocumentSchema.Field>>(this.TestSchemas.AsQueryable());
        var schemaValidsQuery= new EagerLoadedQueryable<DocumentSchema, List<DocumentSchema.ValidationRule>>(this.TestSchemas.AsQueryable());
        var schemaEvalRefs= new EagerLoadedQueryable<DocumentSchema, List<DocumentSchema.EvaluationRef>>(this.TestSchemas.AsQueryable());
        var storeMock= new Mock<IDataStore>();
        storeMock.Setup(s => s.LoadRelated<DocumentSchema, List<DocumentSchema.Field>>(It.IsAny<IQueryable<DocumentSchema>>(), It.IsAny<Expression<Func<DocumentSchema, List<DocumentSchema.Field>>>>()))
                 .Returns(schemaFieldsQuery);
        storeMock.Setup(s => s.LoadRelated<DocumentSchema, List<DocumentSchema.ValidationRule>>(It.IsAny<IQueryable<DocumentSchema>>(), It.IsAny<Expression<Func<DocumentSchema, List<DocumentSchema.ValidationRule>>>>()))
                 .Returns(schemaValidsQuery);
        storeMock.Setup(s => s.LoadRelated<DocumentSchema, List<DocumentSchema.EvaluationRef>>(It.IsAny<IQueryable<DocumentSchema>>(), It.IsAny<Expression<Func<DocumentSchema, List<DocumentSchema.EvaluationRef>>>>()))
                 .Returns(schemaEvalRefs);
        storeMock.Setup(s => s.UntrackedQuery<DocumentSchema>())
                 .Returns(this.TestSchemas.AsQueryable());

        this.DataStore= storeMock.Object;
      }
    }

    private class EagerLoadedQueryable<E, P> : IEagerLoadedQueryable<E, P> {
      private readonly IQueryable<E> q;
      public EagerLoadedQueryable(IQueryable<E> q) {
        this.q = q;
      }
      public Expression Expression => q.Expression;
      public Type ElementType => q.ElementType;
      public IQueryProvider Provider => q.Provider;
      public IEnumerator<E> GetEnumerator() => q.GetEnumerator();
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
  }
}
