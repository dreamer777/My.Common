#region usings
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#endregion



namespace My.Common
{
    public class ConfigCommon
    {
        static string Get(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }


        public static string GetString(string name)
        {
            return Get(name);
        }


        public static int? GetInt(string name)
        {
            string s = Get(name);
            if (s == null)
                return null;
            return int.Parse(s);
        }


        public static int GetIntWithDef(string name, bool required = false, int def = 0)
        {
            string s = Get(name);
            if (s == null)
                if (required)
                    throw new Exception(string.Format("Required parameter [{0}] is not exists in config", name));
                else
                    return def;
            return int.Parse(s);
        }


        public static TimeSpan GetTimespanWithDef(string name, bool required = false, TimeSpan def = default(TimeSpan))
        {
            string s = Get(name);
            if (s == null)
                if (required)
                    throw new Exception(string.Format("Required parameter [{0}] is not exists in config", name));
                else
                    return def;
            return TimeSpan.Parse(s);
        }
    }
}