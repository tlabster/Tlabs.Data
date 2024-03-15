using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Buffers;

namespace Tlabs.Data.Serialize.Xml {

  ///<summary>Abstract Xml base format.</summary>
  public abstract class XmlFormat {
    ///<summary>Logger.</summary>
    protected static readonly ILogger<XmlFormat> log= App.Logger<XmlFormat>();
    ///<summary>Xml reader settings.</summary>
    protected XmlReaderSettings rdSettings= new();
    ///<summary>Xml writer settings.</summary>
    protected XmlWriterSettings wrSettings= DFLTwrSettings;
    ///<summary>Xml serializer namespaces (for writing).</summary>
    protected XmlSerializerNamespaces ns= DFLTnamespace;

    ///<summary>Default <see cref="XmlWriterSettings"/>.</summary>
    public readonly static XmlWriterSettings DFLTwrSettings= new XmlWriterSettings {
      OmitXmlDeclaration= true,
      CloseOutput= false,
      Indent= true
    };

    ///<summary>Default <see cref="XmlSerializerNamespaces"/>.</summary>
    public readonly static XmlSerializerNamespaces DFLTnamespace;

    static XmlFormat() {
      DFLTnamespace= new ();
      DFLTnamespace.Add("", "");    //omit the xmlns:xsi xmlns:xsd decalrations by default...
    }
  }

  ///<summary>Xml format serialization.</summary>
  public class XmlFormat<T, S> : XmlFormat where T : class, new() where S : XmlFormat<T, S>.Schema, new() {
    private XmlSerializer xml;
    private readonly S schema= new();

    ///<summary>Default Ctor.</summary>
    ///<remarks>This ctor is to be used if no <see cref="XmlSerializerOptions{T}"/> are registered with any DI service provider.</remarks>
    public XmlFormat() {
      this.xml= new XmlSerializer(typeof(T), this.schema);
      this.rdSettings.CloseInput= true;
    }

    ///<summary>Ctor from <paramref name="options"/>.</summary>
    public XmlFormat(XmlSerializerOptions<T> options) : this() {
      this.rdSettings= options?.ReaderSettings ?? this.rdSettings;
      this.wrSettings= options?.WriterSettings ?? this.wrSettings;
      this.ns= options?.Namespaces ?? this.ns;
    }

    ///<summary>Xml schema for <typeparamref name="T"/>.</summary>
    public class Schema : XmlAttributeOverrides {

      ///<summary>Finisher for <paramref name="obj"/>.</summary>
      public virtual T? Finished(T? obj) => obj;
    }

    ///<summary>Xml format serializer for <typeparamref name="T"/>.</summary>
    public class Serializer : ISerializer<T> {
      readonly XmlFormat<T, S> format;

      ///<summary>Ctor from <paramref name="format"/>.</summary>
      public Serializer(XmlFormat<T, S> format) {
        this.format= format;
      }

      ///<inheritdoc/>
      public string Encoding => "XML";


      ///<inheritdoc/>
      public T? LoadObj(ReadOnlySequence<byte> utf8) => LoadObj(utf8.ToArray());

      ///<inheritdoc/>
      public T? LoadObj(ReadOnlySpan<byte> utf8xml) => LoadObj(System.Text.Encoding.UTF8.GetString(utf8xml));

      ///<inheritdoc/>
      public T? LoadObj(byte[] utf8Xml) => LoadObj(System.Text.Encoding.UTF8.GetString(utf8Xml));

      ///<summary>Load object from XML <paramref name="strm"/>.</summary>
      public T? LoadObj(Stream strm) {
        using var sr= new StreamReader(strm, System.Text.Encoding.UTF8, true);
        return loadObj(sr);
      }

      ///<summary>Load object from XML <paramref name="text"/>.</summary>
      public T? LoadObj(string text) {
        using var sr= new StringReader(text);
        return loadObj(sr);
      }

      T? loadObj(TextReader txtRd) {
        using var rd= XmlReader.Create(txtRd, format.rdSettings);
        var obj = (T?)format.xml.Deserialize(rd);
        return format.schema.Finished(obj);
      }

      ///<inheritdoc/>
      public byte[] WriteObj(T obj) {
        var strm= new MemoryStream();
        WriteObj(strm, obj);
        return strm.ToArray();
      }

      ///<summary>Write object to XML <paramref name="strm"/>.</summary>
      public void WriteObj(Stream strm, T obj) {
        using var xw= XmlWriter.Create(strm, format.wrSettings);
        format.xml.Serialize(xw, obj, format.ns);
      }

      ///<inheritdoc/>
      public IEnumerable<T> LoadIEnumerable(Stream stream) {
        throw new NotImplementedException();
      }

      // /// <inherit/>
      // public void WriteIEnumerable(Stream strm, IEnumerable<T> itemsToSerialize, ElementCallback<T> callback) {
      //   throw new NotImplementedException();
      // }

    } //class Serializer<T, S>
  } //class XmlFormat<T, S>

}