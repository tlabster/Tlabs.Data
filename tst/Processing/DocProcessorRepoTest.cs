using Microsoft.Extensions.DependencyInjection;

using Tlabs.Test.Common;
using Tlabs.Data.Processing;
using Tlabs.Data.Repo;
using Tlabs.Data.Serialize.Json;
using Moq;
using Xunit;

namespace Tlabs.Data.Intern.Tests {
  [CollectionDefinition("MemoryDB")]
  public class DBCollection : ICollectionFixture<MemoryDBEnvironment> {
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
  }

  [Collection("MemoryDB")]
  public class DocProcessorRepoTest {
    private MemoryDBEnvironment dBEnvironment;
    private IDocSchemaRepo docSchemaRepo;
    private DocumentClassFactory docClassFactory;
    private IDocProcessorRepo repo;

    public DocProcessorRepoTest(MemoryDBEnvironment dBEnvironment) {
      this.dBEnvironment= dBEnvironment;
      this.docSchemaRepo= dBEnvironment.svcProv.GetService<IDocSchemaRepo>();
      this.docClassFactory= dBEnvironment.svcProv.GetService<DocumentClassFactory>();
      var dynSerializer= JsonFormat.CreateDynSerializer();
      var descResolver= dBEnvironment.svcProv.GetService<Processing.SchemaCtxDescriptorResolver>();
      this.repo= new Processing.Intern.DocProcessorRepo(this.docSchemaRepo, this.docClassFactory, dynSerializer, descResolver);
    }

    [Fact]
    void GetDocumentProcessorBySidTest() {
      var proc= repo.GetDocumentProcessorBySid("SAMPLE:1");
      Assert.Equal("SAMPLE:1", proc.Sid);
    }

    [Fact]
    void GetDocumentProcessorByAltNameTest() {
      var proc= repo.GetDocumentProcessorByAltName("ALT-SAMPLE:1");
      Assert.Equal("SAMPLE:1", proc.Sid);
    }

    [Fact]
    void GetDocumentProcessorTest() {
      var doc= new Document();
      doc.Sid= "SAMPLE:1";

      var proc= repo.GetDocumentProcessor<Document>(doc);
      Assert.Equal("SAMPLE:1", proc.Sid);
    }

    [Fact]
    void LoadDocumentBodyObjectTest() {
      var doc= new Document();
      doc.Sid= "SAMPLE:1";

      var proc= repo.GetDocumentProcessor<Document>(doc);
      var body= repo.LoadDocumentBodyObject<Document>(doc);
      Assert.IsAssignableFrom(proc.BodyType, body);
    }

    [Fact]
    void UpdateDocumentBodyObjectTest() {
      var docMock= new Mock<Document>();
      docMock.Object.Sid= "SAMPLE:1";

      var proc= repo.GetDocumentProcessor<Document>(docMock.Object);
      dynamic body= proc.EmptyBody;
      body.Active= true;
      repo.UpdateDocumentBodyObject<Document>(docMock.Object, body);
      docMock.Verify(m => m.SetBodyObject(It.IsAny<object>()));
    }
  }
}
