#region usings
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

using JetBrains.Annotations;

using My.Common.DAL;


#endregion



namespace My.Common.Web
{
    public class StatisticsFilterHelper<TFinder> where TFinder : FinderBase<TFinder>
    {
        readonly Func<TFinder> _finderGetter;
        readonly Func<TFinder, int> _cntGetter;
        readonly Func<int> _currentCntGetter;

        /// <summary>
        ///     Если true, статистика не считается (когда фильтр еще не проинициализирован)
        /// </summary>
        bool _isFilterInitializing;


        public StatisticsFilterHelper(Func<TFinder> finderGetter, Func<TFinder, int> cntGetter, Func<int> currentCntGetter)
        {
            _finderGetter = finderGetter;
            _cntGetter = cntGetter;
            _currentCntGetter = currentCntGetter;
        }


        public void SetInitComplete()
        {
            _isFilterInitializing = false;
        }


        public void SetInitStart()
        {
            _isFilterInitializing = true;
        }


        /// <summary>
        /// </summary>
        /// <typeparam name="TItem"> Тип элемента в списке cases </typeparam>
        /// <param name="anyCheckBox"> Если null, то значит выбираем для ddl. Если checked, то не выбираем ничего - список скрыт </param>
        /// <param name="textAny"> Если null, заполняется "любой" </param>
        /// <param name="foruiCalcer"> По данному case возвращает визуальный элемент </param>
        /// <param name="rvCache"> Кеширующая return value переменная - используется в случае, когда datasource больше одного раза запрашивает Select </param>
        public IEnumerable<ForUICommon> GetForUIListAndSetStats<TItem>(ref IEnumerable<ForUICommon> rvCache,
                                                                       Action<TFinder, List<TItem>> conditionSetter,
                                                                       [CanBeNull] CheckBox anyCheckBox,
                                                                       Func<List<TItem>> casesGetter,
                                                                       Func<TItem, ForUICommon> foruiCalcer = null,
                                                                       ForUICommon textAny = null)
        {
            if (rvCache != null) return rvCache;
            ForUICommon[] rv = this.GetForUIListAndSetStats(conditionSetter,
                                                            anyCheckBox,
                                                            casesGetter,
                                                            foruiCalcer,
                                                            textAny
                    ).ToArray();
            if (!_isFilterInitializing)
                rvCache = rv;
            return rv;
        }


        /// <summary>
        /// </summary>
        /// <typeparam name="TItem"> Тип элемента в списке cases </typeparam>
        /// <param name="anyCheckBox"> Если null, то значит выбираем для ddl. Если checked, то не выбираем ничего - список скрыт </param>
        /// <param name="textAny"> Если null, заполняется "любой" </param>
        /// <param name="foruiCalcer"> По данному case возвращает визуальный элемент </param>
        public IEnumerable<ForUICommon> GetForUIListAndSetStats<TItem>(Action<TFinder, List<TItem>> conditionSetter,
                                                                       [CanBeNull] CheckBox anyCheckBox,
                                                                       Func<List<TItem>> casesGetter,
                                                                       Func<TItem, ForUICommon> foruiCalcer = null,
                                                                       ForUICommon textAny = null)
        {
            if (textAny == null)
                textAny = new ForUICommon("любой", "");
            if (anyCheckBox != null && anyCheckBox.Checked)
            {
                anyCheckBox.Text = textAny.Caption;
                return new ForUICommon[0]; // если выбрано "любой", то ниче не считаем
            }

            if (foruiCalcer == null)
            {
                if (!typeof (ForUICommon).IsAssignableFrom(typeof (TItem)))
                    throw new Exception(typeof (TItem).Name + " is not ForUI class - you must pass foruiCalcer parameter");
                foruiCalcer = x => x as ForUICommon;
            }

            if (_isFilterInitializing) // нужно, чтобы заполнить варианты выбора без статистики до того, как полностью проинициализируется вель фильтр
                return AddAllOrNot(casesGetter().Select(foruiCalcer), textAny, anyCheckBox);

            TFinder userFinder = _finderGetter();
            TFinder f = userFinder.Clone();

            conditionSetter(f, null);
            int? cntAll = GetAllCountAndSetAnyComboBoxTitle(anyCheckBox, f, userFinder, textAny);
            List<TItem> cases = casesGetter();
            IEnumerable<KeyValuePair<TItem, ForUICommon>> casesKvp = cases.Select(x => new KeyValuePair<TItem, ForUICommon>(x, foruiCalcer(x)));
            if (cntAll == null || cntAll <= 0)
                return AddAllOrNot(casesKvp.Select(kvp => new ForUICommon(kvp.Value.Caption + " (0)", kvp.Value.ToolTip, kvp.Value.Id) {ForeColor = Color.LightGray}),
                                   textAny, anyCheckBox);

            return AddAllOrNot(GetForUIListForCblChooser(f, userFinder, casesKvp, conditionSetter), textAny, anyCheckBox);
        }


        static IEnumerable<ForUICommon> AddAllOrNot(IEnumerable<ForUICommon> cases, ForUICommon textAny, [CanBeNull] CheckBox anyCheckBox)
        {
            return anyCheckBox == null ? new List<ForUICommon> {textAny}.Concat(cases) : cases;
        }


        /// <summary>
        ///     Calcs count for finder if checkbox is checked. Sets title "Any" for without condition or "Any (count)" with it. Corresponding filter condition must be nulled before call.
        /// </summary>
        /// <returns> Count without corresponding filter condition or null if checkbox is checked </returns>
        int? GetAllCountAndSetAnyComboBoxTitle([CanBeNull] CheckBox anyCheckBox,
                                               TFinder finder, TFinder userFinder,
                                               ForUICommon textAny)
        {
            if (anyCheckBox == null || !anyCheckBox.Checked)
            {
                int cntAll = GetCnt(finder, userFinder);
                if (anyCheckBox == null) // выбираем для ddl
                    textAny.Caption = textAny.Caption + " (" + cntAll + ")";
                else
                {
                    //  ! anyCheckBox.Checked - список развернут
                    anyCheckBox.Text = textAny.Caption + " (" + cntAll + ")";
                    if (cntAll == 0)
                        anyCheckBox.ForeColor = Color.LightGray;
                }
                return cntAll;
            }

            anyCheckBox.Text = textAny.Caption;
            return null;
        }


        IEnumerable<ForUICommon> GetForUIListForCblChooser<TItem>(TFinder finder, TFinder userFinder,
                                                                  IEnumerable<KeyValuePair<TItem, ForUICommon>> items,
                                                                  Action<TFinder, List<TItem>> conditionSetter)
        {
            List<ForUICommon> l = new List<ForUICommon>();
            foreach (KeyValuePair<TItem, ForUICommon> kvp in items)
            {
                conditionSetter(finder, new List<TItem> {kvp.Key});
                int cnt = GetCnt(finder, userFinder);
                l.Add(new ForUICommon(kvp.Value.Caption + " (" + cnt + ")", kvp.Value.ToolTip, kvp.Value.Id)
                      {
                              ForeColor = cnt == 0 ? Color.LightGray : kvp.Value.ForeColor
                      });
            }
            return l;
        }


        int GetCnt(TFinder finder, TFinder userFinder)
        {
            if (finder == userFinder)
                return _currentCntGetter();
            return _cntGetter(finder);
        }
    }
}