#pragma warning disable CS1591
using System;
using System.Collections;
using System.Collections.Generic;
using System.Buffers;
using System.IO;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using Tlabs.Misc;

#nullable enable

namespace Tlabs.Data.Serialize.Json {

  internal sealed class JsonStreamEnumerator { };
  public sealed class JsonStreamEnumerator<T> : IEnumerator<T?>, IEnumerable<T?> {
    static readonly ILogger log= App.Logger<JsonStreamEnumerator>();

    Deserializer.StreamState enumState;
    readonly Stream stream;
    public JsonStreamEnumerator(Stream strm, JsonSerializerOptions? jsonOpt= default, int bufSz= 4096) {
      this.stream= strm;
      init(strm, jsonOpt, bufSz);
    }

    public T? Current => enumState.currentElement;

    object? IEnumerator.Current => Current;

    public bool MoveNext() {
      var jsonDeserializer= new Deserializer(enumState);
      var hasNext= jsonDeserializer.MoveNext();
      this.enumState= jsonDeserializer.InternalState;
      return hasNext;
    }

    public void Reset() {
      stream.Seek(0, SeekOrigin.Begin);   //can throw if stream.CanSeek == false
      var opt= enumState.jsonOptions;
      var sz= enumState.buffer.MinChunkSz;
      enumState.Dispose();
      init(stream, opt, sz);
    }

    public void Dispose() => enumState.Dispose();

    void init(Stream strm, JsonSerializerOptions? jsonOpt, int bufSz) {
      var jsonDeserializer= new Deserializer(strm, jsonOpt, bufSz);
      this.enumState= jsonDeserializer.InternalState;
    }

    public IEnumerator<T?> GetEnumerator() => this;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public ref struct Deserializer {
#pragma warning disable CA1001   //StreamState is IDisposable !
      public struct StreamState : IDisposable {
        internal ReadStreamBuffer buffer;
        internal long bufOffset;
        internal JsonReaderState jsonState;
        internal JsonSerializerOptions? jsonOptions;
        internal T? currentElement;

        public StreamState(Stream stream, JsonSerializerOptions? jsonOptions, int bufferSize) {
          this.buffer= new ReadStreamBuffer(stream, bufferSize);
          this.jsonState= default(Utf8JsonReader).CurrentState;
          this.bufOffset= 0;
          this.jsonOptions= jsonOptions;
          this.currentElement= default!;
        }
        internal Utf8JsonReader newJsonReader(ref Utf8JsonReader prevReader, in ReadOnlySequence<byte> seq) {
          bufOffset+= prevReader.BytesConsumed;
          return new Utf8JsonReader(seq.Slice(bufOffset), buffer.IsEndOfStream, prevReader.CurrentState);
        }

        public readonly void Dispose() {
          buffer?.Dispose();
        }
      }
#pragma warning restore CA1001   //StreamState is IDisposable !

      StreamState myState;
      Utf8JsonReader jsonReader;

      public Deserializer(Stream stream, JsonSerializerOptions? jsonOptions= null, int bufferSize= 4096) {
        this.myState= new StreamState(stream, jsonOptions, bufferSize);
        this.jsonReader= default;

        if (   !this.readToken()
            || JsonTokenType.StartArray != this.currToken) throw new JsonException("No json array to enumerate");
        myState.bufOffset+= jsonReader.BytesConsumed - myState.buffer.Shrink();
        myState.jsonState= jsonReader.CurrentState;
      }

      public Deserializer(StreamState state) {
        this.myState= state;
        this.jsonReader= default;

        var seq= myState.buffer.Sequence.Slice(myState.bufOffset);
        jsonReader= new Utf8JsonReader(seq, myState.buffer.IsEndOfStream, myState.jsonState);
        if (log.IsEnabled(LogLevel.Trace)) log.LogTrace("Deserializer start: '{json}'", System.Text.Encoding.UTF8.GetString(seq.Slice(jsonReader.Position).ToArray()));
      }

      public readonly StreamState InternalState => myState;

      public readonly T? Current => myState.currentElement;

      public bool MoveNext() {
        if (!this.readToken() || !hasNext()) return false;

        var elemStartPos= seekEndOfObject();

        var elSeq= myState.buffer.Sequence.Slice(elemStartPos, jsonReader.Position);
        if (log.IsEnabled(LogLevel.Trace)) log.LogTrace("Json element: '{json}'", System.Text.Encoding.UTF8.GetString(elSeq.Slice(jsonReader.Position).ToArray()));
        var elementJsonReader= new Utf8JsonReader(elSeq, true, default);

        // deserialize element
        myState.currentElement= JsonSerializer.Deserialize<T>(ref elementJsonReader, myState.jsonOptions);

        myState.bufOffset+= jsonReader.BytesConsumed - myState.buffer.Shrink();
        myState.jsonState= jsonReader.CurrentState;
        return true;
      }

      internal bool hasNext() {
        if (atEndOfObject()) readToken();
        return atStartOfObject();
      }

      internal JsonTokenType currToken => jsonReader.TokenType;

      internal bool readToken() {
        while (!jsonReader.Read()) { // read could be unsuccessful due to insufficient bufer size, retrying in loop with additional buffer segments
          if (myState.buffer.IsEndOfStream) return false;

          jsonReader= myState.newJsonReader(ref this.jsonReader, myState.buffer.Expand());
        }
        return true;
      }

      bool atEndOfObject() {
        return    JsonTokenType.EndObject == currToken
               || JsonTokenType.EndArray == currToken;
      }

      bool atStartOfObject() {
        return    JsonTokenType.StartObject == currToken
               || JsonTokenType.StartArray == currToken;
      }

      private SequencePosition seekEndOfObject() {
        /* This performs basically Utf8JsonReader.Skip(), but is using our readToken() to
         * expand the buuffer sequence on demand...
         */
        var tokPos= myState.buffer.Sequence.GetPosition(myState.bufOffset + jsonReader.TokenStartIndex);
        int depth= 0;

        if (atStartOfObject()) ++depth;

        while (depth > 0 && readToken()) {  // seek through data until end of object is found
          if (atStartOfObject()) ++depth;
          else if (atEndOfObject()) --depth;
        }

        return tokPos;
      }

    }

  }

}