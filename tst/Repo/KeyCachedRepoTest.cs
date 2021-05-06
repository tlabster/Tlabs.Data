using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Xunit;
using Xunit.Abstractions;
using Moq;
using System.Linq.Expressions;

namespace Tlabs.Data.Repo.Intern.Tests {

  public class KeyCachedRepoTest : IClassFixture<KeyCachedRepoTest.TestFixture> {

    public class TestFixture {
      public int QueryCnt;
      public IDataStore Store;

      public TestFixture() {
        var storeMoq= new Mock<IDataStore>();
        storeMoq.Setup(store => store.UntrackedQuery<TestEntity>())
                .Callback(() => ++QueryCnt )
                .Returns(new List<TestEntity> {
                  new TestEntity {
                    Key= "key_01",
                    Title= "Title of key_01"
                  },
                  new TestEntity {
                    Key= "key_02",
                    Title= "Title of key_02"
                  },
                  new TestEntity {
                    Key= "key_03",
                    Title= "Title of key_03"
                  }
                }.AsQueryable());
        this.Store= storeMoq.Object;

      }
    }

    public class TestEntity : Entity.Intern.BaseEntity {
      public string Key { get; set; }
      public string Title { get; set; }
    }

    public class TestKeyCacheRepo : AbstractKeyCachedRepo<TestEntity, string> {
      public int SupplCnt;
      public TestKeyCacheRepo(IDataStore store) : base(store) { }
      protected override Expression<Func<TestEntity, string>> getKeyExpression => ent => ent.Key;
      protected override IQueryable<TestEntity> supplementalQuery(IQueryable<TestEntity> query) {
        ++SupplCnt;
        return base.supplementalQuery(query);
      }
    }

    TestFixture fix;
    public KeyCachedRepoTest(TestFixture fix) {
      this.fix= fix;
    }

    [Fact]
    public void GuidExperiment() {
      var binguid= Guid.NewGuid().ToByteArray();
      var base64Guid= Convert.ToBase64String(binguid);
      var msgid= base64Guid.Substring(0, 22);
      var encodedMsgId= System.Net.WebUtility.UrlEncode(msgid);
      encodedMsgId= System.Net.WebUtility.UrlEncode(encodedMsgId + " =/?");
    }

    [Fact]
    public void BasicTest() {
      var repo= new TestKeyCacheRepo(fix.Store);

      fix.QueryCnt= 0;
      Assert.Null(repo.GetByKey(null));
      Assert.ThrowsAny<DataEntityNotFoundException>(() => repo.GetByKey(null, mustExist: true));
      Assert.Equal(0, fix.QueryCnt);
      Assert.Equal(fix.QueryCnt, repo.SupplCnt);

      var key= "key_01";
      var ent= repo.GetByKey(key, mustExist: true);
      Assert.NotNull(ent);
      Assert.Equal(key, ent.Key);
      Assert.True(object.ReferenceEquals(ent, repo.GetByKey(key, mustExist: true)));
      Assert.NotEmpty(repo.AllUntracked);

      key= "key_03";
      Assert.NotNull(ent= repo.GetByKey(key, mustExist: false));
      Assert.Equal(key, ent.Key);

      Assert.Null(repo.GetByKey("___non_existing__", mustExist: false));
      Assert.ThrowsAny<DataEntityNotFoundException>(() => repo.GetByKey("___non_existing__", mustExist: true));

      Assert.Equal(1, fix.QueryCnt);
      Assert.Equal(fix.QueryCnt, repo.SupplCnt);
    }

    [Fact]
    public void GenericMethodExperiment() {
      var n= nameof(TstClass.GenericMethod);
      Assert.Throws<System.Reflection.AmbiguousMatchException>(()=> typeof(TstClass).GetMethod(n));
      var meth= typeof(TstClass).GetMethod(n, 3, TstClass.GenericMethParams, null);
      Assert.NotNull(meth);
      meth= meth.MakeGenericMethod(typeof(TestKeyCacheRepo), typeof(string), typeof(Type));
      meth.Invoke(new TstClass(), new object[] { 1, "tst", GetType() });

      var c= new TstClass();
      c.GenericMethod(new GenericClass<string, Type> { P1= typeof(GenericClass<string, Type>).Name, P2= typeof(GenericClass<string, Type>) });
      c.GenericMethod(new DfltGenericClass { P1= typeof(DfltGenericClass).Name, P2= typeof(DfltGenericClass) });

      Assert.NotNull(Activator.CreateInstance(typeof(GenericClass<string, Type>), true));

      // Assert.NotNull(Activator.CreateInstance(typeof(GenericClass<string, Type>),
      //                                           BindingFlags.Instance
      //                                         | BindingFlags.NonPublic
      //                                         | BindingFlags.CreateInstance,
      //                                         null,
      //                                         new object[] {"tst", GetType()}));
    }
    class TstClass {
      public void GenericMethod() { }
      public void GenericMethod<T1, T2, T3>(int i, T2 t2, T3 t3) {
        Assert.Equal(1, i);
        Assert.IsType<string>(t2);
        Assert.Equal("tst", t2 as string);
        Assert.IsAssignableFrom<Type>(t3);
      }
      public void GenericMethod<T1, T2>(GenericClass<T1, T2> genVal) {
        Assert.IsType<string>(genVal.P1);
        Assert.Equal(genVal.GetType().Name, genVal.P1.ToString());
        Assert.IsAssignableFrom<Type>(genVal.P2);
      }

      public static readonly Type[] GenericMethParams= new Type[] {typeof(int), Type.MakeGenericMethodParameter(1) , Type.MakeGenericMethodParameter(2)};
    }

    class GenericClass<T1, T2> {
      public GenericClass() { }
      private GenericClass(T1 t1, T2 t2) {
        this.P1= t1;
        this.P2= t2;
      }
      public T1 P1 { get; set; }
      public T2 P2 { get; set; }
    }

    class DfltGenericClass : GenericClass<string, Type> { }
  }

}