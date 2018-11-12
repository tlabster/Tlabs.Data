using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Tlabs.Config;
using Tlabs.Data.Store;

namespace Tlabs.Data.Repo.Intern {

  ///<summary>Configure a no persitence <see cref="IDataStore"/>.</summary>
  public class NoPersistenceStoreConfigurator : IConfigurator<IServiceCollection> {

    ///<inherit/>
    public void AddTo(IServiceCollection services, IConfiguration cfg) {
      var log= App.Logger<NoPersistenceStoreConfigurator>();
      services.AddScoped(typeof(IDataStore), typeof(NoDataStore));
      log.LogWarning("NO persistend storage applied.");
    }

    ///<summary>Non storing data store.<see cref="IDataStore"/>.</summary>
    public class NoDataStore : IDataStore {
      private ILogger<NoDataStore> log;

      ///<summary>Ctor from <paramref name="log"/>.</summary>
      public NoDataStore(ILogger<NoDataStore> log) {
        this.log= log;
      }
      ///<inherit/>
      public bool AutoCommit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      ///<inherit/>
      public void Attach<TEntity>(TEntity ent) where TEntity : class =>  throw new NotImplementedException();

      ///<inherit/>
      public void CommitChanges() => throw new NotImplementedException();

      ///<inherit/>
      public void Delete<TEntity>(TEntity ent) where TEntity : class => throw new NotImplementedException();

      ///<inherit/>
      public void Dispose() { }

      ///<inherit/>
      public void EnsureStore(IEnumerable<IDataSeed> seeds) => log.LogInformation("Confirmed to NOT provide ANY storage facility.");

      ///<inherit/>
      public void Evict<TEntity>(TEntity ent) where TEntity : class => throw new NotImplementedException();

      ///<inherit/>
      public TEntity Get<TEntity>(params object[] keys) where TEntity : class => throw new NotImplementedException();

      ///<inherit/>
      public object GetIdentifier<TEntity>(TEntity ent) where TEntity : class => throw new NotImplementedException();

      ///<inherit/>
      public void Insert<TEntity>(TEntity ent) where TEntity : class => throw new NotImplementedException();

      ///<inherit/>
      public TEntity Merge<TEntity>(TEntity entity) where TEntity : class, new() => throw new NotImplementedException();

      ///<inherit/>
      public IQueryable<TEntity> Query<TEntity>() where TEntity : class => throw new NotImplementedException();

      ///<inherit/>
      public void ResetChanges() => throw new NotImplementedException();

      ///<inherit/>
      public IQueryable<TEntity> UntrackedQuery<TEntity>() where TEntity : class => throw new NotImplementedException();

      ///<inherit/>
      public void Update<TEntity>(TEntity ent) where TEntity : class => throw new NotImplementedException();

      ///<inherit/>
      public void LoadExplicit<E, P>(E entity, Expression<Func<E, P>> prop) where E : class where P : class => throw new NotImplementedException();

      ///<inherit/>
      public void LoadExplicit<E, P>(E entity, Expression<Func<E, IEnumerable<P>>> prop) where E : class where P : class => throw new NotImplementedException();

      ///<inherit/>
      public IQueryable<E> LoadRelated<E>(IQueryable<E> query, string navigationPropertyPath) where E : class => throw new NotImplementedException();

      ///<inherit/>
      public IEagerLoadedQueryable<E, P> LoadRelated<E, P>(IQueryable<E> query, Expression<Func<E, P>> navProperty) where E : class => throw new NotImplementedException();

      ///<inherit/>
      public IEagerLoadedQueryable<E, Prop> ThenLoadRelated<E, Prev, Prop>(IEagerLoadedQueryable<E, IEnumerable<Prev>> query, Expression<Func<Prev, Prop>> navProperty) where E : class
         => throw new NotImplementedException();

      ///<inherit/>
      public IEagerLoadedQueryable<E, Prop> ThenLoadRelated<E, Prev, Prop>(IEagerLoadedQueryable<E, Prev> query, Expression<Func<Prev, Prop>> navProperty) where E : class
         => throw new NotImplementedException();
    }
  }
}