using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.Extensions.Options;

using Tlabs.Data.Entity;
using Tlabs.Data.Repo.Intern;
using Tlabs.Data.Entity.Intern;

using Xunit;
using Moq;
using Tlabs.Misc;

namespace Tlabs.Data.Processing.Tests {

  public class DocBodyCacheTest {
    class CacheOpt : IOptions<ObjCache.Options> {
      public ObjCache.Options Value => new ObjCache.Options {Warming= true};
    }
    class ObjCache : AbstractObjectCache<int, DocSchemaProcessorTest.TstDocument> {
      public ObjCache(IOptions<Options> opt) : base(opt) { }
      public override int GetKey(DocSchemaProcessorTest.TstDocument tmplObj) => tmplObj.Id;
    }

    class BodyCache : AbstractDocBodyCache<int, DocSchemaProcessorTest.TstDocument> {
      public BodyCache(IObjectCache<int, DocSchemaProcessorTest.TstDocument> cache,
                       IDocProcessorRepo docProcRepo,
                       IRepo<DocSchemaProcessorTest.TstDocument> docRepo) : base(cache, docProcRepo, docRepo) { }
      protected override DocSchemaProcessorTest.TstDocument ObtainMissingDocument(DocSchemaProcessorTest.TstDocument tmplObj) {
        ++obtainedMisDoc;
        return tmplObj;
      }
    }
    static int obtainedMisDoc;

    DocumentSchema docSchema;
    IDocSchemaProcessor docSchemaProc;
    DocSchemaProcessorTest.TstDocument doc;
    BodyCache bdyCache;
    public DocBodyCacheTest() {
      this.docSchema= DocSchemaProcessorTest.CreateDocSchema();
      this.docSchemaProc= DocSchemaProcessorTest.CreateDocSchemaProcessor(docSchema);
      this.doc= new DocSchemaProcessorTest.TstDocument(this.docSchema.TypeId) { Id= 123 };

      var docProcRepoMock= new Mock<IDocProcessorRepo>();
      DocSchemaProcessorTest.TstDocument currDoc= null;
      docProcRepoMock.Setup(r => r.LoadDocumentBodyObject(It.IsAny<DocSchemaProcessorTest.TstDocument>()))
                     .Callback<DocSchemaProcessorTest.TstDocument>(d => currDoc= d)
                     .Returns(()=> this.docSchemaProc.LoadBodyObject(currDoc));
      var docProcRepo= docProcRepoMock.Object;

      var storeMock= new Mock<IDataStore>();
      storeMock.Setup(s => s.LoadRelated<DocSchemaProcessorTest.TstDocument, BaseDocument<DocSchemaProcessorTest.TstDocument>.BodyData>(It.IsAny<IQueryable<DocSchemaProcessorTest.TstDocument>>(), It.IsAny<Expression<Func<DocSchemaProcessorTest.TstDocument, BaseDocument<DocSchemaProcessorTest.TstDocument>.BodyData>>>()))
               .Returns(() => new NoopEagerLoadedQueryable<DocSchemaProcessorTest.TstDocument, BaseDocument<DocSchemaProcessorTest.TstDocument>.BodyData>(EnumerableUtil.One<DocSchemaProcessorTest.TstDocument>(doc).AsQueryable()));

      var docRepoMock= new Mock<IRepo<DocSchemaProcessorTest.TstDocument>>();
      docRepoMock.Setup(r => r.Store).Returns(storeMock.Object);

      this.bdyCache= new BodyCache(new ObjCache(new CacheOpt()), docProcRepo, docRepoMock.Object);
    }

    [Fact]
    public void BasicTest() {
      var doc= new DocSchemaProcessorTest.TstDocument(this.docSchema.TypeId) { Id= 123 };

      obtainedMisDoc= 0;
      var bdy= bdyCache[doc];
      Assert.NotNull(bdy);
      Assert.Equal(1, obtainedMisDoc);
      Assert.NotNull(bdyCache[doc]);
      Assert.Equal(1, obtainedMisDoc);

      bdyCache[doc]= null;
      obtainedMisDoc= 0;
      Assert.NotNull(bdyCache[doc]);
      Assert.Equal(1, obtainedMisDoc);

      bdyCache.WarmUp();
    }

  }
}