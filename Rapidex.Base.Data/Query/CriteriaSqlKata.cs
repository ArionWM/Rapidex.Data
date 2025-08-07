//using SqlKata;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Rapidex.Data.Query
//{
//    [Obsolete("", true)]
//    public class CriteriaSqlKata : IDbCriteria
//    {
//        public IDbEntityMetadata EntityMetadata { get; protected set; }
//        public string Alias { get; protected set; }
//        public SqlKata.Query Query { get; } = new SqlKata.Query();

//        public CriteriaSqlKata()
//        {
//            this.Alias = RandomHelper.RandomText(5);
//            this.Query.QueryAlias = Alias;
//        }

//        public CriteriaSqlKata(IDbEntityMetadata em) : this()
//        {
//            this.EntityMetadata = em;
//        }

//        public CriteriaSqlKata(string entityName) : this()
//        {
//            this.EntityMetadata = Database.Metadata.Get(entityName);
//        }

//        internal CriteriaSqlKata(string alias, SqlKata.Query query)
//        {
//            this.Alias = alias;
//            this.Query = query;
//            this.Query.QueryAlias = Alias;
//        }


//        public CriteriaSqlKata Where(string column, string op, object value)
//        {
//            Query.Where(column, op, value);
//            return this;

//        }

//        public CriteriaSqlKata WhereNot(string column, string op, object value)
//        {
//            Query.WhereNot(column, op, value);
//            return this;

//        }

//        public CriteriaSqlKata OrWhere(string column, string op, object value)
//        {
//            Query.OrWhere(column, op, value);
//            return this;


//        }

//        public CriteriaSqlKata OrWhereNot(string column, string op, object value)
//        {
//            Query.OrWhereNot(column, op, value);
//            return this;


//        }

//        public CriteriaSqlKata Where(string column, object value)
//        {
//            Query.Where(column, value);
//            return this;

//        }
//        public CriteriaSqlKata WhereNot(string column, object value)
//        {
//            Query.WhereNot(column, value);
//            return this;

//        }

//        public CriteriaSqlKata OrWhere(string column, object value)
//        {
//            Query.OrWhere(column, value);
//            return this;



//        }

//        public CriteriaSqlKata OrWhereNot(string column, object value)
//        {
//            Query.OrWhereNot(column, value);
//            return this;

//        }

//        /// <summary>
//        /// Perform a where constraint
//        /// </summary>
//        /// <param name="constraints"></param>
//        /// <returns></returns>
//        public CriteriaSqlKata Where(object constraints)
//        {
//            Query.Where(constraints);
//            return this;
//        }

//        public CriteriaSqlKata Where(IEnumerable<KeyValuePair<string, object>> values)
//        {
//            Query.Where(values);
//            return this;

//        }

//        public CriteriaSqlKata WhereRaw(string sql, params object[] bindings)
//        {
//            Query.WhereRaw(sql, bindings);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereRaw(string sql, params object[] bindings)
//        {
//            Query.OrWhereRaw(sql, bindings);
//            return this;

//        }

//        /// <summary>
//        /// Apply a nested where clause
//        /// </summary>
//        /// <param name="callback"></param>
//        /// <returns></returns>
//        public CriteriaSqlKata Where(Func<SqlKata.Query, SqlKata.Query> callback)
//        {
//            Query.Where(callback);
//            return this;

//        }

//        public CriteriaSqlKata WhereNot(Func<SqlKata.Query, SqlKata.Query> callback)
//        {
//            Query.WhereNot(callback);
//            return this;

//        }

//        public CriteriaSqlKata OrWhere(Func<SqlKata.Query, SqlKata.Query> callback)
//        {
//            Query.OrWhere(callback);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereNot(Func<SqlKata.Query, SqlKata.Query> callback)
//        {
//            Query.OrWhereNot(callback);
//            return this;

//        }

//        public CriteriaSqlKata WhereColumns(string first, string op, string second)
//        {
//            Query.WhereColumns(first, op, second);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereColumns(string first, string op, string second)
//        {
//            Query.OrWhereColumns(first, op, second);
//            return this;

//        }

//        public CriteriaSqlKata WhereNull(string column)
//        {
//            Query.WhereNull(column);
//            return this;

//        }

//        public CriteriaSqlKata WhereNotNull(string column)
//        {
//            Query.WhereNotNull(column);
//            return this;


//        }

//        public CriteriaSqlKata OrWhereNull(string column)
//        {
//            Query.WhereNull(column);

//            return this;

//        }

//        public CriteriaSqlKata OrWhereNotNull(string column)
//        {
//            Query.OrWhereNotNull(column);
//            return this;

//        }

//        public CriteriaSqlKata WhereTrue(string column)
//        {
//            Query.WhereTrue(column);
//            return this;


//        }

//        public CriteriaSqlKata OrWhereTrue(string column)
//        {
//            Query.OrWhereTrue(column);
//            return this;

//        }

//        public CriteriaSqlKata WhereFalse(string column)
//        {
//            Query.WhereFalse(column);
//            return this;


//        }

//        public CriteriaSqlKata OrWhereFalse(string column)
//        {
//            Query.OrWhereFalse(column);
//            return this;


//        }

//        public CriteriaSqlKata WhereLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
//        {
//            Query.WhereLike(column, value, caseSensitive, escapeCharacter);
//            return this;

//        }

//        public CriteriaSqlKata WhereNotLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
//        {
//            Query.WhereNotLike(column, value, caseSensitive, escapeCharacter);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
//        {
//            Query.OrWhereLike(column, value, caseSensitive, escapeCharacter);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereNotLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
//        {
//            Query.OrWhereNotLike(column, value, caseSensitive, escapeCharacter);
//            return this;

//        }
//        public CriteriaSqlKata WhereStarts(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
//        {
//            Query.WhereStarts(column, value, caseSensitive, escapeCharacter);
//            return this;

//        }

//        public CriteriaSqlKata WhereNotStarts(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
//        {
//            Query.WhereNotStarts(column, value, caseSensitive, escapeCharacter);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereStarts(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
//        {
//            Query.OrWhereStarts(column, value, caseSensitive, escapeCharacter);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereNotStarts(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
//        {
//            Query.OrWhereNotStarts(column, value, caseSensitive, escapeCharacter);
//            return this;

//        }

//        public CriteriaSqlKata WhereEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
//        {
//            Query.WhereEnds(column, value, caseSensitive, escapeCharacter);
//            return this;

//        }

//        public CriteriaSqlKata WhereNotEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
//        {
//            Query.WhereNotEnds(column, value, caseSensitive, escapeCharacter);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
//        {
//            Query.OrWhereEnds(column, value, caseSensitive, escapeCharacter);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereNotEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
//        {
//            Query.OrWhereNotEnds(column, value, caseSensitive, escapeCharacter);
//            return this;

//        }

//        public CriteriaSqlKata WhereContains(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
//        {
//            Query.WhereContains(column, value, caseSensitive, escapeCharacter);
//            return this;

//        }

//        public CriteriaSqlKata WhereNotContains(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
//        {
//            Query.WhereNotContains(column, value, caseSensitive, escapeCharacter);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereContains(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
//        {
//            Query.OrWhereContains(column, value, caseSensitive, escapeCharacter);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereNotContains(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
//        {
//            Query.OrWhereNotContains(column, value, caseSensitive, escapeCharacter);
//            return this;

//        }

//        public CriteriaSqlKata WhereBetween<T>(string column, T lower, T higher)
//        {
//            Query.WhereBetween(column, lower, higher);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereBetween<T>(string column, T lower, T higher)
//        {
//            Query.OrWhereBetween(column, lower, higher);
//            return this;

//        }
//        public CriteriaSqlKata WhereNotBetween<T>(string column, T lower, T higher)
//        {
//            Query.WhereNotBetween(column, lower, higher);
//            return this;

//        }
//        public CriteriaSqlKata OrWhereNotBetween<T>(string column, T lower, T higher)
//        {
//            Query.OrWhereNotBetween(column, lower, higher);
//            return this;

//        }

//        public CriteriaSqlKata WhereIn<T>(string column, IEnumerable<T> values)
//        {
//            Query.WhereIn(column, values);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereIn<T>(string column, IEnumerable<T> values)
//        {
//            Query.OrWhereIn(column, values);
//            return this;

//        }

//        public CriteriaSqlKata WhereNotIn<T>(string column, IEnumerable<T> values)
//        {
//            Query.WhereNotIn(column, values);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereNotIn<T>(string column, IEnumerable<T> values)
//        {
//            Query.OrWhereNotIn(column, values);
//            return this;

//        }


//        public CriteriaSqlKata WhereIn(string column, SqlKata.Query query)
//        {
//            Query.WhereIn(column, query);
//            return this;

//        }
//        public CriteriaSqlKata WhereIn(string column, Func<SqlKata.Query, SqlKata.Query> callback)
//        {
//            Query.WhereIn(column, callback);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereIn(string column, SqlKata.Query query)
//        {
//            Query.OrWhereIn(column, query);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereIn(string column, Func<SqlKata.Query, SqlKata.Query> callback)
//        {
//            Query.OrWhereIn(column, callback);
//            return this;

//        }
//        public CriteriaSqlKata WhereNotIn(string column, SqlKata.Query query)
//        {
//            Query.WhereNotIn(column, query);
//            return this;

//        }

//        public CriteriaSqlKata WhereNotIn(string column, Func<SqlKata.Query, SqlKata.Query> callback)
//        {
//            Query.WhereNotIn(column, callback);
//            return this;


//        }

//        public CriteriaSqlKata OrWhereNotIn(string column, SqlKata.Query query)
//        {
//            Query.OrWhereNotIn(column, query);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereNotIn(string column, Func<SqlKata.Query, SqlKata.Query> callback)
//        {
//            Query.OrWhereNotIn(column, callback);
//            return this;

//        }


//        /// <summary>
//        /// Perform a sub query where clause
//        /// </summary>
//        /// <param name="column"></param>
//        /// <param name="op"></param>
//        /// <param name="callback"></param>
//        /// <returns></returns>
//        public CriteriaSqlKata Where(string column, string op, Func<SqlKata.Query, SqlKata.Query> callback)
//        {
//            Query.Where(column, op, callback);
//            return this;

//        }

//        public CriteriaSqlKata Where(string column, string op, SqlKata.Query query)
//        {
//            Query.Where(column, op, query);
//            return this;

//        }

//        public CriteriaSqlKata WhereSub(SqlKata.Query query, object value)
//        {
//            Query.WhereSub(query, value);
//            return this;

//        }

//        public CriteriaSqlKata WhereSub(SqlKata.Query query, string op, object value)
//        {
//            Query.WhereSub(query, op, value);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereSub(SqlKata.Query query, object value)
//        {
//            Query.OrWhereSub(query, value);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereSub(SqlKata.Query query, string op, object value)
//        {
//            Query.OrWhereSub(query, op, value);
//            return this;

//        }

//        public CriteriaSqlKata OrWhere(string column, string op, SqlKata.Query query)
//        {
//            Query.OrWhere(column, op, query);
//            return this;

//        }
//        public CriteriaSqlKata OrWhere(string column, string op, Func<SqlKata.Query, SqlKata.Query> callback)
//        {
//            Query.OrWhere(column, op, callback);
//            return this;

//        }

//        public CriteriaSqlKata WhereExists(SqlKata.Query query)
//        {
//            Query.WhereExists(query);
//            return this;

//        }
//        public CriteriaSqlKata WhereExists(Func<SqlKata.Query, SqlKata.Query> callback)
//        {
//            Query.WhereExists(callback);
//            return this;

//        }

//        public CriteriaSqlKata WhereNotExists(SqlKata.Query query)
//        {
//            Query.WhereNotExists(query);
//            return this;

//        }

//        public CriteriaSqlKata WhereNotExists(Func<SqlKata.Query, SqlKata.Query> callback)
//        {
//            Query.WhereNotExists(callback);
//            return this;

//        }

//        public CriteriaSqlKata OrWhereExists(SqlKata.Query query)
//        {
//            Query.OrWhereExists(query);
//            return this;

//        }
//        public CriteriaSqlKata OrWhereExists(Func<SqlKata.Query, SqlKata.Query> callback)
//        {
//            Query.OrWhereExists(callback);
//            return this;

//        }
//        public CriteriaSqlKata OrWhereNotExists(SqlKata.Query query)
//        {
//            Query.OrWhereNotExists(query);
//            return this;

//        }
//        public CriteriaSqlKata OrWhereNotExists(Func<SqlKata.Query, SqlKata.Query> callback)
//        {
//            Query.OrWhereNotExists(callback);
//            return this;

//        }


//    }
//}
