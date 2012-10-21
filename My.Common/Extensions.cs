#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml.Linq;

using JetBrains.Annotations;


#endregion



namespace My.Common
{
    public static class Extensions
    {
        // ReSharper disable UnusedMember.Global
        public static decimal CalcMedian(this decimal[] ar)
        {
            if (ar == null || ar.Length == 0) return 0;
            ar = ar.OrderBy(x => x).ToArray();
            if (ar.Length%2 == 1) return ar[ar.Length/2];
            return (ar[ar.Length/2 - 1] + ar[ar.Length/2])/2m;
        }


        public static string TrimOrNullIfEmpty(this string s)
        {
            return s == null || string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }


        public static string TrimAndHtmlEncode(this string s)
        {
            return HttpUtility.HtmlEncode(s.TrimOrNullIfEmpty());
        }


        /// <summary>
        ///     Does not add anything if val is empty. Name may be path (/-separated tag names)
        /// </summary>
        public static void AddSubElem([NotNull] this XElement e, [NotNull] string name, [CanBeNull] string val,
                                      Action<string> execIfMandatoryAndEmpty = null)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(val))
            {
                if (execIfMandatoryAndEmpty != null)
                        // specification error - sent notification but not to interrupt sending
                    execIfMandatoryAndEmpty(name);
                return;
            }

            if (name.Contains("/"))
            {
                AddSubSubElem(e, name, val);
                return;
            }

            XElement sube = new XElement(name);
            e.Add(sube);

            sube.Value = val;
        }


        /// <summary>
        ///     Does not add anything if val is empty
        /// </summary>
        /// <param name="path"> Separated by '/' tag names </param>
        static void AddSubSubElem([NotNull] this XElement e, [NotNull] string path, [CanBeNull] string val)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (string.IsNullOrEmpty(val)) return;

            string[] tags = path.Split('/');
            XElement sube = e;
            foreach (string tag in tags)
            {
                if (tag == "") throw new ArgumentException("path");

                XElement test = sube.Element(tag);
                if (test == null)
                {
                    XElement sube1 = new XElement(tag);
                    sube.Add(sube1);
                    sube = sube1;
                }
                else
                    sube = test;
            }
            sube.Value = val;
        }


        // ReSharper restore UnusedMember.Global
    }
}