#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

using My.Common.DAL;


#endregion



namespace My.Common.Web
{
    /// <summary>
    ///     With paging - Select(string orderBy, int count, int startIndex)
    /// </summary>
    public interface IPageWithOneStatFilter<TFinder> where TFinder : FinderBase<TFinder>
    {
        /// <summary>
        ///     Must be called in Page_Load in !PostBack section at the beginning.
        /// </summary>
        [UsedImplicitly]
        void LoadFilterFromQS();


        /// <summary>
        ///     Must be called in Page_Load in !PostBack section at the end and in FilterChanged at the end.
        /// </summary>
        [UsedImplicitly]
        void SetFilterLink();


        /// <summary>
        ///     Must be called in SetFilterLink.
        /// </summary>
        [UsedImplicitly]
        string CalcQueryStringForUrl(TFinder f);


        /// <summary>
        ///     Must be called in LoadFilterFromQS section at the end and in FilterChanged at the end.
        /// </summary>
        [UsedImplicitly]
        void ReDataBindFilterCases();


        /// <summary>
        ///     Must be called in SelectCount and in constructor for _filterHelper. Should cache that count in field.
        /// </summary>
        [UsedImplicitly]
        int GetCurrentCount();


        [UsedImplicitly]
        TFinder Finder { get; }


        [UsedImplicitly]
        void FilterChanged(object sender, EventArgs e);


        [UsedImplicitly]
        void bRefresh_OnClick(object sender, EventArgs e);


        [UsedImplicitly]
        int SelectCount();


        [UsedImplicitly]
        IEnumerable<object> Select(string orderBy, int count, int startIndex);
    }
}