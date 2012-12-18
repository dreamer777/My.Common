#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

using NLog;


#endregion



namespace My.Common.Web
{
    public abstract class PageBaseLogined<TParameters> : PageBaseAnonym<TParameters>
            where TParameters : PageParametersBase
    {
        static readonly Logger Logger = LogManager.GetLogger("PageBaseLogined");


        [UsedImplicitly]
        protected override void Page_PreInit()
        {
            try
            {
                bool notAmi1 = !AmI(1);
                if (notAmi1)
                    AuthCommon();
                CheckParsAndCreatePs();
                if (notAmi1)
                    AuthDepended();
                // 3. Set page mode - controls visibility, enabled etc - this will be done on Page_PreRenderComplete in page
            }
            catch (PageDeniedException ex)
            {
                if (IsPostBack)
                {
                    Logger.WarnException("", ex);
                    Response.Redirect(Request.RawUrl, true);
                }
                else
                    SetResponseAsMyException(ex);
            }
        }

        #region auth
        // ReSharper disable StaticFieldInGenericType
        [NotNull] static readonly int[] EmptyRoles = new int[] {};
        // ReSharper restore StaticFieldInGenericType

        /// <summary>
        ///     Which roles are allowed to view this page
        /// </summary>
        protected virtual int[] GrantedRoles { get { return EmptyRoles; } }

        protected abstract IEnumerable<int> MyRoles { get; }


        /// <summary>
        ///     1. Common auth - depends just on page and user roles
        /// </summary>
        void AuthCommon()
        {
            int[] grantedRoles = GrantedRoles;
            if (grantedRoles != null && grantedRoles.Length != 0 && !AmI(grantedRoles))
                throw new PageDeniedException(string.Format("Недостаточно прав для этой страницы.<br/>(Разрешенные роли: {0}, ваши роли: {1})",
                                                            string.Join(", ", grantedRoles),
                                                            string.Join(", ", MyRoles)));
        }


        /// <summary>
        ///     Am I in given roles?
        /// </summary>
        public bool AmI(IEnumerable<int> roles)
        {
            return MyRoles.Intersect(roles).Any();
        }


        /// <summary>
        ///     Am I in given roles?
        /// </summary>
        public bool AmI(params int[] roles)
        {
            return MyRoles.Intersect(roles).Any();
        }


        /// <summary>
        ///     Am I in given role?
        /// </summary>
        public bool AmI(int roleId)
        {
            return MyRoles.Any(rid => rid == roleId);
        }


        /// <summary>
        ///     2. Depended auth - fe "user can see only own supplier"
        /// </summary>
        protected virtual void AuthDepended() {}


        /// <summary>
        ///     Set page mode - controls visibility, enabled etc
        /// </summary>
        [UsedImplicitly]
        protected virtual void Page_LoadComplete()
        {
            if (!AmI(1))
                AuthVisual();
        }


        //// <summary>
        /////     Set page mode - controls visibility, enabled etc
        ///// </summary>
        //[UsedImplicitly]
        //protected void Page_PreRenderComplete()
        //{
        //    if (!AmI(1))
        //        AuthVisual();
        //}

        /// <summary>
        ///     Is called in Page_LoadComplete, should set Visible and Enabled props depending by roles or other rules
        /// </summary>
        protected virtual void AuthVisual() {}
        #endregion
    }
}