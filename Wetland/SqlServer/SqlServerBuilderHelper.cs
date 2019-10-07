/*
 * https://blog.csdn.net/sunshine_qqr/article/details/82011727
 */

using Open.ExpressionVisitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Wetland.Models;

namespace Wetland.SqlServer
{
    /// <summary>
    /// 对象拼接sql语句
    /// </summary>
    public class SqlServerBuilderHelper
    {
        /// <summary>
        /// Insert SQL语句
        /// </summary>
        /// <param name="obj">要转换的对象，不可空</param>
        /// <param name="tableName">要添加的表明，不可空</param>
        /// <returns>
        /// 空
        /// sql语句
        /// </returns>
        public static string InsertSql<T>(T t, string tableName)
        {
            if (t == null || string.IsNullOrEmpty(tableName))
            {
                return string.Empty;
            }
            string columns = GetColmons(t);
            if (string.IsNullOrEmpty(columns))
            {
                return string.Empty;
            }
            string values = GetValues(t);
            if (string.IsNullOrEmpty(values))
            {
                return string.Empty;
            }
            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT INTO " + tableName);
            sql.Append("(" + columns + ")");
            sql.Append(" VALUES(" + values + ")");
            return sql.ToString();
        }

        public static string InsertSql<T>(T t, string tableName, WetlandPrimaryKeyModel pk)
        {
            if (t == null || string.IsNullOrEmpty(tableName))
            {
                return string.Empty;
            }
            string columns = GetColmons(t);
            if (string.IsNullOrEmpty(columns))
            {
                return string.Empty;
            }
            string values = GetValues(t);
            if (string.IsNullOrEmpty(values))
            {
                return string.Empty;
            }

            string pkColumn = GetColmons(pk);
            string pkValue = GetValues(pk);
            columns=columns + "," + pkColumn;
            values = values + "," + pkValue;
            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT INTO " + tableName);
            sql.Append("(" + columns + ")");
            sql.Append(" VALUES(" + values + ")");
            return sql.ToString();
        }

        /// <summary>
        /// BulkInsert SQL语句（批量添加）
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="objs">要转换的对象集合，不可空</param>
        /// <param name="tableName">>要添加的表明，不可空</param>
        /// <returns>
        /// 空
        /// sql语句
        /// </returns>
        public static string BulkInsertSql<T>(List<T> objs, string tableName) where T : class
        {
            if (objs == null || objs.Count == 0 || string.IsNullOrEmpty(tableName))
            {
                return string.Empty;
            }
            string columns = GetColmons(objs[0]);
            if (string.IsNullOrEmpty(columns))
            {
                return string.Empty;
            }
            string values = string.Join(",", objs.Select(p => string.Format("({0})", GetValues(p))).ToArray());
            StringBuilder sql = new StringBuilder();
            sql.Append("Insert into " + tableName);
            sql.Append("(" + columns + ")");
            sql.Append(" values " + values + "");
            return sql.ToString();
        }

        /// <summary>
        /// 获得类型的列名
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string GetColmons<T>(T obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return string.Join(",", obj.GetType().GetProperties().Select(p => p.Name).ToList());
        }

        /// <summary>
        /// 获得值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string GetValues<T>(T obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return string.Join(",", obj.GetType().GetProperties().Select(p => string.Format("'{0}'", p.GetValue(obj))).ToArray());
        }

        public static string GetSqlByLambda<T>(Expression<Func<T, bool>> condition)
        {
            ExpressionSqlWriter writer = new ExpressionSqlWriter();
            string sql = writer.Translate(condition);
            return sql;
        }

        /// <summary>
        /// 参数化查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string GetSqlByLambda<T>(Expression<Func<T, bool>> condition,out List<object> parameters)
        {
            parameters = new List<object>();
            ExpressionParameterizedSqlWriter writer = new ExpressionParameterizedSqlWriter(parameters);
            string sql = writer.Translate(condition);
            return sql;
        }


    }
}
