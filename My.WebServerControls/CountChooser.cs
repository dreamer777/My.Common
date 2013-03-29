#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using JetBrains.Annotations;


#endregion



namespace My.WebServerControls
{
    [DefaultProperty("Text")]
    [ToolboxData(@"<{0}:CountChooser runat=""server"" ID="""" Counts=""10,20,50,100,200"" Title=""Выводить на странице:"" DefaultValue=""100""/>")]
    public class CountChooser : DropDownList
    {
        int[] _counts;

        public string Counts
        {
            set
            {
                _counts = value.Split(new[] {';', ','}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s)).Select(int.Parse).ToArray();
            }
        }


        public void SetAsCount(int cnt)
        {
            this.Visible = cnt >= _counts.Min();
            if (!this.Visible) return;

            int prevSelectedValue = SelectedValue;

            if (cnt <= _counts.Max())
            {
                int beginCut = _counts.Where(s => s >= cnt).Min();
                this.Items.Clear();
                this.Items.AddRange(_counts.Where(s => s < cnt).Select(s => new ListItem(s.ToString(), s.ToString()))
                                            .Concat(new[]
                                                    {
                                                            new ListItem("вывести все", (prevSelectedValue < beginCut ? beginCut : prevSelectedValue)
                                                                                                .ToString())
                                                    })
                                            .ToArray());
            }
            else
            {
                this.DataSource = _counts;
                this.DataBind();
            }
            SelectedValue = prevSelectedValue;
        }


        public string Title { get; set; }

        public new int SelectedValue { get { return string.IsNullOrEmpty(base.SelectedValue) ? DefaultValue : int.Parse(base.SelectedValue); } set { base.SelectedValue = value.ToString(); } }

        public int DefaultValue
        {
            get
            {
                object obj = ViewState["df"];
                return (obj == null) ? _counts.Min() : (int) obj;
            }
            set
            {
                ViewState["df"] = value;
                SelectedValue = value;
            }
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                DataSource = _counts;
                DataBind();
            }
        }


        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write(Title);
            base.Render(writer);
        }
    }
}