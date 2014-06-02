#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

using JetBrains.Annotations;

using System.Linq;

#endregion



namespace My.Common.DAL
{
    public abstract partial class DbBase : MarshalByRefObjectNoTimeout
    {
        [NotNull]
        public string ConnectionString { get; protected set; }


        /// <summary>
        ///     In seconds
        /// </summary>
        // ReSharper disable ConvertToConstant.Global
        protected int CommandTimeout = 30;


        // ReSharper restore ConvertToConstant.Global


        /// <summary>
        ///     Sets CommandType, CommandTimeout, opens connection
        /// </summary>
        [NotNull]
        protected SqlCommand PrepareCommand([NotNull] SqlConnection con, CommandType ct = CommandType.Text, bool needOpenConn = true)
        {
            SqlCommand co = con.CreateCommand();
            co.CommandType = ct;
            co.CommandTimeout = this.CommandTimeout;
            if (needOpenConn)
                con.Open();
            return co;
        }


        protected static void AddListParam(string name, IEnumerable l, SqlCommand com)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("i");
            string sqltype = null;
            foreach (object i in l)
            {
                if (i != null && sqltype == null)
                    // first elem
                    if (i is int || i is int?)
                        sqltype = "Int";
                    else if (i is string)
                        sqltype = "String";
                    else
                        throw new Exception(string.Format("Type {0} not supported", i.GetType()));
                dt.Rows.Add(i);
            }
            SqlParameter p = com.Parameters.AddWithValue(name, dt);
            p.SqlDbType = SqlDbType.Structured;
            p.TypeName = string.Format("dbo.{0}List", sqltype ?? "Int");
        }


        /// <summary>
        ///     Supports only int and string lists.
        /// </summary>
        protected static void AddListParam<T>(string name, IEnumerable<T> l, SqlCommand com)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("i");
            foreach (T i in l)
                dt.Rows.Add(i);

            SqlParameter p = com.Parameters.AddWithValue(name, dt);
            p.SqlDbType = SqlDbType.Structured;
            string t;
            if (typeof (T) == typeof (int) || typeof (T) == typeof (int?))
                t = "Int";
            else if (typeof (T) == typeof (string))
                t = "String";
            else
                throw new Exception(string.Format("Type {0} not supported", typeof (T)));
            p.TypeName = string.Format("dbo.{0}List", t);
        }


        // ReSharper disable UnusedMember.Global
        // ReSharper disable CSharpWarnings::CS1573
        // ReSharper disable MemberCanBePrivate.Global


        #region update support
        /// <param name="pkFilters"> Must not contains DBNull parameters </param>
        [CanBeNull]
        protected T Update<T>([NotNull] UpdateBase u, [NotNull] string tableName,
                              [NotNull] KeyValuePair<string, object>[] pkFilters,
                              [CanBeNull] Func<IDataRecord, T> reader)
            where T : DtoDbBase<T>
        {
            bool outputInserted = reader != null;
            if (u.IsEmpty)
                return outputInserted ? DoSelect(tableName, pkFilters, reader) : null;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand com = conn.CreateCommand())
            {
                com.CommandType = CommandType.Text;

                StringBuilder sb;
                if (outputInserted)
                    sb = new StringBuilder("declare @id as table ("
                                           + string.Join(", ",
                                                         pkFilters.Select(
                                                             kvp => EnsureInBraces(kvp.Key) + " " + GetSqlType(kvp.Value.GetType())))
                                           + "); update ");
                else
                    sb = new StringBuilder("update ");

                sb.Append(EnsureInBraces(tableName));
                sb.Append(" set ");

                AddEqualityParameters(u.ChangedProps, com, sb, " , ");

                if (outputInserted)
                    sb.Append(" output " + string.Join(", ", pkFilters.Select(kvp => "inserted." + EnsureInBraces(kvp.Key))) + " into @id ");

                return AddWhereAndExecute(pkFilters, reader, conn, sb, com,
                                          outputInserted
                                              ? "; select t.* from " + EnsureInBraces(tableName) + " t where " +
                                                string.Join(" and ",
                                                            pkFilters.Select(
                                                                kvp =>
                                                                string.Format("t.{0}=(select top 1 {0} from @id)", EnsureInBraces(kvp.Key))))
                                              : null
                    );
            }
        }


        protected string GetSqlType(Type t)
        {
            if (t == typeof (Guid))
                return "uniqueidentifier";
            if (t == typeof (int))
                return "int";
            if (t == typeof (long))
                return "bigint";
            if (t == typeof (string))
                return "nvarchar(max)";
            if (t == typeof (short))
                return "smallint";
            if (t == typeof (byte))
                return "tinyint";
            if (t == typeof (bool))
                return "bit";
            if (t == typeof (decimal))
                return "decimal";
            if (t == typeof (char))
                return "nchar";
            if (t == typeof (DateTime))
                return "datetime";
            if (t == typeof (float))
                return "float";
            throw new Exception("Can not determine to sql type: " + t.Name);
        }


        /// <param name="pkFilters"> Must not contains DBNull parameters </param>
        protected void Update([NotNull] UpdateBase u, [NotNull] string tableName,
                              [NotNull] IEnumerable<KeyValuePair<string, object>> pkFilters)
        {
            Update<DtoDummy>(u, tableName, pkFilters.ToArray(), null);
        }


        protected void Update([NotNull] List<UpdateBase> uArr, [NotNull] string tableName,
                              [NotNull] List<IEnumerable<KeyValuePair<string, object>>> pkFiltersArr)
        {
            Update<DtoDummy>(uArr, tableName, pkFiltersArr, null);
        }


        protected List<T> Update<T>([NotNull] List<UpdateBase> uArr, [NotNull] string tableName,
                                    [NotNull] List<IEnumerable<KeyValuePair<string, object>>> pkFiltersArr,
                                    [CanBeNull] Func<IDataRecord, T> reader)
            where T : DtoDbBase<T>
        {
            if (uArr.Count != pkFiltersArr.Count)
                throw new Exception("Updater and filter lists must have same size");

            List<T> output = new List<T>();
            bool outputInserted = reader != null;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand com = conn.CreateCommand())
            {
                conn.Open();
                for (int i = 0; i < uArr.Count; i++)
                {
                    com.Parameters.Clear();
                    if (uArr[i].CountChangedProps == 0)
                        if (outputInserted)
                        {
                            StringBuilder sb = new StringBuilder("select * from  ");
                            sb.Append(EnsureInBraces(tableName));

                            sb.Append(" where ");

                            AddEqualityParameters(pkFiltersArr[i], com, sb, " and ");

                            com.CommandText = sb.ToString();

                            using (SqlDataReader r = com.ExecuteReader())
                                if (r.Read())
                                    output.Add(reader(r));
                        }
                        else
                            output.Add(null);
                    else
                    {
                        com.CommandType = CommandType.Text;

                        StringBuilder sb;
                        if (outputInserted)
                            sb = new StringBuilder("declare @id as table ("
                                                   + string.Join(", ",
                                                                 pkFiltersArr[i].Select(
                                                                     kvp => EnsureInBraces(kvp.Key) + " " + GetSqlType(kvp.Value.GetType())))
                                                   + "); update ");
                        else
                            sb = new StringBuilder("update ");

                        sb.Append(EnsureInBraces(tableName));
                        sb.Append(" set ");

                        AddEqualityParameters(uArr[i].ChangedProps, com, sb, " , ");

                        if (outputInserted)
                            sb.Append(" output " + string.Join(", ", pkFiltersArr[i].Select(kvp => "inserted." + EnsureInBraces(kvp.Key)))
                                      + " into @id ");

                        sb.Append(" where ");

                        AddEqualityParameters(pkFiltersArr[i], com, sb, " and ");

                        if (outputInserted)
                            sb.Append("; select t.* from " + EnsureInBraces(tableName) + " t where " +
                                      string.Join(" and ",
                                                  pkFiltersArr[i].Select(
                                                      kvp => string.Format("t.{0}=(select top 1 {0} from @id)", EnsureInBraces(kvp.Key))))
                                );

                        com.CommandText = sb.ToString();

                        if (reader == null)
                            com.ExecuteNonQuery();
                        else
                            using (SqlDataReader r = com.ExecuteReader())
                                if (r.Read())
                                    output.Add(reader(r));
                    }
                }
            }
            return output;
        }


        [CanBeNull]
        protected T DoSelect<T>([NotNull] string tableName, [NotNull] IEnumerable<KeyValuePair<string, object>> pkFilters,
                                [NotNull] Func<IDataRecord, T> reader)
            where T : DtoDbBase<T>
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand com = conn.CreateCommand())
            {
                com.CommandType = CommandType.Text;

                StringBuilder sb = new StringBuilder("select * from  ");
                sb.Append(EnsureInBraces(tableName));

                return AddWhereAndExecute(pkFilters, reader, conn, sb, com);
            }
        }


        [NotNull]
        static string EnsureInBraces([NotNull] string name)
        {
            name = name.Trim();
            if (name.StartsWith("dbo."))
                return "dbo." + EnsureInBraces(name.Substring(4));
            if (!name.StartsWith("["))
                name = "[" + name;
            if (!name.EndsWith("]"))
                name = name + "]";
            return name;
        }


        [NotNull]
        static string EnsureWOBraces([NotNull] string name)
        {
            name = name.Trim();
            if (name.StartsWith("["))
                name = name.Substring(1);
            if (name.EndsWith("]"))
                name = name.Substring(0, name.Length - 1);
            return name;
        }


        [CanBeNull]
        protected static T AddWhereAndExecute<T>([NotNull] IEnumerable<KeyValuePair<string, object>> pkFfilters,
                                                 [CanBeNull] Func<IDataRecord, T> reader, [NotNull] IDbConnection conn,
                                                 [NotNull] StringBuilder sb, [NotNull] SqlCommand com, [CanBeNull] string addon = null)
            where T : DtoDbBase<T>
        {
            sb.Append(" where ");

            AddEqualityParameters(pkFfilters, com, sb, " and ");

            if (!string.IsNullOrEmpty(addon))
                sb.Append(addon);

            com.CommandText = sb.ToString();

            conn.Open();

            if (reader == null)
                com.ExecuteNonQuery();
            else
                using (SqlDataReader r = com.ExecuteReader())
                    if (r.Read())
                        return reader(r);
            return null;
        }


        protected static void AddEqualityParameters([NotNull] IEnumerable<KeyValuePair<string, object>> equalities,
                                                    [NotNull] SqlCommand com, [NotNull] StringBuilder sb, [NotNull] string delimeter)
        {
            List<string> sets = new List<string>();
            foreach (KeyValuePair<string, object> f in equalities)
            {
                com.Parameters.AddWithValue(EnsureWOBraces(f.Key), f.Value ?? DBNull.Value);
                sets.Add(string.Format(" {0}=@{1} ", EnsureInBraces(f.Key), EnsureWOBraces(f.Key)));
            }
            sb.Append(string.Join(delimeter, sets));
        }
        #endregion


        #region get nullable support
        protected static T? GetNullableVal<T>(object o, T? def = default(T?)) where T : struct
        {
            return o == null || o == DBNull.Value ? def : (T) o;
        }


        [CanBeNull]
        protected static string GetNullableString(object o, string def = null)
        {
            return GetNullableRef<string>(o, def);
        }


        [CanBeNull]
        protected static T GetNullableRef<T>(object o, T def = default(T)) where T : class
        {
            return o == null || o is DBNull ? def : (T) o;
        }


        [NotNull]
        protected static object GetNullableParamVal<T>(T? p) where T : struct
        {
            return p.HasValue ? (object) (T) p : DBNull.Value;
        }


        [NotNull]
        protected static object GetNullableParamVal<T>(T p) where T : class
        {
            return p == null ? DBNull.Value : (object) p;
        }
        #endregion


        #region xmlserialization support
        public static string SerializeDTO<T>([NotNull] DtoDbBase<T> dto) where T : DtoDbBase<T>
        {
            XmlSerializer xmlSer = new XmlSerializer(dto.GetType());
            //using (TextWriter tw = new StreamWriter(xmlFilePath))
            //    xmlSer.Serialize(tw, dto);
            StringWriter sWriter = new StringWriter();
            xmlSer.Serialize(sWriter, dto);
            return sWriter.ToString();
        }


        public static DtoDbBase<T> DeserializeXml<T>(string xml, DtoDbBase<T> dto) where T : DtoDbBase<T>
        {
            XmlSerializer xmlSer = new XmlSerializer(dto.GetType());
            //using (TextReader tr = new StreamReader(xmlFilePath))
            //{
            //    DtoDbBase retDTO = (DtoDbBase) xmlSer.Deserialize(tr);
            //    return retDTO;
            //}
            StringReader sReader = new StringReader(xml);
            DtoDbBase<T> retDTO = (DtoDbBase<T>) xmlSer.Deserialize(sReader);
            return retDTO;
        }
        #endregion


        #region execute helpers
        /// <summary>
        ///     Parameter for sql-query
        /// </summary>
        [DebuggerDisplay("{Name} {Val}")]
        protected class P
        {
            // ReSharper disable FieldCanBeMadeReadOnly.Global
            public string Name;
            public object Val;
            // ReSharper restore FieldCanBeMadeReadOnly.Global


            public P(string name, object val)
            {
                Name = name;
                Val = val;
            }
        }



        /// <summary>
        ///     Query with parameters
        /// </summary>
        protected class Q
        {
            // ReSharper disable FieldCanBeMadeReadOnly.Global
            public string Sql;
            public IEnumerable<P> Pars;
            public CommandType Ct;
            // ReSharper restore FieldCanBeMadeReadOnly.Global


            public Q(string sql, IEnumerable<P> pars = null, CommandType ct = CommandType.Text)
            {
                Sql = sql;
                Pars = pars;
                Ct = ct;
            }


            public Q(string sql, params P[] pars)
            {
                Sql = sql;
                Pars = pars;
                Ct = CommandType.Text;
            }


            public Q(string sql, CommandType ct, params P[] pars)
            {
                Sql = sql;
                Pars = pars;
                Ct = ct;
            }
        }



        static void AddPars(IEnumerable<P> pars, SqlCommand com)
        {
            if (pars != null)
                foreach (P p in pars)
                    if (p.Val is IEnumerable && !(p.Val is string) && !(p.Val is IEnumerable<byte>))
                        AddListParam(p.Name, (IEnumerable) p.Val, com);
                    else
                        com.Parameters.AddWithValue(p.Name, GetNullableParamVal(p.Val));
        }


        #region ExecuteNonQuery
        protected int ExecuteNonQuery([NotNull] string sql, params P[] pars)
        {
            return ExecuteNonQuery(new Q(sql, pars));
        }


        protected int ExecuteNonQuery([NotNull] string sql, CommandType ct, params P[] pars)
        {
            return ExecuteNonQuery(new Q(sql, pars, ct));
        }


        protected int ExecuteNonQuery([NotNull] Q q)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                return ExecuteNonQuery(q, con);
            }
        }


        /// <summary>
        ///     Assumes that connection is already opened.
        /// </summary>
        protected int ExecuteNonQuery([NotNull] Q q, [NotNull] SqlConnection con)
        {
            using (SqlCommand com = PrepareCommand(con, q.Ct, needOpenConn: false))
            {
                com.CommandText = q.Sql;
                AddPars(q.Pars, com);
                return com.ExecuteNonQuery();
            }
        }
        #endregion


        #region ExecuteScalar
        protected T ExecuteScalar<T>([NotNull] string sql, params P[] pars)
        {
            return ExecuteScalar<T>(new Q(sql, pars));
        }


        protected T ExecuteScalar<T>([NotNull] string sql, CommandType ct, params P[] pars)
        {
            return ExecuteScalar<T>(new Q(sql, ct, pars));
        }


        protected T ExecuteScalar<T>([NotNull] Q q)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                return ExecuteScalar<T>(q, con);
            }
        }


        /// <summary>
        ///     Assumes that connection is already opened.
        /// </summary>
        protected T ExecuteScalar<T>([NotNull] Q q, [NotNull] SqlConnection con)
        {
            using (SqlCommand com = PrepareCommand(con, q.Ct, needOpenConn: false))
            {
                com.CommandText = q.Sql;
                AddPars(q.Pars, com);
                return (T) com.ExecuteScalar();
            }
        }
        #endregion


        #region ExecuteList
        protected List<T> ExecuteList<T>([NotNull] string sql, params P[] pars)
        {
            return ExecuteList<T>(new Q(sql, pars));
        }


        protected List<T> ExecuteList<T>([NotNull] string sql, Func<IDataRecord, T> reader, params P[] pars)
        {
            return ExecuteList<T>(new Q(sql, pars), reader);
        }


        protected List<T> ExecuteList<T>([NotNull] string sql, CommandType ct, Func<IDataRecord, T> reader = null, params P[] pars)
        {
            return ExecuteList(new Q(sql, ct, pars), reader);
        }


        protected List<T> ExecuteList<T>([NotNull] Q q, Func<IDataRecord, T> reader = null)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                return ExecuteList<T>(q, reader, con);
            }
        }


        /// <summary>
        ///     Assumes that connection is already opened.
        /// </summary>
        protected List<T> ExecuteList<T>([NotNull] Q q, [CanBeNull] Func<IDataRecord, T> reader, [NotNull] SqlConnection con)
        {
            using (SqlCommand com = PrepareCommand(con, q.Ct, needOpenConn: false))
            {
                com.CommandText = q.Sql;
                AddPars(q.Pars, com);
                using (SqlDataReader r = com.ExecuteReader())
                {
                    List<T> l = new List<T>();
                    while (r.Read())
                        l.Add(reader == null ? r[0] is DBNull ? default(T) : (T) r[0] : reader(r));
                    return l;
                }
            }
        }
        #endregion


        #region ExecuteReader
        protected void ExecuteReader([NotNull] string sql, [NotNull] Action<SqlDataReader> reader, params P[] pars)
        {
            ExecuteReader(new Q(sql, pars), reader);
        }


        protected void ExecuteReader([NotNull] string sql, CommandType ct, [NotNull] Action<SqlDataReader> reader, params P[] pars)
        {
            ExecuteReader(new Q(sql, pars, ct), reader);
        }


        protected void ExecuteReader([NotNull] Q q, [NotNull] Action<SqlDataReader> reader)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                ExecuteReader(q, reader, con);
            }
        }


        protected void ExecuteReader([NotNull] Q q, [NotNull] Action<SqlDataReader> reader, [NotNull] SqlConnection con)
        {
            using (SqlCommand com = PrepareCommand(con, q.Ct, needOpenConn: false))
            {
                com.CommandText = q.Sql;
                AddPars(q.Pars, com);
                using (SqlDataReader r = com.ExecuteReader())
                    reader(r);
            }
        }
        #endregion


        #endregion


        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore CSharpWarnings::CS1573
        // ReSharper restore UnusedMember.Global
    }
}