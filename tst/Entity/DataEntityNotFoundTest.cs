using Xunit;
using Tlabs.Data.Entity;
using System;

namespace Tlabs.Data.Entity.Tests {


  public class DataEntityNotFoundTest {

    [Fact]
    public void BasicTest() {
      var key= "unknownUser";
      var ex= new DataEntityNotFoundException<User>(key);
      Assert.Equal(3, ex.Data.Count);
      Assert.NotNull(ex.MsgTemplate());
      Assert.Equal(2, ex.TemplateData().Count);
      Assert.Contains(nameof(User), ex.ResolvedMsgTemplate());
      Assert.Contains(key, ex.ResolvedMsgTemplate());
      Assert.Equal(ex.Message, ex.ResolvedMsgTemplate());
    }

  }

}