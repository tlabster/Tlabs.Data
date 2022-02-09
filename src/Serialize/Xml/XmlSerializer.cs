using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Tlabs.Data.Serialize.Xml {

  ///<summary>Abstract Xml base format.</summary>
  public abstract class XmlFormat {
    ///<summary>Logger.</summary>
    protected static readonly ILogger<XmlFormat> log= App.Logger<XmlFormat>();
    ///<summary>Xml reader settings.</summary>
    protected XmlReaderSettings xmlSettings;
    ///<summary>Xml writer settings.</summary>
    protected XmlWriterSettings wrSettings;
    ///<summary>Xml serializer.</summary>
    protected XmlSerializer xml;
    ///<summary>Xml serializer namespaces (for writing).</summary>
    protected XmlSerializerNamespaces ns;
  }

  ///<summary>Xml format serialization.</summary>
  public class XmlFormat<T, S> : XmlFormat where T : class, new() where S : XmlFormat<T, S>.Schema, new() {
    private readonly S schema= new S();

    ///<summary>Default Ctor.</summary>
    ///<remarks>This ctor is to be used if no <see cref="XmlSerializerOptions{T}"/> are registered with any DI service provider.</remarks>
    public XmlFormat() : this(null) { }

    ///<summary>Ctor from <paramref name="options"/>.</summary>
    public XmlFormat(XmlSerializerOptions<T> options) {
      this.xmlSettings= options?.ReaderSettings ?? new XmlReaderSettings();
      this.xmlSettings.CloseInput= true;
      this.wrSettings= new XmlWriterSettings();
      this.wrSettings.CloseOutput= true;
      this.wrSettings.OmitXmlDeclaration= true;
      this.wrSettings.Indent= true;

      this.ns= options?.Namespaces;
      if (null == ns) {
        this.ns= new XmlSerializerNamespaces();
        this.ns.Add("", "");    //omit the xmlns:xsi xmlns:xsd decalrations by default...
      }
      this.xml= new XmlSerializer(typeof(T), this.schema);
    }

    ///<summary>Xml schema for <typeparamref name="T"/>.</summary>
    public class Schema : XmlAttributeOverrides {

      ///<summary>Finisher for <paramref name="obj"/>.</summary>
      public virtual T Finished(T obj) => obj;
    }

    ///<summary>Xml format serializer for <typeparamref name="T"/>.</summary>
    public class Serializer : ISerializer<T> {
      private XmlFormat<T, S> format;

      ///<summary>Ctor from <paramref name="format"/>.</summary>
      public Serializer(XmlFormat<T, S> format) {
        this.format= format;
      }

      ///<inheritdoc/>
      public string Encoding => "XML";

      ///<inheritdoc/>
      public T LoadObj(byte[] utf8Xml) => LoadObj(System.Text.Encoding.UTF8.GetString(utf8Xml));

      ///<summary>Load object from XML <paramref name="strm"/>.</summary>
      public T LoadObj(Stream strm) {
        using (var sr= new StreamReader(strm, System.Text.Encoding.UTF8, true)) {
          return loadObj(sr);
        }
      }

      ///<summary>Load object from XML <paramref name="text"/>.</summary>
      public T LoadObj(string text) {
        using (var sr= new StringReader(text)) {
          return loadObj(sr);
        }
      }

      T loadObj(TextReader txtRd) {
        using (var rd = XmlReader.Create(txtRd, format.xmlSettings)) {
          var obj= (T)format.xml.Deserialize(rd);
          return format.schema.Finished(obj);
        }
      }

      ///<inheritdoc/>
      public byte[] WriteObj(T obj) {
        var strm= new MemoryStream();
        WriteObj(strm, obj);
        return strm.ToArray();
      }

      ///<summary>Write object to XML <paramref name="strm"/>.</summary>
      public void WriteObj(Stream strm, T obj) {
        var xw= XmlWriter.Create(strm, format.wrSettings);
        format.xml.Serialize(xw, obj, format.ns);
      }

      ///<inherit/>
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