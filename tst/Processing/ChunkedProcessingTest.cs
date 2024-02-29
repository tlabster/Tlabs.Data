using System.Linq;
using System.Threading;

using Tlabs.Config;

using Xunit;
using System.Collections.Generic;

namespace Tlabs.Data.Processing.Tests {

  public class ChunkedProcessingTest : IClassFixture<ChunkedProcessingTest.SvcProvFactory> {
    public class SvcProvFactory : AbstractServiceProviderFactory { }

    public ChunkedProcessingTest(SvcProvFactory f) { Assert.NotNull(f.SvcProv); }

    static int dataSum= 0;
    static int procIdx= 0;
    static int[,] dataRes= new int[3, 27];

    [Fact]
    public void BasicTest() {
      var data= new int[dataRes.GetLength(1)];
      for (int l= 0; l< data.Length; ++l) data[l]= l;

      var qdata= data.AsQueryable();
      qdata.ChunkedProcessing<TestChnkProcessor, int>(10);

      Thread.Sleep(500);
      Assert.Equal(data.Sum(), dataSum);
      int[] data2= new int[data.Length];
      for (int j= 0, m= dataRes.GetLength(0); j < m; ++j)
        for (int l= 0, n= dataRes.GetLength(1); l < n; ++l) {
          var i= dataRes[j, l];
          if (0 != i)
            data2[l]= i;
        }
      Assert.True(data.SequenceEqual(data2));
    }

    private class TestChnkProcessor : IChunkProcessor<int> {
      public bool Process(IEnumerable<int> chunk) {
        var idx= Interlocked.Increment(ref procIdx)-1;
        foreach (int i in chunk) {
          Interlocked.Add(ref dataSum, i);
          dataRes[idx, i]= i;
        }
        return true;
      }
    }
  }

}