#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
    /// <summary>
    ///     Select-метод должен быть кеширующим на текущий http-запрос - он выполняется два раза на DataBind()
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData(@"<{0}:CheckBoxListEx runat=""server"" ID="""" DataSourceID="""" DataTextField=""Caption"" DataValueField=""UserGroupId"" ToolTipField=""Desc""></{0}:CheckBoxListEx>")]
    public class CheckBoxListEx : CheckBoxList
    {
        public string DataCheckField { get; set; }
        public string ToolTipField { get; set; }
        public string ForeColorField { get; set; }
        public string EnabledField { get; set; }

        public string ForeColorOnChecked { get; set; }
        public string ForeColorOnNotChecked { get; set; }
        public bool BoldSelected { get; set; }


        public CheckBoxListEx()
        {
            DataBound += SetItemsStyleFromODS;
            SelectedIndexChanged += SelectedIndexChangedEx;
        }


        void SelectedIndexChangedEx(object sender, EventArgs e)
        {
            if (BoldSelected)
                foreach (ListItem item in base.Items.Cast<ListItem>().Where(item => item.Selected))
                    item.Attributes.CssStyle.Add(HtmlTextWriterStyle.FontWeight, "bold");
        }


        string ViewStateStateName { get { return this.ClientID + "addon"; } }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // load from ViewState
            if (Page.IsPostBack)
            {
                object props = ViewState[ViewStateStateName];
                if (props == null)
                    return;
                Dictionary<int, string[]> itemsStates = (Dictionary<int, string[]>) props;
                for (int i = 0; i < Items.Count; i++)
                {
                    ListItem li = Items[i];

                    if (li.Selected)
                    {
                        if (BoldSelected)
                            li.Attributes.CssStyle.Add(HtmlTextWriterStyle.FontWeight, "bold");
                        if (!string.IsNullOrEmpty(ForeColorOnChecked))
                            li.Attributes.CssStyle.Add(HtmlTextWriterStyle.Color, ForeColorOnChecked);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ForeColorOnNotChecked))
                            li.Attributes.CssStyle.Add(HtmlTextWriterStyle.Color, ForeColorOnNotChecked);
                    }

                    if (itemsStates.ContainsKey(i))
                    {
                        string[] states = itemsStates[i];
                        if (states.Length > 0 && !string.IsNullOrEmpty(states[0]))
                            li.Attributes.Add("title", states[0]);
                        if (states.Length > 1 && !string.IsNullOrEmpty(states[1]))
                            li.Attributes.CssStyle.Add(HtmlTextWriterStyle.Color, states[1]);
                        if (states.Length > 2 && !string.IsNullOrEmpty(states[2]))
                            li.Enabled = bool.Parse(states[2]);
                    }
                }
            }
        }


        /// <summary>
        ///     Chooses uses DataSource type and set items style
        /// </summary>
        void SetItemsStyleFromODS(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.DataCheckField) && string.IsNullOrEmpty(this.ForeColorField) && string.IsNullOrEmpty(this.ToolTipField))
                return;

            // Независимо от IsPostBack надо сделать Select - DataBind может быть и на IsPostBack

            object dataSource = FindDataSource;
            if (dataSource == null) return;

            Dictionary<int, string[]> itemsStates = new Dictionary<int, string[]>();

            if (dataSource is SqlDataSource)
            {
                SqlDataSource sqlDa = dataSource as SqlDataSource;

                DataView dataItems = sqlDa.Select(new DataSourceSelectArguments(base.DataValueField)) as DataView;
                if (dataItems == null) return;

                SetItemsStyleFromODS_Internal(itemsStates, dataItems, (di, i) => ((DataView) di).Table.Rows[i], (di, name) => ((DataRow) di)[name]);
            }
            else if (dataSource is ObjectDataSource)
            {
                ObjectDataSource ods = dataSource as ObjectDataSource;
                object[] dataItems = ods.Select().Cast<object>().ToArray();

                if (dataItems == null || dataItems.Length == 0) return;

                SetItemsStyleFromODS_Internal(itemsStates, dataItems, (di, i) => ((object[]) di)[i], (di, name) => di.GetType().InvokeMember(name, BindingFlags.GetProperty, null, di, null));
            }
            else if (dataSource is IEnumerable)
            {
                dataSource = ((IEnumerable) dataSource).Cast<object>().ToList();
                SetItemsStyleFromODS_Internal(itemsStates, dataSource, (di, i) => ((IList<object>) di)[i], (di, name) => di.GetType().InvokeMember(name, BindingFlags.GetProperty, null, di, null));
            }
            else
                throw new Exception(string.Format("CheckBoxListEx: such dataSource is not supported, {0}", dataSource));

            if (itemsStates != null && itemsStates.Count > 0)
                ViewState[ViewStateStateName] = itemsStates;
        }


        /// <summary>
        ///     Must fill itemsStates (ViewState collection-saved styles)
        /// </summary>
        void SetItemsStyleFromODS_Internal(Dictionary<int, string[]> itemsStates, object dataItems, Func<object, int, object> dataItemGetter,
                                           Func<object, string, object> dataFieldGetter)
        {
            for (int i = 0; i < base.Items.Count; i++)
            {
                object dataItem = dataItemGetter(dataItems, i);

                if (!string.IsNullOrEmpty(DataCheckField) && (bool) dataFieldGetter(dataItem, DataCheckField))
                {
                    base.Items[i].Selected = true;
                    if (!string.IsNullOrEmpty(ForeColorOnChecked))
                        base.Items[i].Attributes.CssStyle.Add(HtmlTextWriterStyle.Color, ForeColorOnChecked);
                    if (BoldSelected)
                        base.Items[i].Attributes.CssStyle.Add(HtmlTextWriterStyle.FontWeight, "bold");
                }
                else if (!string.IsNullOrEmpty(ForeColorOnNotChecked))
                    base.Items[i].Attributes.CssStyle.Add(HtmlTextWriterStyle.Color, ForeColorOnNotChecked);

                if (!string.IsNullOrEmpty(ToolTipField))
                {
                    object toolTipO = dataFieldGetter(dataItem, ToolTipField);
                    if (toolTipO != null)
                    {
                        string toolTip = toolTipO.ToString();
                        if (!string.IsNullOrEmpty(toolTip))
                        {
                            AddToViewState(itemsStates, toolTip, i, 0);
                            base.Items[i].Attributes.Add("title", toolTip);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(ForeColorField))
                {
                    object colorO = dataFieldGetter(dataItem, ForeColorField);
                    if (colorO != null)
                    {
                        string colorS = colorO.ToString();
                        if (!string.IsNullOrEmpty(colorS))
                        {
                            AddToViewState(itemsStates, colorS, i, 1);
                            base.Items[i].Attributes.CssStyle.Add(HtmlTextWriterStyle.Color, colorS);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(EnabledField))
                {
                    object enabledO = dataFieldGetter(dataItem, EnabledField);
                    if (enabledO != null)
                    {
                        bool enabled = bool.Parse(enabledO.ToString());
                        AddToViewState(itemsStates, enabledO.ToString(), i, 2);
                        base.Items[i].Enabled = enabled;
                    }
                }
            }
        }


        static void AddToViewState(Dictionary<int, string[]> itemsStates, string val, int itemNumber, int i)
        {
            if (!itemsStates.ContainsKey(itemNumber))
                itemsStates[itemNumber] = new string[3];
            itemsStates[itemNumber][i] = val;
        }


        object FindDataSource
        {
            get
            {
                string dataSourceId = base.DataSourceID;

                Control dataSource = Page.FindControl(dataSourceId);
                if (dataSource == null)
                {
                    dataSource = Parent.FindControl(dataSourceId);
                    if (dataSource == null)
                        if (Parent.Parent != null)
                        {
                            dataSource = Parent.Parent.FindControl(dataSourceId);
                            if (dataSource == null)
                                if (Parent.Parent.Parent != null)
                                    dataSource = Parent.Parent.Parent.FindControl(dataSourceId);
                        }
                }
                return dataSource ?? this.DataSource;
            }
        }


        [NotNull]
        public List<int> GetSelectedIntValues()
        {
            return this.Items.Cast<ListItem>().Where(li => li.Selected).Select(li => int.Parse(li.Value)).ToList();
        }


        [NotNull]
        public List<string> GetSelectedStringValues()
        {
            return this.Items.Cast<ListItem>().Where(li => li.Selected).Select(li => li.Value).ToList();
        }


        public void SetSelectedValues([CanBeNull] IEnumerable<string> values)
        {
            if (values == null) return;

            foreach (ListItem li in this.Items.Cast<ListItem>().Where(li => values.Contains(li.Value, StringComparer.InvariantCultureIgnoreCase)))
            {
                li.Selected = true;
                if (BoldSelected)
                    li.Attributes.CssStyle.Add(HtmlTextWriterStyle.FontWeight, "bold");
            }
        }


        public void SetSelectedValues([CanBeNull] IEnumerable<int> values)
        {
            if (values == null) return;
            int i;
            foreach (ListItem li in this.Items.Cast<ListItem>().Where(li => int.TryParse(li.Value, out i) && values.Contains(int.Parse(li.Value))))
            {
                li.Selected = true;
                if (BoldSelected)
                    li.Attributes.CssStyle.Add(HtmlTextWriterStyle.FontWeight, "bold");
            }
        }


        public void ReDataBind()
        {
            List<string> selected = this.GetSelectedStringValues();
            this.DataBind();
            this.SetSelectedValues(selected);
        }
    }
}