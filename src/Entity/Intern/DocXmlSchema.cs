using System.Xml.Serialization;
using System.Collections.Generic;

using Tlabs.Data.Serialize.Xml;

namespace Tlabs.Data.Entity.Intern {

  ///<summary>Xml schema for a <see cref="DocumentSchema"/></summary>
  ///<remarks>
  /// This override of <sse cref="XmlAttributeOverrides"/> specifies the xml schema required by the XmlSerializer (by not using any class [attribites]...)
  ///</remarks>
  public class DocXmlSchema : XmlFormat<DocumentSchema, DocXmlSchema>.Schema {
    ///<summary>Default ctor</summary>
    public DocXmlSchema() : base() {
      //XML root element: <form>
      var xmlAttr= new XmlAttributes();
      xmlAttr.XmlRoot= new XmlRootAttribute("form");
      this.Add(typeof(DocumentSchema), xmlAttr);

      //form element 'name' attribute <form name=...>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("name");
      this.Add(typeof(DocumentSchema), nameof(DocumentSchema.TypeName), xmlAttr);

      //form element 'version' attribute <form version=...>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("version");
      this.Add(typeof(DocumentSchema), nameof(DocumentSchema.TypeVers), xmlAttr);

      //form element 'alternatename' attribute <form alternatename=...>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("alternatename");
      this.Add(typeof(DocumentSchema), nameof(DocumentSchema.TypeAltName), xmlAttr);

      //form element 'generator' attribute <form generator=...>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("generator");
      this.Add(typeof(DocumentSchema), nameof(DocumentSchema.Comment), xmlAttr);

      xmlAttr= new XmlAttributes();
      xmlAttr.XmlElements.Add(new XmlElementAttribute("evalContextType"));
      this.Add(typeof(DocumentSchema), nameof(DocumentSchema.EvalContextType), xmlAttr);

      xmlAttr= new XmlAttributes();
      xmlAttr.XmlElements.Add(new XmlElementAttribute("evalCtxSelfProp"));
      this.Add(typeof(DocumentSchema), nameof(DocumentSchema.EvalCtxSelfProp), xmlAttr);

      //form child element <form><field>...
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlElements.Add(new XmlElementAttribute("field", typeof(AnyChildXmlField)));
      this.Add(typeof(DocumentSchema), nameof(DocumentSchema.Fields), xmlAttr);

      //field element 'name' attribute <field name=...>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("name");
      this.Add(typeof(DocumentSchema.Field), nameof(DocumentSchema.Field.Name), xmlAttr);

      //field element 'type' attribute <field type=...>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("type");
      this.Add(typeof(DocumentSchema.Field), nameof(DocumentSchema.Field.TypeName), xmlAttr);

      //field element 'sensible' attribute <field name=...>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("sensitive");
      this.Add(typeof(DocumentSchema.Field), nameof(DocumentSchema.Field.Sensitive), xmlAttr);


      //calculation attrib <calculation desc=...>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("calc-formula");
      this.Add(typeof(DocumentSchema.Field), nameof(DocumentSchema.Field.CalcFormula), xmlAttr);

      //field child element <form><field><any>...
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAnyElements.Add(new XmlAnyElementAttribute());
      this.Add(typeof(AnyChildXmlField), "AnyChildElements", xmlAttr);

      //Ignore field properties 'Types', 'Schema'...
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlIgnore= true;
      this.Add(typeof(DocumentSchema.Field), nameof(DocumentSchema.Field.Type), xmlAttr);
      this.Add(typeof(DocumentSchema.Field), nameof(DocumentSchema.Field.Schema), xmlAttr);

      //form child elements <form><validations><rule>...>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlArray= new XmlArrayAttribute("validations");
      xmlAttr.XmlArrayItems.Add(new XmlArrayItemAttribute("rule"));
      this.Add(typeof(DocumentSchema), nameof(DocumentSchema.Validations), xmlAttr);

      //rule element 'id' attribute <rule id=...>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("id");
      this.Add(typeof(DocumentSchema.ValidationRule), nameof(DocumentSchema.ValidationRule.Key), xmlAttr);

      //rule element 'desc' attribute <rule desc=...>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("desc");
      this.Add(typeof(DocumentSchema.ValidationRule), nameof(DocumentSchema.ValidationRule.Description), xmlAttr);

      //rule element inner 'code' <rule>code</rule>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlText= new XmlTextAttribute();
      this.Add(typeof(DocumentSchema.ValidationRule), nameof(DocumentSchema.ValidationRule.Code), xmlAttr);

      //rule ignore Schema property
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlIgnore= true;
      this.Add(typeof(DocumentSchema.ValidationRule), nameof(DocumentSchema.ValidationRule.Schema), xmlAttr);

      //form child elements <form><evalReferences><ref>...>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlArray= new XmlArrayAttribute("evalReferences");
      xmlAttr.XmlArrayItems.Add(new XmlArrayItemAttribute("ref"));
      this.Add(typeof(DocumentSchema), nameof(DocumentSchema.EvalReferences), xmlAttr);

      //ref element 'prop' attribute ref <prop=...>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("prop");
      this.Add(typeof(DocumentSchema.EvaluationRef), nameof(DocumentSchema.EvaluationRef.PropName), xmlAttr);

      //ref element text content <ref>Sid</ref>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlText= new XmlTextAttribute();
      this.Add(typeof(DocumentSchema.EvaluationRef), nameof(DocumentSchema.EvaluationRef.ReferenceSid), xmlAttr);

      //ref ignore Schema property
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlIgnore= true;
      this.Add(typeof(DocumentSchema.EvaluationRef), nameof(DocumentSchema.EvaluationRef.Schema), xmlAttr);

    }


    ///<summary>Custom <paramref name="schema"/> finishing by adding references in children to this schema.</summary>
    public override DocumentSchema Finished(DocumentSchema schema) {
      foreach (var fld in schema.Fields)
        fld.Schema= schema;
      foreach (var vld in schema.Validations)
        vld.Schema= schema;

      return schema;
    }
  } //class DocXmlSchema

  ///<summary>Derived <see cref="DocumentSchema.Field"/> to convert any xml child-elements into <see cref="DocumentSchema.Field.ExtMappingInfo"/>.</summary>
  public class AnyChildXmlField : DocumentSchema.Field {

    ///<summary>Property to receive any child elements.</summary>
    public System.Xml.XmlElement[] AnyChildElements {
      get { return null; }
      set {
        if (null == value) return;
        var mappingInfo= new List<string>();
        foreach (var el in value) {
          foreach (var a in el.Attributes) {
            var attr= (System.Xml.XmlAttribute)a; //down cast into XmlAttribute
            mappingInfo.Add($"{el.Name}-{attr.Name}={attr.Value}"); //like: edifact-path=value
          }
        }
        this.ExtMappingInfo= string.Join("\n", mappingInfo);
      }
    }
  } //class EdifactXmlField

}