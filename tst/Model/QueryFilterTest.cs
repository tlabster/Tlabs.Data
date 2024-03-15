using System;

using Xunit;

namespace Tlabs.Data.Model.Tests {
  public class QueryFilterTest {

    [Fact]
    public void CtorTest() {
      var filter= new QueryFilter {
        Limit= 55
      };

      Assert.Equal(55, filter.Limit);
      Assert.Null(filter.Start);
      Assert.False(filter.NoTotalCount);
      Assert.Null(filter.Properties);
      Assert.Null(filter.SortAscBy);

      var tfilter= new TimeQueryFilter();
      Assert.Equal(DateTime.MinValue, tfilter.Since);
      Assert.Equal(DateTime.MaxValue, tfilter.Until);

      tfilter= new TimeQueryFilter(filter);
      tfilter.Since= default(System.DateTime);
      Assert.Equal(filter.Limit, tfilter.Limit);
      Assert.Null(tfilter.Start);
      Assert.False(tfilter.NoTotalCount);
      Assert.NotEmpty(tfilter.Properties);
      Assert.Null(tfilter.SortAscBy);
      Assert.NotNull(tfilter.Since);
      Assert.Equal(DateTime.MaxValue, tfilter.Until);

    }

  }
}