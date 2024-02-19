using System.Collections.Generic;
using System.Linq;

using Tlabs.Data.Entity;

using Xunit;
using Moq;
using Xunit.Abstractions;

namespace Tlabs.Data.Repo.Intern.Tests {

  public class CachedRepoTest {
    ITestOutputHelper tstout;
    public CachedRepoTest(ITestOutputHelper tstout) => this.tstout= tstout;

    [Fact]
    public void BasicTest() {
      var storeMock= new Mock<IDataStore>();
      storeMock.Setup(s => s.UntrackedQuery<User>())
               .Returns(() => new List<User> {
                new User {
                  Id= 1,
                  UserName= "usr#1"
                },
                new User {
                  Id= 2,
                  UserName= "usr#2"
                }
               }.AsQueryable());

      var cacheRepo= new CachedRepo<User>(storeMock.Object);
      Assert.NotEmpty(cacheRepo.AllUntracked());
      Assert.NotEmpty(DataStoreFilter<User>.AsQueryable().Filter(cacheRepo.AllUntracked()));


      Assert.NotNull(cacheRepo.AllUntracked().Where(e => e.Id == 1).SingleEntity(1));
      Assert.NotNull(cacheRepo.AllUntracked().SingleEntity(e => e.Id == 1, 1));
      var filter= DataStoreFilter<User>.AsQueryable().Where(e => e.Id == 1);
      tstout.WriteLine(filter.ToString());
      Assert.NotEmpty(filter.Filter(cacheRepo.AllUntracked()));
    }

  }

}