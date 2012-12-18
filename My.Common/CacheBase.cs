#region usings
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;


#endregion



namespace My.Common
{
    /// <summary>
    ///     Can be used only for asp.net Cache object
    /// </summary>
    public abstract class CacheBase
    {
        protected readonly Dictionary<Type, Tuple<Func<int, object>, string[]>> _data = new Dictionary<Type, Tuple<Func<int, object>, string[]>>();

        protected abstract string DatabaseEntryName { get; }
        protected abstract string ConnectionStringName { get; }


        // ReSharper disable UnusedMemberInSuper.Global
        protected abstract void Init();
        // ReSharper restore UnusedMemberInSuper.Global


        protected T Get<T>(int id)
        {
            return (T) _data[typeof (T)].Item1(id);
        }


        protected string[] GetTables<T>()
        {
            return _data[typeof (T)].Item2;
        }


        protected AggregateCacheDependency GetAggregateCacheDependency<T>()
        {
            AggregateCacheDependency acd = new AggregateCacheDependency();
            IEnumerable<string> tables = GetTables<T>();
            foreach (string table in tables)
            {
                SqlCacheDependency cd = null;
                try
                {
                    cd = new SqlCacheDependency(DatabaseEntryName, table);
                }
                catch (DatabaseNotEnabledForNotificationException ex)
                {
                    SqlCacheDependencyAdmin.EnableNotifications(ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString);
                    SqlCacheDependencyAdmin.EnableTableForNotifications(ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString, table);
                }
                catch (TableNotEnabledForNotificationException ex)
                {
                    SqlCacheDependencyAdmin.EnableTableForNotifications(ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString, table);
                }
                if (cd == null)
                    cd = new SqlCacheDependency(DatabaseEntryName, table);
                acd.Add(cd);
            }

            return acd;
        }


        public T FindCached<T>(int id)
        {
            if (HttpContext.Current == null)
                return Get<T>(id);

            Cache c = HttpContext.Current.Cache;
            object dobj = c[typeof (T).Name];

            Dictionary<int, T> d;
            if (dobj != null && dobj is Dictionary<int, T>)
                d = (Dictionary<int, T>) dobj;
            else
            {
                d = new Dictionary<int, T>();
                AggregateCacheDependency acd = GetAggregateCacheDependency<T>();
                c.Insert(typeof (T).Name, d, acd, DateTime.Today.AddDays(1), TimeSpan.Zero, CacheItemPriority.Normal, null);
            }

            lock (d)
            {
                T o;
                if (d.TryGetValue(id, out o))
                    return o;
                o = Get<T>(id);
                if (o == null)
                    return default(T);
                d.Add(id, o);

                return o;
            }
        }
    }
}