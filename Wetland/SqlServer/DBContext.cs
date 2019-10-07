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
                var pk = new WetlandPrimaryKeyModel();
                pk.WetlandID = GetMaxWetlandIDRule(tableName) + 1;
                string sql = SqlServerBuilderHelper.InsertSql<T>(entity, string.Format("[{0}].[dbo].[{1}]", dbName, tableName),pk);
                var cmd = writer.GetSqlStringCommand(sql);
                var result = writer.ExecuteNonQuery(cmd);
                return result > 0;
            }
        }
        public virtual List<T> Get<T>(Expression<System.Func<T, bool>> where, DBPageBase page)
        {
            var result = new List<T>();//返回结果
            List<TableDataCount> tdcList = new List<TableDataCount>();//查询符合要求总条数 
            var actualList = new List<TableDataCount>();//实际参与查询的数据表集合

            #region  查询符合要求总条数 
            var tables = Get_Wetland_RelTable<T>();

            if (tables.Count == 0)
            {
                return result;
            }
            string whereSql = SqlServerBuilderHelper.GetSqlByLambda(where);
            string allRowSql = string.Empty;
            for (int i = 0; i < tables.Count; i++)
            {
                var tempRowSql = " SELECT COUNT(1) AS [Count],'{0}' AS [TableName] FROM {1} WHERE 1=1 ";
                if (!string.IsNullOrEmpty(whereSql))
                {
                    tempRowSql += "AND " + whereSql;
                }
                tempRowSql = string.Format(tempRowSql, tables[i].TableName, string.Format("[{0}].[dbo].[{1}]", dbName, tables[i].TableName));
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
            tdcList = tdcList.Where(p => p.Count > 0).ToList();
            page.TotalRows = tdcList.Sum(s => s.Count);
            var maxPageRows = page.PageIndex * page.PageSize;
            if (maxPageRows > page.TotalRows)
            {
                return result;
            }
            #endregion

            #region 自定义分块算法
            var blockBase = this.WetlandPageRule(page.PageIndex, page.PageSize, tdcList);
            string bodySql = string.Empty;
            for (int i = blockBase.BlockStart; i <= blockBase.BlockEnd; i++)
            {
                var sql = "  SELECT  * FROM ( ";
                sql += " SELECT ROW_NUMBER() OVER (ORDER BY WetlandID) AS GroupRowNumber,* FROM ";
                sql += " [{0}].[dbo].[{1}] ";
                sql = string.Format(sql, dbName, tdcList[i].TableName);
                sql += "  WHERE 1 = 1 ";
                if (!string.IsNullOrEmpty(whereSql))
                {
                    sql += " AND " + whereSql;
                }
                sql += " )AS K  WHERE 1=1 ";

                if (i == blockBase.BlockStart)
                {
                    sql += " AND K.GroupRowNumber > " + blockBase.BlockStartNum;
                    if (blockBase.BlockStart == blockBase.BlockEnd)
                    {
                        sql += " AND K.GroupRowNumber <= " + blockBase.BlockEndNum;
                    }
                }
                else if (i == blockBase.BlockEnd)
                {
                    sql += " AND K.GroupRowNumber <= " + blockBase.BlockEndNum;
                }

                if (!string.IsNullOrEmpty(page.OrderBy))
                {
                    sql += " " + page.OrderBy;
                }

                if (i != blockBase.BlockEnd)
                {
                    sql += " UNION ALL ";
                }
                bodySql += sql;
            }
            #endregion

            #region 数据查询结果

            if (tdcList.Count != 0)
            {
                string resultSql = " SELECT  * FROM  ";
                resultSql += " (  ";
                resultSql += " SELECT ROW_NUMBER() OVER (ORDER BY WetlandID) AS RowNumber,*  ";
                resultSql += " FROM(  ";
                resultSql += " " + bodySql + "  ";
                resultSql += " )AS T)AS R ";
                if (!string.IsNullOrEmpty(page.OrderBy))
                {
                    resultSql += page.OrderBy;
                }
                else
                {
                    resultSql += " ORDER BY WetlandID DESC ";
                }

                DbCommand resultSqlCommand = reader.GetSqlStringCommand(resultSql);
                using (IDataReader dataReader = reader.ExecuteReader(resultSqlCommand))
                {
                    result = DataReaderConvert.ReaderToList<T>(dataReader);
                }
            }
            #endregion

            return result;
        }

        public bool AddList<T>(List<T> entitys)
        {
            lock (_InsertLook)
            {
                for (int i = 0; i < entitys.Count; i++)
                {
                    string tableName = GetTableNameRuleByDate<T>(entitys[i]);
                    var pk = new WetlandPrimaryKeyModel();
                    pk.WetlandID = GetMaxWetlandIDRule(tableName) + 1;
                    string sql = SqlServerBuilderHelper.InsertSql<T>(entitys[i], string.Format("[{0}].[dbo].[{1}]", dbName, tableName), pk);
                    var cmd = writer.GetSqlStringCommand(sql);
                    var result = writer.ExecuteNonQuery(cmd);
                }
                return true;
            }
        }

        private  BlockBase WetlandPageRule(int pageIndex, int pageSize, List<TableDataCount> tdcList)
        {
            int startRow = pageSize * (pageIndex - 1);
            int endRow = startRow + pageSize;
            int blockStart = 0;
            int blockStartNum = 0;
            int blockEnd = 0;
            int blockEndNum = 0;

            int startAccumulation = 0;
            //起始点
            for (int i = 0; i < tdcList.Count; i++)
            {
                startAccumulation += tdcList[i].Count;
                if (startAccumulation - startRow >= 0)
                {
                    blockStartNum = tdcList[i].Count - (startAccumulation - startRow);
                    blockStart = i;
                    break;
                }
            }
            int endAccumulation = 0;
            //结束点
            for (int i = 0; i < tdcList.Count; i++)
            {
                endAccumulation += tdcList[i].Count;
                if (endAccumulation - endRow >= 0)
                {
                    blockEndNum = tdcList[i].Count - (endAccumulation - endRow);
                    blockEnd = i;
                    break;
                }
            }
            //最大页设置
            if (blockEnd == 0 && blockEndNum == 0)
            {
                blockEnd = tdcList.Count;
                blockEndNum = endAccumulation;
            }
            return new BlockBase()
            {
                BlockEndNum = blockEndNum,
                BlockEnd = blockEnd,
                BlockStartNum = blockStartNum,
                BlockStart = blockStart,
            };
        }

    }

}
