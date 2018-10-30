using System;

namespace Tlabs.Data.Processing.Formula {
  ///<summary>Quarter of a date.</summary>
  public struct QuarterDate {
    /// TODO: Add int constructor
    DateTime d;
    ///<summary>Ctor from <paramref name="o"/>   .</summary>
    public QuarterDate(object o) {
      var date= o as DateTime?;
      IConvertible q;
      this.d= date.GetValueOrDefault();

      if (   null == date
          && null != (q= o as IConvertible)) {
        int qi= Convert.ToInt32(q);
        this.d= new DateTime(year(qi), month(qi), 1);
      }
    }

    ///<summary>Total number of quarters.</summary>
    public int Q => (12 * (d.Year-1) + d.Month-1) / 3;

    ///<summary>Quarter start date.</summary>
    public DateTime start => new DateTime(d.Year, month(Q), 1);

    ///<summary>Quarter end date.</summary>
    // public DateTime end => new DateTime(d.Year, endMonth, endMonth != 9 ? 31 : 30);
    // private int endMonth => (int)(this.Q % 4) * 3 +3;
    public DateTime end {
      get {
        var endMonth= month(Q) +2;
        return new DateTime(d.Year, endMonth, endMonth == 6 || endMonth == 9 ? 30 : 31);
      }
    }

    private static int month(int q) => (q % 4) * 3 +1;
    private static int year(int q) => q / 4 +1;

    ///<summary>Sub operator.</summary>
    public static int operator -(QuarterDate q1, QuarterDate q2) => q1.Q - q2.Q;
    ///<summary>&lt; operator.</summary>
    public static bool operator <(QuarterDate q1, QuarterDate q2) => q1.Q < q2.Q;
    ///<summary>&lt;= operator.</summary>
    public static bool operator <=(QuarterDate q1, QuarterDate q2) => q1.Q <= q2.Q;
    ///<summary>&gt;= operator.</summary>
    public static bool operator >(QuarterDate q1, QuarterDate q2) => q1.Q > q2.Q;
    ///<summary>&gt;= operator.</summary>
    public static bool operator >=(QuarterDate q1, QuarterDate q2) => q1.Q >= q2.Q;
    ///<summary>== operator.</summary>
    public static bool operator ==(QuarterDate q1, QuarterDate q2) => q1.Q == q2.Q;
    ///<summary>!= operator.</summary>
    public static bool operator !=(QuarterDate q1, QuarterDate q2) => q1.Q != q2.Q;
    ///<inherits/>
    public override bool Equals(object obj) {
      if (null == obj) return false;
      var d2= obj as QuarterDate?;
      return this.d == d2.GetValueOrDefault().d;
    }
    ///<inherits/>
    public override int GetHashCode() => d.GetHashCode();
  }
}