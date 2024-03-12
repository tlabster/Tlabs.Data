#pragma warning disable CS1591

namespace Tlabs.Data.Entity.Intern {

  public class EntityBody : Intern.BaseEntity {
    public enum Enc {
      Undefined= 0,
      Json,
      XML,
      BSON,
      ProtoBuf,
      JSV
    }

    private Enc enc;

    public byte[]? BodyData { get; set; }

    public string Encoding {
      get { return enc.ToString(); }
      set { Enc.TryParse<Enc>(value, true, out enc); }
    }
  }

}