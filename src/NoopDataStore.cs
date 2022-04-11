using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Tlabs.Config;
using System.Collections;

namespace Tlabs.Data {

  ///<summary>Configure a no persitence <see cref="IDataStore"/>.</summary>
  public class NoopStoreConfigurator : IConfigurator<IServiceCollection> {

    ///<inherit/>
    public void AddTo(IServiceCollection services, IConfiguration cfg) {
      services.AddScoped(typeof(IDataStore), typeof(NoopDataStore));
    }

    ///<summary>Non storing data store.<see cref="IDataStore"/>.</summary>
    public class NoopDataStore : IDataStore {
      static readonly ILogger<NoopDataStore> log= App.Logger<NoopDataStore>();

      ///<inherit/>
      public bool AutoCommit { get => false; set => throw new NotImplementedException(); }

      // ///<inherit/>
      // public TEntity Attach<TEntity>(TEntity ent) where TEntity : class =>  ent;

      ///<inherit/>
      public void CommitChanges() {}

      ///<inherit/>
      public void Delete<TEntity>(TEntity ent) where TEntity : class => throw new NotImplementedException();

      ///<inherit/>
      public void Delete<E>(IEnumerable<E> entities) where E : class => throw new NotImplementedException();

      // ///<inherit/>
      // public void Dispose() { }

      ///<inherit/>
      public void EnsureStore(IEnumerable<IDataSeed> seeds) => log.LogInformation("Confirmed to NOT provide ANY storage facility.");

      ///<inherit/>
      public void Evict<TEntity>(TEntity ent) where TEntity : class => throw new NotImplementedException();

      ///<inherit/>
      public TEntity Get<TEntity>(params object[] keys) where TEntity : class => throw new NotImplementedException();

      ///<inherit/>
      public object GetIdentifier<TEntity>(TEntity ent) where TEntity : class => throw new NotImplementedException();

      ///<inherit/>
      public E Insert<E>(E ent) where E : class => ent;

      ///<inherit/>
      public IEnumerable<E> Insert<E>(IEnumerable<E> entities) where E : class => throw new NotImplementedException();

      ///<inherit/>
      public E Update<E>(E ent) where E : class => ent;

      ///<inherit/>
      public IEnumerable<E> Update<E>(IEnumerable<E> entities) where E : class => throw new NotImplementedException();

      ///<inherit/>
      public TEntity Merge<TEntity>(TEntity entity) where TEntity : class, new() => throw new NotImplementedException();

      ///<inherit/>
      public IQueryable<TEntity> Query<TEntity>() where TEntity : class => new List<TEntity>().AsQueryable();

      ///<inherit/>
      public void ResetChanges() { }

      ///<inherit/>
      public void ResetAll() { }

      ///<inherit/>
      public void WithTransaction(Action<IDataTransaction> operation) => operation(new NoOpTransaction());

      ///<inherit/>
      public IQueryable<TEntity> UntrackedQuery<TEntity>() where TEntity : class => new List<TEntity>().AsQueryable();

      ///<inherit/>
      public E LoadExplicit<E, P>(E entity, Expression<Func<E, P>> prop) where E : class where P : class => throw new NotImplementedException();

      ///<inherit/>
      public E LoadExplicit<E, P>(E entity, Expression<Func<E, IEnumerable<P>>> prop) where E : class where P : class => throw new NotImplementedException();

      ///<inherit/>
      public IQueryable<E> LoadRelated<E>(IQueryable<E> query, string navigationPropertyPath) where E : class => query;

      ///<inherit/>
      public IEagerLoadedQueryable<E, P> LoadRelated<E, P>(IQueryable<E> query, Expression<Func<E, P>> navProperty) where E : class => new EagerLoadedQueryable<E,P>(query);

      ///<inherit/>
      public IEagerLoadedQueryable<E, Prop> ThenLoadRelated<E, Prev, Prop>(IEagerLoadedQueryable<E, IEnumerable<Prev>> query, Expression<Func<Prev, Prop>> navProperty) where E : class
         => new EagerLoadedQueryable<E,Prop>(query);

      ///<inherit/>
      public IEagerLoadedQueryable<E, Prop> ThenLoadRelated<E, Prev, Prop>(IEagerLoadedQueryable<E, Prev> query, Expression<Func<Prev, Prop>> navProperty) where E : class
         => new EagerLoadedQueryable<E,Prop>(query);

      E IDataStore.Attach<E>(E ent) => ent;
    }
    
    private class EagerLoadedQueryable<E, P> : IEagerLoadedQueryable<E, P> {
      private readonly IQueryable<E> q;
      public EagerLoadedQueryable(IQueryable<E> q) {
        this.q = q;
      }
      public Expression Expression => q.Expression;
      public Type ElementType => q.ElementType;
      public IQueryProvider Provider => q.Provider;
      public IEnumerator<E> GetEnumerator() => q.GetEnumerator();
      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private class NoOpTransaction : IDataTransaction {
      public object Id => this;

      public void Cancel() { }

      public void Commit() { }

      public void Dispose() { }
    }
  }
}