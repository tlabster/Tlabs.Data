using System;

namespace Tlabs.Data.Serialize.Json {

  ///<summary><see cref="DateTime"/> utility extensions.</summary>
  public static class JsDateExtension {
    ///<summary>JavaScript Date epoch (1/1/1970) as UTC <see cref="DateTime"/></summary>
    public static readonly DateTime JS_UTC_EPOCH= new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    ///<summary>Milliseconds since JavaScript epoch <see cref="JS_UTC_EPOCH"/></summary>
    public static long JsTotalMsec(this DateTime dateTime) {
      /* When a DateTime gets loaded from a persistent store its 'Kind'</c>' is typically 'Unspecified'.
       * For that reason we can't demand DateTime of being 'specific' and stick with the assumption they are UTC...
       * if (0 != dateTime.Ticks && DateTimeKind.Unspecified == dateTime.Kind) throw new ArgumentException($"Can't convert from {DateTimeKind.Unspecified}.");
       */
      return (long)App.TimeInfo.ToUtc(dateTime).Subtract(JS_UTC_EPOCH).TotalMilliseconds;
    }

    ///<summary>Converts JavaScript millisec. (from Date.getTime()) into an Application Time <see cref="DateTime"/>.</summary>
    public static DateTime FromJsMsecToDateTime(this long jsMillisec) => App.TimeInfo.ToAppTime(JS_UTC_EPOCH.AddMilliseconds(jsMillisec));
  }

}