using System;
using System.Collections.Generic;

namespace Tlabs.Data.Entity {
  ///<summary>Insured person.</summary>
  public class Locale : Intern.BaseEntity {
    ///<summary>Language</summary>
    public string Lang { get; set; }

    ///<summary>Decimal separator</summary>
    public string DecimalSep { get; set; }

    ///<summary>Thousand separator</summary>
    public string ThousandSep { get; set; }

    ///<summary>List separator</summary>
    public string ListSep { get; set; }

    ///<summary>Date only format</summary>
    public string DateFormat { get; set; }

    ///<summary>Time only format</summary>
    public string TimeFormat { get; set; }

    ///<summary>Date-Time format</summary>
    public string DateTimeFormat { get; set; }

    ///<summary>Integer format</summary>
    public string IntegerFormat { get; set; }

    ///<summary>Fixed (point) format</summary>
    public string FixedFormat { get; set; }

    ///<summary>Monetary format</summary>
    public string MonetaryFormat { get; set; }
  }
}