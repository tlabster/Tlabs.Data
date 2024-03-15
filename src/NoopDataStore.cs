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

    ///<inheritdoc/>
    public void AddTo(IServiceCollection services, IConfiguration cfg) {
      services.AddScoped(typeof(IDataStore), typeof(NoopDataStore));
    }

    ///<summary>Non storing data store.<see cref="IDataStore"/>.</summary>
    public class NoopDataStore : IDataStore {
      static readonly ILogger<NoopDataStore> log= App.Logger<NoopDataStore>();

      ///<inheritdoc/>
      public bool AutoCommit { get => false; set => throw new NotImplementedException(); }

      // ///<inheritdoc/>
      // public TEntity Attach<TEntity>(TEntity ent) where TEntity : class =>  ent;

      ///<inheritdoc/>
      public void CommitChanges() {}

      ///<inheritdoc/>
      public void Delete<TEntity>(TEntity ent) where TEntity : class => throw new NotImplementedException();

      ///<inheritdoc/>
      public void Delete<E>(IEnumerable<E> entities) where E : class => throw new NotImplementedException();

      // ///<inheritdoc/>
      // public void Dispose() { }

      ///<inheritdoc/>
      public void EnsureStore(IEnumerable<IDataSeed>? seeds) => log.LogInformation("Confirmed to NOT provide ANY storage facility.");

      ///<inheritdoc/>
      public void Evict<TEntity>(TEntity ent) where TEntity : class => throw new NotImplementedException();

      ///<inheritdoc/>
      public TEntity Get<TEntity>(params object[] keys) where TEntity : class => throw new NotImplementedException();

      ///<inheritdoc/>
      public object GetIdentifier<TEntity>(TEntity ent) where TEntity : class => throw new NotImplementedException();

      ///<inheritdoc/>
      public E Insert<E>(E ent) where E : class => ent;

      ///<inheritdoc/>
      public IEnumerable<E> Insert<E>(IEnumerable<E> entities) where E : class => throw new NotImplementedException();

      ///<inheritdoc/>
      public E Update<E>(E ent) where E : class => ent;

      ///<inheritdoc/>
      public IEnumerable<E> Update<E>(IEnumerable<E> entities) where E : class => throw new NotImplementedException();

      ///<inheritdoc/>
      public TEntity Merge<TEntity>(TEntity entity) where TEntity : class, new() => throw new NotImplementedException();

      ///<inheritdoc/>
      public IQueryable<TEntity> Query<TEntity>() where TEntity : class => new List<TEntity>().AsQueryable();

      ///<inheritdoc/>
      public void ResetChanges() { }

      ///<inheritdoc/>
      public void ResetAll() { }

      ///<inheritdoc/>
      public void WithTransaction(Action<IDataTransaction> operation) => operation(new NoOpTransaction());

      ///<inheritdoc/>
      public IQueryable<TEntity> UntrackedQuery<TEntity>() where TEntity : class => new List<TEntity>().AsQueryable();

      ///<inheritdoc/>
      public E LoadExplicit<E, P>(E entity, Expression<Func<E, P?>> prop) where E : class where P : class => throw new NotImplementedException();

      ///<inheritdoc/>
      public E LoadExplicit<E, P>(E entity, Expression<Func<E, IEnumerable<P>>> prop) where E : class where P : class => throw new NotImplementedException();

      ///<inheritdoc/>
      public IQueryable<E> LoadRelated<E>(IQueryable<E> query, string navigationPropertyPath) where E : class => query;

      ///<inheritdoc/>
      public IEagerLoadedQueryable<E, P> LoadRelated<E, P>(IQueryable<E> query, Expression<Func<E, P>> navProperty) where E : class => new NoopEagerLoadedQueryable<E,P>(query);

      ///<inheritdoc/>
      public IEagerLoadedQueryable<E, Prop> ThenLoadRelated<E, Prev, Prop>(IEagerLoadedQueryable<E, IEnumerable<Prev>> query, Expression<Func<Prev, Prop>> navProperty) where E : class
         => new NoopEagerLoadedQueryable<E,Prop>(query);

      ///<inheritdoc/>
      public IEagerLoadedQueryable<E, Prop> ThenLoadRelated<E, Prev, Prop>(IEagerLoadedQueryable<E, Prev> query, Expression<Func<Prev, Prop>> navProperty) where E : class
         => new NoopEagerLoadedQueryable<E,Prop>(query);

      E IDataStore.Attach<E>(E ent) => ent;

    }

    private class NoOpTransaction : IDataTransaction {
      public object Id => this;

      public void Cancel() { }

      public void Commit() { }

      public void Dispose() { }
    }
  }

  ///<summary>Dummy no op. <see cref="IEagerLoadedQueryable{E, P}"/> implementation.</summary>
  public class NoopEagerLoadedQueryable<E, P> : IEagerLoadedQueryable<E, P> {
    private readonly IQueryable<E> q;
    ///<summary>Ctor from <paramref name="queryable"/>.</summary>
    public NoopEagerLoadedQueryable(IQueryable<E> queryable) => this.q = queryable;
    ///<inheritdoc/>
    public Expression Expression => q.Expression;
    ///<inheritdoc/>
    public Type ElementType => q.ElementType;
    ///<inheritdoc/>
    public IQueryProvider Provider => q.Provider;
    ///<inheritdoc/>
    public IEnumerator<E> GetEnumerator() => q.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }

}