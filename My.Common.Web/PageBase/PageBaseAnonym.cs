#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using JetBrains.Annotations;

using NLog;

using My.Common.DAL;


#endregion



namespace My.Common.Web
{
    public abstract class PageBaseAnonym<TParameters> : PageBase
            where TParameters : PageParametersBase
    {
        static readonly Logger _logger = LogManager.GetLogger("PageBaseAnonym");

        /// <summary>
        ///     Page parameters
        /// </summary>
        [NotNull]
        protected TParameters Ps { get; private set; }


        [UsedImplicitly]
        protected virtual void Page_PreInit()
        {
            try
            {
                CheckParsAndCreatePs();
            }
            catch (PageDeniedException ex)
            {
                _logger.WarnException("", ex);
                SetResponseAsMyException(ex);
            }
        }


        protected void SetResponseAsMyException([NotNull] MyException ex)
        {
            Response.Write(GetResponseAsMyException(ex));
            Response.End();
        }


        protected virtual string GetResponseAsMyException([NotNull] MyException ex)
        {
            return ex.Message + "<br/><br/><a href='/default.aspx'>Перейти на главную<a>"
                   + " <a href='" + Request.UrlReferrer + "'>Назад<a>";
        }


        /// <summary>
        ///     Create parameters (this.Ps object)
        /// </summary>
        protected void CheckParsAndCreatePs()
        {
            try
            {
                if (typeof (TParameters) == typeof (NoParams))
                    Ps = (TParameters) (object) new NoParams();
                else
                    Ps = (TParameters) Activator.CreateInstance(typeof (TParameters), Context, GetDb());
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is MyException)
                    throw new PageDeniedException(ex.InnerException.Message);
                throw new PageDeniedException(ex.ToString());
            }
            catch (MyException ex)
            {
                throw new PageDeniedException(ex.Message);
            }
        }


        [NotNull]
        protected abstract DbBase GetDb();
    }
}