#region usings
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using JetBrains.Annotations;


#endregion



namespace My.Common
{
    public static class Parser
    {
        static readonly string[] DateTimeFormats = new string[]
                                                   {
                                                           "dd.MM.yyyy HH:mm:ss",
                                                           "dd.MM.yyyy HH:mm",
                                                           "dd.MM.yy HH:mm:ss",
                                                           "dd.MM.yyyy",
                                                           "dd.MM.yy",
                                                           "yyyy.MM.dd",
                                                   };


        public static DateTime? ParseDateTime([CanBeNull] string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;
            return DateTime.ParseExact(s, DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
        }


        public static decimal ParseDecimal([NotNull] string s)
        {
            if (!PrepareForDecimalParse(ref s)) throw new FormatException("Required decimal value is omitted");
            s = s.Replace(',', '.');
            return decimal.Parse(s, CultureInfo.InvariantCulture);
        }


        public static int ParseInt([NotNull] string s)
        {
            if (!PrepareForDecimalParse(ref s)) throw new FormatException("Required int value is omitted");
            return int.Parse(s, CultureInfo.InvariantCulture);
        }


        public static decimal? ParseDecimalNullable([CanBeNull] string s)
        {
            if (!PrepareForDecimalParse(ref s)) return null;
            s = s.Replace(',', '.');
            return decimal.Parse(s, CultureInfo.InvariantCulture);
        }


        public static int? ParseIntNullable([CanBeNull] string s)
        {
            if (!PrepareForDecimalParse(ref s)) return null;
            return int.Parse(s, CultureInfo.InvariantCulture);
        }


        static bool PrepareForDecimalParse(ref string s)
        {
            if (s == null)
                return false;
            s = Regex.Replace(s, @"(\s+|(&nbsp;)+)", "");
            return s != "";
        }


        public static string TimeSpanForUI(object ts, bool isForTextBox = false)
        {
            if (ts is TimeSpan? && ((TimeSpan?) ts).HasValue)
                return TimeSpanForUI(((TimeSpan?) ts).Value, isForTextBox);
            return null;
        }


        public static string TimeSpanForUI(TimeSpan ts, bool isForTextBox = false)
        {
            TimeSpan oneSec = new TimeSpan(0, 0, 1);
            return ts < oneSec ? "0"
                           : ts.ToString(isForTextBox
                                                 ? (
                                                           (ts.Days > 0 ? @"%d\ " : "")
                                                           + @"hh\:"
                                                           + @"mm"
                                                           + (ts.Seconds > 0 ? @"\:ss" : "")
                                                   )
                                                 : (
                                                           (ts.Days > 0 ? @"%d\д\ " : "")
                                                           + (ts.Hours > 0 ? @"%h\ч\ " : "")
                                                           + (ts.Minutes > 0 ? @"%m\м\ " : "")
                                                           + (ts.Seconds > 0 ? @"%s\с" : "")
                                                   )
                                     );
        }


        public static string DateTimeForUI(object eval, bool onlyDate = false, bool isForTextBox = false, bool omitDateIfToday = false,
                                           bool withSeconds = true, DateTime? omitDateIfSame = null, bool omitYearIfCurrent = true)
        {
            DateTime? dt = eval as DateTime?;
            if (dt.HasValue)
                return DateTimeForUI(dt.Value, onlyDate, isForTextBox, omitDateIfToday, withSeconds, omitDateIfSame, omitYearIfCurrent);
            return "";
        }


        public static string DateTimeForUI(DateTime dt, bool onlyDate = false, bool isForTextBox = false, bool omitDateIfToday = false,
                                           bool withSeconds = true, DateTime? omitDateIfSame = null, bool omitYearIfCurrent = true)
        {
            bool omitDate = omitDateIfToday && dt.Date == DateTime.Today || omitDateIfSame.HasValue && omitDateIfSame.Value.Date == dt.Date;

            return dt.ToString((isForTextBox
                                        ? "dd.MM.yyyy"
                                        : (omitDate ? ""
                                                   : (
                                                             "d MMM." +
                                                             (omitYearIfCurrent && dt.Year == DateTime.Now.Year
                                                                      ? ""
                                                                      : " yyyyг")
                                                     )
                                          )
                               )
                               + (onlyDate && !omitDate ? "" : (" HH:mm" + (withSeconds ? ":ss" : ""))));
        }


        public static string SizeForUI(long size)
        {
            if (size < 1 << 10)
                return size.ToString() + " B";
            if (size < 1 << 20)
                return (size >> 10).ToString() + " KB";
            if (size < 1 << 30)
                return (size >> 20).ToString() + "." + (((size%(1 << 20)) >> 10)/100).ToString() + " MB";

            return (size >> 30).ToString() + "." + (((size%(1 << 30)) >> 20)/100).ToString() + " GB";
        }
    }
}