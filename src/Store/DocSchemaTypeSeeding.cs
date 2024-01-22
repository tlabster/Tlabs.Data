using System;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Logging;

using Tlabs.Data.Entity;

namespace Tlabs.Data.Store {

  ///<summary>Master type data seeding.</summary>
  public class DocSchemaTypeSeeding : AbstractDataSeed {
    readonly Repo.IDocSchemaRepo schemaRepo;
    readonly Serialize.ISerializer<Entity.DocumentSchema> schemaSeri;
    readonly ILogger<DocSchemaTypeSeeding> log;

    ///<summary>Ctor from <paramref name="store"/>.</summary>
    public DocSchemaTypeSeeding(IDataStore store, Repo.IDocSchemaRepo schemaRepo, Serialize.ISerializer<Entity.DocumentSchema> schemaSeri, ILogger<DocSchemaTypeSeeding> log) : base(store) {
      this.schemaRepo= schemaRepo;
      this.schemaSeri= schemaSeri;
      this.log= log;
    }

    ///<inheritdoc/>
    public override string Campaign => "Schema type data";

    ///<inheritdoc/>
    public override void Perform() {
      if (!schemaRepo.AllUntracked.Any())
        SeedDocumentSchemas();
    }

    private void SeedDocumentSchemas() {
      var rscDir= new DirectoryInfo(Path.Combine(App.ContentRoot, "rsc"));

      foreach (var dir in rscDir.GetDirectories("*.xls.data")) {
        log.LogWarning("Seeding {schema} from: {dir}", nameof(DocumentSchema), dir.FullName);

        var xmlFile= dir.GetFiles("*.xml");
        if (1 != xmlFile.Length) throw new AppConfigException($"Exactly ONE XML schema file required in {dir.FullName}");
        using var xmlStrm= File.OpenRead(xmlFile[0].FullName);
        var schema= schemaSeri.LoadObj(xmlStrm);
        if (null == schema) throw EX.New<AppConfigException>("Invalid document schema in {file}", xmlFile[0].FullName);

        var htmlFile= dir.GetFiles("*.htm");
        if (htmlFile.Length > 0) {
          schema.FormData= File.ReadAllBytes(htmlFile[0].FullName);
          if (htmlFile.Length > 1) log.LogWarning("Only {file} of multiple HTML forms loaded!", htmlFile[0].Name);
        }
        else log.LogWarning("No HTML form in {dir}", dir.FullName);

        var cssFile= dir.GetFiles("*.css");
        if (cssFile.Length > 0) {
          schema.FormStyleData= File.ReadAllBytes(cssFile[0].FullName);
          if (cssFile.Length > 1) log.LogWarning("Only {file} of multiple CSS files loaded!", htmlFile[0].Name);
        }
        else log.LogWarning("No CSS file in {dir}", dir.FullName);

        var xlsFile= dir.GetFiles("*.xls");
        if (xlsFile.Length > 1) {
          schema.CalcModelData= File.ReadAllBytes(xlsFile[0].FullName);
          if (xlsFile.Length > 1) log.LogWarning("Only {file} of multiple XLS files load!", xlsFile[0].FullName);
        }
        else log.LogInformation("No calc.model (xls) in {dir}", dir.FullName);

        schemaRepo.Insert(schema);
      }//foreach *.data dir
      store.CommitChanges();
    }

  }
}