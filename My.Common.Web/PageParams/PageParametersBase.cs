#region usings
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

using JetBrains.Annotations;


#endregion



namespace My.Common.Web
{
    public abstract class PageParametersBase
    {
        readonly char[] SeparatorInArray = new[] {',', ';'};


        protected int[] GetIntArFromQueryString([NotNull] HttpContext c, [NotNull] string name, bool canOmit)
        {
            string s = c.Request.QueryString[name];
            if (string.IsNullOrEmpty(s))
                if (!canOmit)
                    throw RequestParamsException.CreateAsNoRequiredParam(name);
                else
                    return null;

            string[] ss = s.Split(SeparatorInArray, StringSplitOptions.RemoveEmptyEntries);
            int[] rv = ss.Select(x =>
                                     {
                                         int val;
                                         if (int.TryParse(x, out val))
                                             return val;
                                         throw RequestParamsException.CreateAsWrongIntAr(name, s);
                                     }).ToArray();
            return rv;
        }


        protected string[] GetStringArFromQueryString([NotNull] HttpContext c, [NotNull] string name, bool canOmit)
        {
            string s = c.Request.QueryString[name];
            if (string.IsNullOrEmpty(s))
                if (!canOmit)
                    throw RequestParamsException.CreateAsNoRequiredParam(name);
                else
                    return null;

            string[] ss = s.Split(SeparatorInArray, StringSplitOptions.RemoveEmptyEntries).Select(x => x.TrimOrNullIfEmpty())
                    .Where(x => x != null).ToArray();
            return ss;
        }


        protected int? GetIntFromQueryString([NotNull] HttpContext c, [NotNull] string name, bool canOmit)
        {
            string s = c.Request.QueryString[name];
            if (string.IsNullOrEmpty(s))
                if (!canOmit)
                    throw RequestParamsException.CreateAsNoRequiredParam(name);
                else
                    return null;
            int val;
            if (!int.TryParse(s, out val))
                throw RequestParamsException.CreateAsWrongInt(name, s);
            return val;
        }


        protected bool? GetBoolFromQueryString([NotNull] HttpContext c, [NotNull] string name, bool canOmit)
        {
            string s = c.Request.QueryString[name];
            if (string.IsNullOrEmpty(s))
                if (!canOmit)
                    throw RequestParamsException.CreateAsNoRequiredParam(name);
                else
                    return null;
            bool val;
            if (!bool.TryParse(s, out val))
                throw RequestParamsException.CreateAsWrongBool(name, s);
            return val;
        }

        protected decimal? GetDecimalFromQueryString([NotNull] HttpContext c, [NotNull] string name, bool canOmit)
        {
            string s = c.Request.QueryString[name];
            if (string.IsNullOrEmpty(s))
                if (!canOmit)
                    throw RequestParamsException.CreateAsNoRequiredParam(name);
                else
                    return null;
            decimal? val;
            try { val = Parser.ParseDecimalNullable(s); }
            catch (FormatException) { throw RequestParamsException.CreateAsWrongDecimal(name, s); }
            return val;
        }



        protected DateTime? GetDateTimeFromQueryString([NotNull] HttpContext c, [NotNull] string name, bool canOmit)
        {
            string s = c.Request.QueryString[name];
            if (string.IsNullOrEmpty(s))
                if (!canOmit)
                    throw RequestParamsException.CreateAsNoRequiredParam(name);
                else
                    return null;
            DateTime val;
            //if (!DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None,  out val))
            if (!DateTime.TryParseExact(s, new[]
                                           {
                                                   "yyyy.MM.dd", "yyyy.MM.ddTHH:mm:ss", "yyyy.MM.dd HH:mm:ss", "yyyy.MM.ddTHH:mm", "yyyy.MM.dd HH:mm",
                                                   "dd.MM.yyyy", "dd.MM.yyyyTHH:mm:ss", "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyyTHH:mm", "dd.MM.yyyy HH:mm",
                                                   "dd.MM.yy", "dd.MM.yyTHH:mm:ss", "dd.MM.yy HH:mm:ss", "dd.MM.yyTHH:mm", "dd.MM.yy HH:mm",
                                           }, CultureInfo.InvariantCulture, DateTimeStyles.None, out val))
                throw RequestParamsException.CreateAsWrongDateTime(name, s);
            return val;
        }


        [CanBeNull]
        protected string GetStringFromQueryString([NotNull] HttpContext c, [NotNull] string name, bool canOmit)
        {
            string s = c.Request.QueryString[name];
            if (string.IsNullOrEmpty(s))
                if (!canOmit)
                    throw RequestParamsException.CreateAsNoRequiredParam(name);
            return s;
        }


        protected Guid? GetGuidFromQueryString([NotNull] HttpContext c, [NotNull] string name, bool canOmit)
        {
            string s = c.Request.QueryString[name];
            if (string.IsNullOrEmpty(s))
                if (!canOmit)
                    throw RequestParamsException.CreateAsNoRequiredParam(name);
                else
                    return null;
            Guid guid;
            if (!Guid.TryParse(s, out guid))
                throw RequestParamsException.CreateAsWrongGuid(name, s);
            return guid;
        }
    }
}