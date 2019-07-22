using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Tlabs.Sync;

namespace Tlabs.Data.Processing {

  ///<summary>Interface of a processor of a chunk of items.</summary>
  public interface IChunkProcessor<T> {

    ///<summary>Process a <paramref name="chunk"/> of items.</summary>
    ///<returns>False to abort further chunk processing.</returns>
    bool Process(IEnumerable<T> chunk);
 
  }

  ///<summary><see cref="IQueryable{T}"/> extension for chunked processing>.</summary>
  public static class ChunkedPrecessingExtension {
    ///<summary>Chunked processing of <paramref name="query"/> result with a processor of <typeparamref name="TProc"/>>.</summary>
    ///<remarks>The processor receives a chunk of <typeparamref name="TEnt"/> with <paramref name="chunkSz"/> where up to <paramref name="parallelCnt"/> processor being executed in parallel.</remarks>
    public static void ChunkedProessing<TProc, TEnt>(this IQueryable<TEnt> query, int chunkSz, int parallelCnt= 2) where TProc : IChunkProcessor<TEnt> {
      new ChunkContext<TProc, TEnt>(query, chunkSz, parallelCnt);
    }

    private class ChunkRes {
      public bool Abort;
    }

    private class ChunkContext<TProc, TEnt> where TProc : IChunkProcessor<TEnt> {
      public int chunkSz;
      public int parallelCnt;
      public bool abort;
      public int procCnt;
      public SyncMonitor<int> syncCnt= new SyncMonitor<int>();

      public ChunkContext(IQueryable<TEnt> query, int chunkSz, int parallelCnt) {
        this.chunkSz= chunkSz;
        this.parallelCnt= parallelCnt;
        this.abort= false;
        this.syncCnt= new SyncMonitor<int>();
        syncCnt.Signal(procCnt= parallelCnt);
        var chunk= new List<TEnt>(chunkSz);
        foreach (var ent in query) {
          if (chunk.Count < chunkSz) {
            chunk.Add(ent);
            continue;
          }
          nextChunk(chunk);
          chunk= new List<TEnt>(chunkSz);
          chunk.Add(ent);
        }
        if (chunk.Count > 0) nextChunk(chunk);
        while (syncCnt.Value < this.parallelCnt && syncCnt.WaitForSignal() < this.parallelCnt);
      }

      private void nextChunk(List<TEnt> chunk) {
        if (abort) return;
        if (syncCnt.WaitForSignal() > 0)
          syncCnt.Signal(Interlocked.Decrement(ref procCnt));
        App.RunBackgroundService<TProc, ChunkRes>(chunkProc => new ChunkRes { Abort= !chunkProc.Process(chunk) })
           .ContinueWith(abortTsk => {
             if (false == (abort= abortTsk.GetAwaiter().GetResult().Abort)) {
               syncCnt.Signal(Interlocked.Increment(ref procCnt));
             }
        });
      }
    }
  }
}