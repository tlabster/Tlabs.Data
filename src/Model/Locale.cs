#pragma warning disable CS1591

namespace Tlabs.Data.Model {
  public class Locale {
    public Locale() { }

    public Locale(Tlabs.Data.Entity.Locale loc) {
      this.Lang= loc.Lang;
      this.DecimalSep= loc.DecimalSep;
      this.ThousandSep= loc.ThousandSep;
      this.ListSep= loc.ListSep;
      this.DateFormat= loc.DateFormat;
      this.TimeFormat= loc.TimeFormat;
      this.DateTimeFormat= loc.DateTimeFormat;
      this.IntegerFormat= loc.IntegerFormat;
      this.FixedFormat= loc.FixedFormat;
      this.MonetaryFormat= loc.MonetaryFormat;
    }

    public Tlabs.Data.Entity.Locale AsEntity() {
      return new Tlabs.Data.Entity.Locale {
        Lang= this.Lang,
        DecimalSep= this.DecimalSep,
        ThousandSep= this.ThousandSep,
        ListSep= this.ListSep,
        DateFormat= this.DateFormat,
        TimeFormat= this.TimeFormat,
        DateTimeFormat= this.DateTimeFormat,
        IntegerFormat= this.IntegerFormat,
        FixedFormat= this.FixedFormat,
        MonetaryFormat= this.MonetaryFormat
      };
    }

    public Tlabs.Data.Entity.Locale CopyTo(Tlabs.Data.Entity.Locale ent) {
      ent.Lang= this.Lang ?? ent.Lang;
      ent.DecimalSep= this.DecimalSep ?? ent.DecimalSep;
      ent.ThousandSep= this.ThousandSep ?? ent.ThousandSep;
      ent.ListSep= this.ListSep ?? ent.ListSep;
      ent.DateFormat= this.DateFormat ?? ent.DateFormat;
      ent.TimeFormat= this.TimeFormat ?? ent.TimeFormat;
      ent.DateTimeFormat= this.DateTimeFormat ?? ent.DateTimeFormat;
      ent.IntegerFormat= this.IntegerFormat ?? ent.IntegerFormat;
      ent.FixedFormat= this.FixedFormat ?? ent.FixedFormat;
      ent.MonetaryFormat= this.MonetaryFormat ?? ent.MonetaryFormat;
      return ent;
    }

    public string? Lang;
    public string? DecimalSep;
    public string? ThousandSep;
    public string? ListSep;
    public string? DateFormat;
    public string? TimeFormat;
    public string? DateTimeFormat;
    public string? IntegerFormat;
    public string? FixedFormat;
    public string? MonetaryFormat;
  }
}