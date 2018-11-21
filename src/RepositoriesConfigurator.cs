using System;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using Tlabs.Config;
using Tlabs.Data.Entity;
using Tlabs.Data.Repo;
using Tlabs.Data.Serialize;
using Tlabs.Data.Serialize.Json;
using Tlabs.Data.Serialize.Xml;

namespace Tlabs.Data {

  ///<summary>Configures all data repositories as services.</summary>
  public class RepositoriesConfigurator : IConfigurator<IServiceCollection> {
    ///<summary>Default time zone</summary>
    public const string DEFAULT_TIME_ZONE= "W. Europe Standard Time";   //TODO: this is probably windows only

    private ILogger log= App.Logger<RepositoriesConfigurator>();

    ///<inherit/>
    public void AddTo(IServiceCollection services, IConfiguration cfg) {


      services.AddScoped(typeof(IRepo<>), typeof(Repo.Intern.BaseRepo<>));
      configureCustomRepos(services);
      services.AddSingleton<XmlFormat<DocumentSchema, Entity.Intern.DocXmlSchema>>();
      services.AddSingleton<ISerializer<DocumentSchema>, XmlFormat<DocumentSchema, Entity.Intern.DocXmlSchema>.Serializer>();

      services.AddSingleton<JsonSchemaFormat<SerializationSchema>>();
      services.AddSingleton<JsonSchemaFormat<SerializationSchema>.SchemaSerializer>();
      services.AddSingleton<SensitiveJsonSchemaFormat<SerializationSchema>>();
      services.AddSingleton<SensitiveJsonSchemaFormat<SerializationSchema>.SchemaSerializer>();

      configureDocProcessor(services);
      log.LogDebug("Repository services added.");
    }

    private void configureDocProcessor(IServiceCollection services) {
      services.TryAddSingleton<IDocumentClassFactory, DocumentClassFactory>();
      services.AddScoped(typeof(Processing.DocProcessorRepo));

      string tzid= null;
      // config.TryGetValue("timeZone", out tzid);
      tzid= tzid ?? DEFAULT_TIME_ZONE;

      /* TODO: To become OS independent it would be better to use
       *       TimeZoneInfo.FromSerializedString() / ToSerilaizedString() but these are available only starting from .NET Core 2.0 ...
       */
      TimeZoneInfo timeZoneInfo;
      try {
        timeZoneInfo= TimeZoneInfo.FindSystemTimeZoneById(tzid);
      }
      catch (Exception e) {
        log.LogWarning(0, e, "Time-zone {tz} not available on this system - falling back to UTC !!!", tzid);
        timeZoneInfo= TimeZoneInfo.Utc;
      }
      App.TimeInfo= new DateTimeHelper(timeZoneInfo);
      log.LogWarning("Application time zone: '{id}'", timeZoneInfo.Id);


    }
    private void configureCustomRepos(IServiceCollection services) {
#if false
      var thisAss= this.GetType().GetTypeInfo().Assembly;
      var repo= typeof(Repo.Intern.BaseRepo<>).GetTypeInfo();
      var repoNsp= repo.Namespace.Substring(0, repo.Namespace.LastIndexOf('.')); //parent nsp

      foreach (var t in thisAss.GetTypes()) {
        var ti= t.GetTypeInfo();
        if (!ti.IsClass) continue;
        if (repoNsp == t.Namespace) {
          services.AddScoped(t);
        }
      }
#endif
      services.AddScoped<IDocSchemaRepo, DocSchemaRepo>();
    }
    
  }
}