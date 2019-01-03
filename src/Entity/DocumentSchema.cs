#pragma warning disable CS1591

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Tlabs.Misc;

namespace Tlabs.Data.Entity {

  public partial class DocumentSchema : Intern.EditableEntity {
    public static readonly IReadOnlyDictionary<string, Type> ATTR_TYPE= new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase) {
      ["TEXT"]= typeof(string),
      ["NUMBER"]= typeof(decimal?),
      ["BOOLEAN"]= typeof(bool),
      ["DATETIME"]= typeof(DateTime?),
      ["DATE"]= typeof(DateTime?),
      ["TIME"]= typeof(TimeSpan?),

      ["TEXT[]"]= typeof(List<string>),
      ["NUMBER[]"]= typeof(List<decimal>),
      ["BOOLEAN[]"]= typeof(List<bool>),
      ["DATETIME[]"]= typeof(List<DateTime>),
      ["DATE[]"]= typeof(List<DateTime>),
      ["TIME[]"]= typeof(List<TimeSpan>)
    };

    private static readonly char[] TSID= new char[] { ':' };
    private static readonly char[] TSUB= new char[] { '-' };
    public static readonly BasicCache<string, DocumentSchema> Cache= new BasicCache<string, DocumentSchema>();
    public static readonly BasicCache<string, string> AltNameCache= new BasicCache<string, string>();
    public static string BuildTypeId(string typeName, string typeVers) {
      return $"{typeName}:{typeVers}";
    }

    private string baseType;
    private string subType;
    private byte[] binary_calcModelData; //not to be used as backing field
    private Stream calcModelStream;

    public static void ParseTypeId(string typeId, out string typeName, out string typeVers) {
      var typeComp= typeId.Split(TSID, 2);
      if (typeComp.Length != 2) throw new ArgumentException($"Invalid Type ID: '{typeId}'");
      typeName= typeComp[0];
      typeVers= typeComp[1];
    }

    public static void ParseTypeName(string typeName, out string baseType, out string subType) {
      if (null == typeName) throw new ArgumentNullException(nameof(typeName));
      var comp= typeName.Split(TSUB);
      if (comp.Length > 2) throw new ArgumentException($"Invalid {nameof(typeName)} format: '{typeName}'");
      subType= comp.Length == 2 ? comp[1] : "";
      baseType= comp[0];
    }


    public string TypeName { get; set; }
    public string TypeAltName { get; set; } // alternate (import) form name
    public string TypeVers { get; set; }
    public string Comment { get; set; }

    public string TypeId {  //implicitly not mapped
      get { return BuildTypeId(TypeName, TypeVers); }
    }

    public string BaseType {
      get {
        parseSubType();
        return baseType;
      }
    }
    public string SubType {
      get {
        parseSubType();
        return subType;
      }
    }

    private void parseSubType() {
      if (null == TypeName) return;
      if (null == baseType || null == subType)
        ParseTypeName(TypeName, out this.baseType, out this.subType);
    }

    public byte[] FormData { get; set; }

    public byte[] FormStyleData { get; set; }

    public byte[] CalcModelData {
      get => binary_calcModelData;
      set {
        binary_calcModelData= value;
        calcModelStream=   HasCalcModel
                         ? new MemoryStream(binary_calcModelData, writable: false)
                         : null;
      } 
    }

    public Stream CalcModelStream => calcModelStream;

    public bool HasCalcModel => null != binary_calcModelData && binary_calcModelData.Length > 0;
    
    public List<Field> Fields { get; set; }
    public List<ValidationRule> Validations { get; set; }

    public class Field : Intern.EditableEntity {
      private string typeName;
      private Type type;
      private string extMappingInfo;
      private IDictionary<string, string> mappingInfo;

      ///<summary>Default ctor</summary>
      public Field() { }

      ///<summary>Copy ctor</summary>
      public Field(Field other) {
        this.Id= other.Id;
        this.Editor= other.Editor;
        this.Modified= other.Modified;

        this.Name= other.Name;
        this.TypeName= other.TypeName;
        this.Schema= other.Schema;
        this.ExtMappingInfo= other.ExtMappingInfo;
        this.Sensitive= other.Sensitive;
      }

      public string Name { get; set; }
      public bool Sensitive { get; set; }
      public string TypeName {
        get { return typeName; }
        set { typeName= value; }
      }
      public DocumentSchema Schema { get; set; }

      //implicitly not mapped
      public Type Type {
        get {
          if (type == null && !ATTR_TYPE.TryGetValue(typeName, out type)) throw new AppConfigException($"Unknown document attribute[{Name ?? "???"}] type: '{typeName}'.");
          return type;
        }
      }

      public string ExtMappingInfo {
        get => this.extMappingInfo;
        set {
          this.extMappingInfo= value;
          if (string.IsNullOrEmpty(this.extMappingInfo)) {
            this.mappingInfo= new Dictionary<string, string>();
            return;
          }
          var mappings= this.extMappingInfo.Split('\n').Select(elem => elem.Split('=')).Where(pair => pair.Count() == 2);
          this.mappingInfo= mappings.ToDictionary(pair => pair[0], pair => pair[1]);
        }
      }

      //implicitly not mapped
      public IDictionary<string, string> MappingInfo {
        get => this.mappingInfo;
      }
    } //class Field

    public class ValidationRule : Intern.EditableEntity {
      public string Key { get; set; }
      public string Description { get; set; }
      public string Code { get; set; }
      public DocumentSchema Schema { get; set; }


      /// <summary>Validation exception.</summary>
      public class ValidationException : AppConfigException {

        /// <summary>Default ctor</summary>
        public ValidationException() : base() { }

        /// <summary>Ctor from message</summary>
        public ValidationException(string message) : base(message) { }

        /// <summary>Ctor from message and inner exception.</summary>
        public ValidationException(string message, Exception e) : base(message, e) { }

      }
    } //class ValidationRule

  }
}
