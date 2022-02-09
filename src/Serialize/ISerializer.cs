using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Tlabs.Data.Serialize {

  /// <summary>
  /// Callback delegate to allow performing modifications on single serialized items
  /// </summary>
  public delegate object ElementCallback(object elem);

  /// <summary>
  /// Callback delegate to allow performing modifications on single serialized items
  /// </summary>
  /// <param name="elem"></param>
  /// <returns></returns>
  public delegate object ElementCallback<T>(T elem);

  /// <summary>Interface of dynamic object serializer / deserializer where the type of the object is only known during runtime.</summary>
  public interface IDynamicSerializer {
    /// <summary>Supported encoding (like 'Json, XML,...) of the serialized data.</summary>
    string Encoding { get; }

    /// <summary>Return serialized <paramref name="obj"/> as utf8 encoded bytes.</summary>
    byte[] WriteObj(object obj);
    /// <summary>Write serialized <paramref name="obj"/> to <paramref name="strm"/>.</summary>
    void WriteObj(Stream strm, object obj);

    /// <summary>Load deserialized object from <paramref name="utf8"/> bytes with expected <paramref name="type"/>.</summary>
    object LoadObj(byte[] utf8, Type type);

    /// <summary>Load deserialized object from <paramref name="strm"/> stream with expected <paramref name="type"/>.</summary>
    object LoadObj(Stream strm, Type type);

    /// <summary>Load deserialized object from <paramref name="text"/> string with expected <paramref name="type"/>.</summary>
    object LoadObj(string text, Type type);

    /// <summary>Write serialized <paramref name="itemsToSerialize"/> to <paramref name="strm"/> from IEnumerable.</summary>
    void WriteIEnumerable(Stream strm, IEnumerable itemsToSerialize, ElementCallback callback);

    /// <summary>Load deserialized items from <paramref name="strm"/> string as IEnumerable.</summary>
    IEnumerable LoadIEnumerable(Stream strm);
  }

  /// <summary>Interface of a serializer / deserializer of objects with type <typeparamref name="T"/>.</summary>
  public interface ISerializer<T> {
    /// <summary>Supported encoding (like 'Json, XML,...) of the serialized data.</summary>
    string Encoding { get; }

    /// <summary>Return serialized <paramref name="obj"/> as utf8 encoded bytes.</summary>
    byte[] WriteObj(T obj);

    /// <summary>Write serialized <paramref name="obj"/> to <paramref name="strm"/>.</summary>
    void WriteObj(Stream strm, T obj);

    /// <summary>Load deserialized instance of T from <paramref name="utf8"/> bytes.</summary>
    T LoadObj(byte[] utf8);

    /// <summary>Load deserialized instance of T from <paramref name="strm"/> stream.</summary>
    T LoadObj(Stream strm);

    /// <summary>Load deserialized instance of T from <paramref name="text"/> string.</summary>
    T LoadObj(string text);
    
    // /// <summary>Write serialized <paramref name="itemsToSerialize"/> to <paramref name="strm"/> from IEnumerable.</summary>
    // void WriteIEnumerable(Stream strm, IEnumerable<T> itemsToSerialize, ElementCallback<T> callback);

    /// <summary>Load deserialized items from <paramref name="strm"/> string as IEnumerable.</summary>
    IEnumerable<T> LoadIEnumerable(Stream strm);
  }

  /// <summary>
  /// Interface of a serializer / deserializer of objects with type <typeparamref name="T"/>
  /// and a serialization Schema of type <typeparamref name="S"/>.
  /// </summary>
  public interface ISerializer<T, S> : ISerializer<T> where T : class, new() { }


}