using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Tlabs.Data;
using Tlabs.Data.Entity;
using Tlabs.Data.Entity.Intern;
using Tlabs.Data.Processing;
using Tlabs.Data.Repo;
using Tlabs.Data.Serialize;
using Tlabs.Data.Serialize.Json;

namespace Tlabs.Test.Common {
  public class Document : BaseDocument<Document> {

  }

  public class MemoryDBEnvironment : IDisposable {
    private ISerializer<DocumentSchema> schemaSeri;
    private IDocProcessorRepo procRepo;
    private DocSchemaRepo docSchemaRepo;
    public IServiceProvider svcProv { get; set; }
    public virtual IQueryable<Document> Documents { get { return new List<Document>().AsQueryable(); } }

    public IEnumerable<DocumentSchema> ChachedSchemas {
      get {
        var rscDir= new DirectoryInfo(Path.Combine(App.ContentRoot, "rsc"));
        foreach(var dir in rscDir.GetDirectories("*.xls.data")) {
          var xmlFile= dir.GetFiles("*.xml"); 
          if (1 != xmlFile.Length) throw new InvalidOperationException($"No XML schema found in {dir.FullName}");
          using (var xmlStrm= new FileStream(xmlFile[0].FullName, FileMode.Open, FileAccess.Read)) {
            yield return schemaSeri.LoadObj(xmlStrm);
          }
        }
      }
    }
    
    public MemoryDBEnvironment() {
      var svcColl= new ServiceCollection().AddLogging();

      AddConfigurators(svcColl);

      var emptyStore= new NoopStoreConfigurator.NoopDataStore();
      
      var prMock= new Mock<IRepo<Document>>();
      prMock.Setup(r => r.All).Returns(Documents);
      prMock.Setup(r => r.AllUntracked).Returns(Documents);
      prMock.Setup(r => r.Store).Returns(emptyStore);
      svcColl.AddSingleton(typeof(IRepo<Document>), prMock.Object);

      this.svcProv= svcColl.BuildServiceProvider();
      
      schemaSeri= (ISerializer<DocumentSchema>)svcProv.GetService(typeof(ISerializer<DocumentSchema>));
      procRepo= (IDocProcessorRepo)svcProv.GetService(typeof(IDocProcessorRepo));
      docSchemaRepo= (DocSchemaRepo)svcProv.GetService(typeof(IDocSchemaRepo));
      
      SeedSchemas();
    }

    public void SeedSchemas() {
      foreach(var schema in ChachedSchemas) {
        procRepo.CreateDocumentProcessor(schema);
        docSchemaRepo.Insert(schema);
      }
    }

    public void Dispose() {}

    protected virtual void AddConfigurators(IServiceCollection svcColl) {
      new NoopStoreConfigurator().AddTo(svcColl, Tlabs.Config.Empty.Configuration);
      new RepositoriesConfigurator().AddTo(svcColl, Tlabs.Config.Empty.Configuration);   //add Entity Repos
      new JsonFormat.Configurator().AddTo(svcColl, Tlabs.Config.Empty.Configuration);
    }
  }
}