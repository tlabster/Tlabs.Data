using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Tlabs.Data.Serialize.Xml {

  ///<summary>Xml format serialization.</summary>
  public class XmlFormat<T, S> where T : class, new() where S : XmlFormat<T, S>.Schema, new() {
    private ILogger<XmlFormat<T, S>> log;
    private readonly XmlReaderSettings xmlSettings= new XmlReaderSettings();
    private readonly XmlSerializer xml;
    private readonly S schema= new S();

    ///<summary>Ctor from <paramref name="log"/>.</summary>
    public XmlFormat(ILogger<XmlFormat<T, S>> log) {
      this.log= log;
      this.xmlSettings.CloseInput= true;
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
      public T LoadObj(byte[] utf8Json) => LoadObj(System.Text.Encoding.UTF8.GetString(utf8Json));

      ///<summary>Load object from XML <paramref name="strm"/>.</summary>
      public T LoadObj(Stream strm) {
        using (var sr= new StreamReader(strm, System.Text.Encoding.UTF8, true)) {
          using (var rd= XmlReader.Create(sr, format.xmlSettings)) {
            var obj= (T)format.xml.Deserialize(rd);
            return format.schema.Finished(obj);
          }
        }
      }

      ///<summary>Load object from XML <paramref name="text"/>.</summary>
      public T LoadObj(string text) {
        using (var sr = new StringReader(text)) {
          using (var rd = XmlReader.Create(sr, format.xmlSettings)) {
            var obj= (T)format.xml.Deserialize(rd);
            return format.schema.Finished(obj);
          }
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
        format.xml.Serialize(strm, obj);
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