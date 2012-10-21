#region usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

using JetBrains.Annotations;


#endregion



namespace My.Common
{
    [DebuggerDisplay("{Id} {Caption} {Color}")]
    public class ForUICommon
    {
        public ForUICommon(string id)
        {
            Id = id;
        }


        public ForUICommon(int id) : this(id.ToString())
        {
            IdInt = id;
        }


        public ForUICommon(string caption, string id)
        {
            Caption = caption;
            Id = id;
        }


        public ForUICommon(string caption, int id) : this(caption, id.ToString())
        {
            IdInt = id;
        }


        public ForUICommon(string caption, string tooltip, string id)
        {
            Caption = caption;
            ToolTip = tooltip;
            Id = id;
        }


        public ForUICommon(string caption, string tooltip, int id) : this(caption, tooltip, id.ToString())
        {
            IdInt = id;
        }


        public ForUICommon(string captionBase, int count, string id, string tooltip = null)
        {
            Caption = string.Format("{0} ({1})", captionBase, count);
            Id = id;
            ForeColor = count == 0 ? System.Drawing.Color.LightGray : System.Drawing.Color.Black;
            ToolTip = tooltip;
        }


        public ForUICommon(string captionBase, int count, int id, string tooltip = null) : this(captionBase, count, id.ToString(), tooltip)
        {
            IdInt = id;
        }


        [UsedImplicitly]
        public string Id { get; private set; }

        [UsedImplicitly]
        public int? IdInt { get; private set; }

        [UsedImplicitly]
        public string Caption { get; set; }

        [UsedImplicitly]
        public string ToolTip { get; set; }

        [UsedImplicitly]
        public string Color
        {
            get
            {
                return ForeColor == System.Drawing.Color.Black || ForeColor == System.Drawing.Color.Empty
                               ? ""
                               : ForeColor.ToKnownColor().ToString();
            }
        }

        [UsedImplicitly]
        public Color ForeColor { get; set; }

        [UsedImplicitly]
        public bool Checked { get; set; }

        [UsedImplicitly]
        public bool Enabled { get { return _enabled; } set { _enabled = value; } }

        bool _enabled = true;


        protected void Init(For f, Action ifCbl, Action ifDdl)
        {
            if (f == For.Cbl)
                ifCbl();
            else if (f == For.Ddl)
                ifDdl();
            else throw new Exception(string.Format("For {0} is not supported", f));
        }



        public enum For
        {
            /// <summary>
            ///     Fills large caption and no tooltip
            /// </summary>
            Ddl,

            /// <summary>
            ///     Fills simple caption and large tooltip
            /// </summary>
            Cbl
        }
    }
}