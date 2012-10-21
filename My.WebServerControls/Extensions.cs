#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;


#endregion



namespace My.WebServerControls
{
    public static class Extensions
    {
        public static void ReDataBind(this DropDownList ddl)
        {
            string s = ddl.SelectedValue;
            ddl.DataBind();
            ddl.SelectedValue = s;
        }
    }
}