using Tlabs.Test.Common;
using Xunit;

namespace Tlabs.Data.Intern.Tests {
  [Collection("MemoryDB")]
  public class BaseDocumentTest {
    private MemoryDBEnvironment memoryDBEnvironment;

    public BaseDocumentTest(MemoryDBEnvironment memoryDBEnvironment) {
      this.memoryDBEnvironment= memoryDBEnvironment;
    }

    [Fact]
    void IsValidTest() {
      var doc = new Document();
      doc.Status= Document.State.VALID.ToString();

      Assert.True(doc.IsValid);
    }

    [Fact]
    void IsInvalidTest() {
      var doc = new Document();
      doc.Status= Document.State.DISABLED.ToString();

      Assert.False(doc.IsValid);
    }

    [Fact]
    void EmptyBodyTest() {
      var doc = new Document();

      Assert.NotNull(doc.Body);
      Assert.Equal(doc.Body.Document, doc);
      Assert.Null(doc.Body.Data);
    }
  }
}
