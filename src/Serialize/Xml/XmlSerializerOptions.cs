using System;
using System.Xml;
using System.Xml.Serialization;

namespace Tlabs.Data.Serialize.Xml {

  ///<summary><see cref="XmlFormat{T, S}.Serializer"/> <typeparamref name="T"/> specific serialization options.</summary>
  public class XmlSerializerOptions<T> where T : class {
  ///<summary>Ctor from <paramref name="readerSettings"/>.</summary>
  public XmlSerializerOptions(XmlReaderSettings readerSettings, XmlSerializerNamespaces ns) {
      this.ReaderSettings= readerSettings;
      this.Namespaces= ns;
    }
    ///<summary>ReaderSettings</summary>
    public XmlReaderSettings ReaderSettings { get; }
    ///<summary>Serializer namespaces</summary>
    public XmlSerializerNamespaces Namespaces { get; }
    ///<summary>Options target type</summary>
    public Type TargetType => typeof(T); 
  }
}