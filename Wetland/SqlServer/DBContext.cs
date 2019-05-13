using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Wetland.DBAttribute;
using Wetland.Models;

namespace Wetland.SqlServer
{
    public partial class DBContext : IDBContext
    {
        public bool Add<T>(T entity)
        {
            lock (_InsertLook)
            {
                string tableName = GetTableNameRuleByDate<T>(entity);
                var temp = (WetlandBaseModel)(object)entity;
                temp.WetlandID = GetMaxWetlandIDRule(tableName) + 1;
                string sql = SqlServerBuilderHelper.InsertSql<T>(entity, string.Format("[{0}].[dbo].[{1}]", dbName, tableName));
                var cmd = writer.GetSqlStringCommand(sql);
                var result = writer.ExecuteNonQuery(cmd);
                return result > 0;
            }
        }
        public List<T> Get<T>(Expression<System.Func<T, bool>> where,DBPageBase page)
        {
            var result = new List<T>();//返回结果
            List<TableDataCount> tdcList = new List<TableDataCount>();//查询符合要求总条数 
            var actualList = new List<TableDataCount>();//实际参与查询的数据表集合

            #region  查询符合要求总条数 
            var tables=Get_Wetland_RelTable<T>();
            string whereSql = SqlServerBuilderHelper.GetSqlByLambda(where);
            string allRowSql = string.Empty;
            for (int i = 0; i < tables.Count; i++)
            {
                var tempRowSql = " SELECT COUNT(1) AS [Count],'{0}' AS [TableName] FROM {1} WHERE 1=1 ";
                if (!string.IsNullOrEmpty(whereSql))
                {
                    tempRowSql += "AND " + whereSql;
                }
                tempRowSql = string.Format(tempRowSql,tables[i].TableName, string.Format("[{0}].[dbo].[{1}]", dbName, tables[i].TableName));
                if (tables.Count != (i + 1))
                {
                    tempRowSql += " UNION ALL  ";
                }
                allRowSql += tempRowSql;
            }
            DbCommand allRowCommand = reader.GetSqlStringCommand(allRowSql);
            using (IDataReader dataReader = reader.ExecuteReader(allRowCommand))
            {
                while (dataReader.Read())
                {
                    tdcList.Add(new TableDataCount()
                    {
                        Count = Convert.ToInt32(dataReader["Count"].ToString()),
                        TableName = dataReader["TableName"].ToString(),
                    });
                }
            }
            #endregion

            #region 实际参与查询的数据表集合
            int startRow = page.PageSize * (page.PageIndex - 1);
           
            int tempCount = 0;
            for (int i = 0; i < tdcList.Count; i++)
            {
                if (tdcList[i].Count == 0)
                {
                    continue;
                }
                actualList.Add(tdcList[i]);
                tempCount += tdcList[i].Count;
                if (startRow > tempCount && (startRow + page.PageSize) >tempCount )
                {
                    break;
                }
            }

            string bodySql = string.Empty;
            for (int i = 0; i < actualList.Count; i++)
            {
                var tempBodySql = " SELECT * FROM [{0}].[dbo].[{1}] WHERE 1=1 ";
                if (!string.IsNullOrEmpty(whereSql))
                {
                    tempBodySql += "AND " + whereSql;
                }
                tempBodySql = string.Format(tempBodySql, dbName,tables[i].TableName);
                if (tables.Count != (i + 1))
                {
                    tempBodySql += " UNION ALL  ";
                }
                bodySql += tempBodySql;
            }
            #endregion

            #region 数据查询结果
            if (actualList.Count != 0)
            {
                string resultSql = " SELECT TOP " + page.PageSize + " * FROM  ";
                resultSql += " (  ";
                resultSql += " SELECT ROW_NUMBER() OVER (ORDER BY WetlandID) AS RowNumber,*  ";
                resultSql += " FROM(  ";
                resultSql += " " + bodySql + "  ";
                resultSql += " )AS T)AS R ";
                resultSql += " WHERE R.RowNumber > " + startRow + " ";
                if (string.IsNullOrEmpty(page.OrderBy))
                {
                    resultSql += page.OrderBy;
                }
                else
                {
                    resultSql += " ORDER BY WetlandID DESC  ";
                }

                DbCommand resultSqlCommand = reader.GetSqlStringCommand(resultSql);
                using (IDataReader dataReader = reader.ExecuteReader(resultSqlCommand))
                {
                    result = DataReaderConvert.ReaderToList<T>(dataReader);
                }
            }
            #endregion

            page.TotalRows = tdcList.Sum(s => s.Count);
            return result;
        }
    }

}
