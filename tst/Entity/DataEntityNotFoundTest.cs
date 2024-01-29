using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

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

    [Fact]
    public void LinqTest() {

      try {
        new int[] {1, 2}.SingleOrDefault();
      }
      catch (InvalidOperationException e) {
        Assert.EndsWith("more than one element", e.Message);
      }

      try {
        Enumerable.Empty<int>().Single();
      }
      catch (InvalidOperationException e) {
        Assert.EndsWith("contains no elements", e.Message);
      }

      var dict= new Dictionary<int, bool>() {[1]= true };
      try {
        dict.Add(1, false);
      }
      catch (ArgumentException e) {
        Assert.Contains("key", e.Message);
      }

      try {
        var eleven= dict[11];
      }
      catch (KeyNotFoundException e) {
        Assert.Contains("not present", e.Message);
      }
    }

  }
}