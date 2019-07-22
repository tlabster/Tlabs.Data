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
    private DocSchemaRepo docSchemaRepo;
    private DocumentClassFactory docClassFactory;
    private IDocProcessorRepo repo;

    public DocProcessorRepoTest(MemoryDBEnvironment dBEnvironment) {
      this.dBEnvironment= dBEnvironment;
      this.docSchemaRepo= (DocSchemaRepo)dBEnvironment.svcProv.GetService(typeof(IDocSchemaRepo));
      this.docClassFactory= (DocumentClassFactory)dBEnvironment.svcProv.GetService(typeof(IDocumentClassFactory));
      var dynSerializer= JsonFormat.CreateDynSerializer();
      this.repo= new Processing.Intern.DocProcessorRepo(this.docSchemaRepo, this.docClassFactory, dynSerializer);
    }

    [Fact]
    void GetDocumentProcessorBySidTest() {
      var proc= repo.GetDocumentProcessorBySid<Document>("SAMPLE:1");
      Assert.Equal("SAMPLE:1", proc.Sid);
    }

    [Fact]
    void GetDocumentProcessorByAltNameTest() {
      var proc= repo.GetDocumentProcessorByAltName<Document>("ALT-SAMPLE:1");
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
