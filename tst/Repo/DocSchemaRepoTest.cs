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
      Assert.NotEmpty(schRepo.FilteredTypeIdList(":0"));
      var schema= schRepo.GetByTypeId("basic-x:123");
      Assert.Equal("basic", schema.BaseType);
      var altSchema= schRepo.GetByAltTypeName("alt-basic-z:123");
      Assert.Equal("basic", altSchema.BaseType);
      Assert.False(schRepo.TryGetByTypeId("undefined:1", out var noSchema));
      Assert.False(schRepo.TryGetByAltTypeName("undefined:1", out noSchema));

      Assert.Same(schema, schRepo.Update(schema));
    }

    [Fact]
    void StreamsByTypeIdTest() {
      var schRepo= new DocSchemaRepo(fix.DataStore, DocSchemaSerializer);
      Assert.NotEmpty(schRepo.AllUntracked.ToList());
      var streams= schRepo.StreamsByTypeId("TST-DOC:0", true);
      Assert.NotNull(streams.Schema);
      Assert.NotNull(streams.Form);
      Assert.NotNull(streams.Style);

      Assert.Equal(streams.Form.Length, schRepo.FormData("TST-DOC:0", FormDataType.Markup).Length);
      Assert.Equal(streams.Style.Length, schRepo.FormData("TST-DOC:0", FormDataType.Style).Length);
      var schema= schRepo.GetByTypeId("TST-DOC:0");
      Assert.NotNull(schRepo.Get(schema.Id));

      schRepo.Delete(schRepo.GetByTypeId("TST-DOC:0"));
      Assert.False(schRepo.TryGetByTypeId("TST-DOC:0", out var noSchema));
      schRepo.Evict(schRepo.GetByTypeId("basic-x:123"));
      schRepo.Attach(schRepo.GetByTypeId("basic-x:123"));

      schema= schRepo.CreateFromStreams(DocSchemaProcessorTest.CreateDocSchemaProcessor, streams.Schema, streams.Form, streams.Style);
      Assert.Equal("TST", schema.BaseType);
    }

    public class TestFixture {
      static int instanceCnt= 0;

      public IList<DocumentSchema> TestSchemas;
      public IDataStore DataStore;

      public TestFixture() {
        if (++instanceCnt > 1) throw new InvalidOperationException("Must be created only once.");
        this.TestSchemas= new List<DocumentSchema> {
          DocSchemaProcessorTest.CreateTestSchema(),
          DocSchemaProcessorTest.CreateDocSchema()
        };

        var storeMock= new Mock<IDataStore>();
        storeMock.Setup(s => s.LoadRelated<DocumentSchema, List<DocumentSchema.Field>>(It.IsAny<IQueryable<DocumentSchema>>(), It.IsAny<Expression<Func<DocumentSchema, List<DocumentSchema.Field>>>>()))
                 .Returns(()=> new NoopEagerLoadedQueryable<DocumentSchema, List<DocumentSchema.Field>>(this.TestSchemas.AsQueryable()));
        storeMock.Setup(s => s.LoadRelated<DocumentSchema, List<DocumentSchema.ValidationRule>>(It.IsAny<IQueryable<DocumentSchema>>(), It.IsAny<Expression<Func<DocumentSchema, List<DocumentSchema.ValidationRule>>>>()))
                 .Returns(()=> new NoopEagerLoadedQueryable<DocumentSchema, List<DocumentSchema.ValidationRule>>(this.TestSchemas.AsQueryable()));
        storeMock.Setup(s => s.LoadRelated<DocumentSchema, List<DocumentSchema.EvaluationRef>>(It.IsAny<IQueryable<DocumentSchema>>(), It.IsAny<Expression<Func<DocumentSchema, List<DocumentSchema.EvaluationRef>>>>()))
                 .Returns(()=> new NoopEagerLoadedQueryable<DocumentSchema, List<DocumentSchema.EvaluationRef>>(this.TestSchemas.AsQueryable()));
        storeMock.Setup(s => s.UntrackedQuery<DocumentSchema>())
                 .Returns(()=> this.TestSchemas.AsQueryable());
        DocumentSchema dSch= null;
        storeMock.Setup(s => s.Update<DocumentSchema>(It.IsAny<DocumentSchema>()))
                 .Callback((DocumentSchema s) => dSch= s)
                 .Returns(()=> dSch);
        storeMock.Setup(s => s.Attach<DocumentSchema>(It.IsAny<DocumentSchema>()))
                 .Callback((DocumentSchema s) => dSch= s)
                 .Returns(() => dSch);
        storeMock.Setup(s => s.Insert<DocumentSchema>(It.IsAny<DocumentSchema>()))
                 .Callback((DocumentSchema s) => dSch= s)
                 .Returns(() => dSch);
        storeMock.Setup(s => s.Delete<DocumentSchema>(It.IsAny<DocumentSchema>()))
                 .Callback((DocumentSchema s) => {
                   this.TestSchemas= new List<DocumentSchema> { this.TestSchemas[1] };
                 });
        storeMock.Setup(s => s.Evict<DocumentSchema>(It.IsAny<DocumentSchema>()));


        this.DataStore= storeMock.Object;
      }
    }

  }
}
