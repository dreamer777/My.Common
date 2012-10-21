#region usings
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;

using JetBrains.Annotations;

using NLog;


#endregion



namespace My.Common.Web
{
    public abstract class PageBase : Page
    {
        static readonly Logger Logger = LogManager.GetLogger("PageBase");


        public override void VerifyRenderingInServerForm(Control control)
        {
            /* Do nothing */
            //base.VerifyRenderingInServerForm();
            //Logger.Info("VerifyRenderingInServerForm, Control: " + control);
        }


        protected void ods_ObjectCreating(object sender, ObjectDataSourceEventArgs e)
        {
            e.ObjectInstance = this;
        }


        protected void ods_ObjectDisposing(object sender, ObjectDataSourceDisposingEventArgs e)
        {
            e.Cancel = true;
        }


        public List<T> FindControlsRecursive<T>([NotNull] Control rootControl, string id = null) where T : Control
        {
            List<T> l = new List<T>();
            _FindControlRecursive(rootControl, id, l);
            return l;
        }


        void _FindControlRecursive<T>([NotNull] Control rootControl, [CanBeNull] string id, [NotNull] List<T> l)
                where T : Control
        {
            if (rootControl is T && (string.IsNullOrEmpty(id) || rootControl.ID == id)) l.Add((T) rootControl);

            foreach (Control c in rootControl.Controls)
                _FindControlRecursive(c, id, l);
        }


        public static void ResponseAsFile([NotNull] string contents, [NotNull] string contentType, [NotNull] string filename,
                                          bool open)
        {
            HttpResponse r = HttpContext.Current.Response;
            r.Clear();
            r.Charset = "UTF-8";
            r.ContentType = contentType;

            string rv = @"  <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
                                                <html xmlns=""http://www.w3.org/1999/xhtml"" >
                                                <head>
                                                    <title>" + filename + @"</title>
                                                    <meta http-equiv=""content-type"" content=""text/html; charset=UTF-8"">
                                                    <meta http-equiv=""X-UA-Compatible"" content=""IE=EmulateIE8"">
                                                </head>
                                                <body>" + contents + @"</body></html>";

            if (contentType == "text/html" && open)
                r.Write(rv);
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(rv);

                if (!open)
                    r.AddHeader("Content-Disposition", "attachment; filename=" + filename);
                r.AddHeader("Content-Length", bytes.Length.ToString(CultureInfo.InvariantCulture));

                r.BinaryWrite(bytes);
            }
            r.End();
        }


        public static void ResponseAsOnlyControl([NotNull] Control control, string fileName = null, string contentType = "text/html")
        {
            HttpResponse r = HttpContext.Current.Response;

            r.Clear();
            if (fileName != null)
                r.AddHeader("Content-Disposition", "attachment;filename=" + fileName);
            r.Charset = "utf-8";
            r.ContentType = contentType;

            using (StringWriter stringWrite = new StringWriter())
            using (HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite))
            {
                control.RenderControl(htmlWrite);
                string s = stringWrite.ToString();
                r.Write(s);
            }

            r.Flush();
            r.End();
        }


        public static void ResponseAsExcelTable([NotNull] IEnumerable<object> a, [NotNull] string caption, [NotNull] string filename)
        {
            GridView gv = new GridView();
            gv.AutoGenerateColumns = true;
            gv.GridLines = GridLines.Both;
            gv.BorderWidth = 1;
            gv.RowStyle.HorizontalAlign = HorizontalAlign.Center;
            gv.RowStyle.VerticalAlign = VerticalAlign.Middle;
            gv.BorderWidth = 1;
            gv.BorderStyle = BorderStyle.Solid;
            gv.BorderColor = Color.Black;

            gv.DataSource = a;
            gv.DataBind();

            HttpResponse r = HttpContext.Current.Response;
            r.Clear();
            r.AddHeader("Content-Disposition", "attachment;filename=" + filename + ".xls");
            r.Charset = "utf-8";
            //EnableViewState = false;
            r.ContentType = "application/vnd.xls";

            using (StringWriter sw = new StringWriter())
            using (HtmlTextWriter hw = new HtmlTextWriter(sw))
            {
                hw.WriteLine(@"  <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
                                                <html xmlns=""http://www.w3.org/1999/xhtml"" >
                                                <head>
                                                    <meta http-equiv=""content-type"" content=""text/html; charset=UTF-8"">
                                                    <meta http-equiv=""X-UA-Compatible"" content=""IE=EmulateIE8"">
                                                </head>
                                                <body><style>td{text-align:center;horiz-align: center;vert-align: middle; vertical-align: central;}</style>");
                hw.WriteLine(string.Format("<h1>{0}</h1>", caption));
                gv.RenderControl(hw);
                hw.WriteLine(@"</body></html>");

                r.Write(sw.ToString());
            }
            r.Flush();
            r.End();
        }
    }
}