using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using Tlabs.Config;
using Tlabs.Data.Entity;
using Tlabs.Data.Repo;
using Tlabs.Data.Serialize;
using Tlabs.Data.Serialize.Xml;

namespace Tlabs.Data {

  ///<summary>Configures all data repositories as services.</summary>
  public class RepositoriesConfigurator : IConfigurator<IServiceCollection> {
    ///<summary>Default windows time zone</summary>
    public static readonly string CET_ZONE_ID=   RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                                               ? "W. Europe Standard Time"
                                               : "Europe/Berlin";
    readonly ILogger log= App.Logger<RepositoriesConfigurator>();
    readonly IDictionary<string, string> config;

    ///<summary>Default ctor.</summary>
    public RepositoriesConfigurator() : this(null) { }

    ///<summary>Ctor from <paramref name="config"/>.</summary>
    public RepositoriesConfigurator(IDictionary<string, string>? config) {
      this.config= config ?? ImmutableDictionary<string, string>.Empty;
    }

    ///<inheritdoc/>
    public void AddTo(IServiceCollection services, IConfiguration cfg) {


      services.AddScoped(typeof(IRepo<>), typeof(Repo.Intern.BaseRepo<>));
      services.AddScoped(typeof(ICachedRepo<>), typeof(Repo.Intern.CachedRepo<>));
      configureCustomRepos(services);
      services.AddSingleton<XmlFormat<DocumentSchema, Entity.Intern.DocXmlSchema>>();
      services.AddSingleton<ISerializer<DocumentSchema>, XmlFormat<DocumentSchema, Entity.Intern.DocXmlSchema>.Serializer>();
      services.TryAddSingleton<IDocumentClassFactory, DocumentClassFactory>();
      services.TryAddSingleton<Processing.SchemaCtxDescriptorResolver>();
      services.AddScoped<Processing.IDocProcessorRepo, Processing.Intern.DocProcessorRepo>();


      // services.AddSingleton<JsonSchemaFormat<SerializationSchema>>();
      // services.AddSingleton<JsonSchemaFormat<SerializationSchema>.SchemaSerializer>();
      // services.AddSingleton<SensitiveJsonSchemaFormat<SerializationSchema>>();
      // services.AddSingleton<SensitiveJsonSchemaFormat<SerializationSchema>.SchemaSerializer>();

      configureAppTime();
      log.LogDebug("Repository services added.");
    }

    private void configureAppTime() {
      var timeZoneInfo= TimeZoneInfo.Utc;
      config.TryGetValue("timeZone", out var tzid);
      if (0 == string.Compare("CET", tzid, StringComparison.OrdinalIgnoreCase)) tzid= CET_ZONE_ID;
      else if (0 == string.Compare("LOCAL", tzid, StringComparison.OrdinalIgnoreCase)) {
        timeZoneInfo= TimeZoneInfo.Local;
        tzid= null;
      }

      try {
        /* TODO: Use TimeZoneInfo.FromSerializedString() / ToSerilaizedString() but these are available only starting from .NET Core 2.0 ...
         */
        if (!string.IsNullOrEmpty(tzid)) timeZoneInfo= TimeZoneInfo.FindSystemTimeZoneById(tzid);
      }
      catch (Exception e) {
        log.LogWarning(0, e, "Time-zone {tz} not available on this system - falling back to UTC !!!", tzid);
        timeZoneInfo= TimeZoneInfo.Utc;
      }
      App.Setup= App.Setup with { TimeInfo= new DateTimeHelper(timeZoneInfo) };
      log.LogWarning("Application time zone: '{id}'", timeZoneInfo.Id);

    }
    static void configureCustomRepos(IServiceCollection services) {
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