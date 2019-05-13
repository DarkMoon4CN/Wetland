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
    public partial class DBContext
    {
        private static Database writer = null; 
        private static Database reader = null; 
        private string relTable = string.Empty;
        private string dbName = string.Empty;
        private object _InsertLook = new object();

        public DBContext()
        {

        }

        public DBContext(string dbName, string relTable)
        {
            writer = SqlServerDatabase.Writer;
            reader = SqlServerDatabase.Reader;
            this.relTable = relTable;
            this.dbName = dbName;
            Create_Wetland_Database(dbName);
            Create_Wetland_RelTable(relTable);
        }

        /// <summary>
        ///  构造时，勿指定数据库:data source=.;uid=sa;pwd=123456;
        /// </summary>
        /// <param name="dbName">水平分库 数据库名</param>
        /// <param name="relTable">水平分库 关系表名</param>
        /// <param name="writerConn">connectionStrings 节点名称</param>
        /// <param name="readerConn">connectionStrings 节点名称</param>
        public DBContext(string dbName, string relTable,string writerConn,string readerConn)
        {
            writer= DatabaseFactory.CreateDatabase(writerConn);
            reader = DatabaseFactory.CreateDatabase(readerConn);
            this.relTable = relTable;
            this.dbName = dbName;
            Create_Wetland_Database(dbName);
            Create_Wetland_RelTable(relTable);
        }

        private bool Create_Wetland_Database(string dbName)
        {
            if (!CheckDatabaseExist(dbName))
            {
                string sql = "CREATE DATABASE {0}";
                sql = string.Format(sql, dbName);
                DbCommand cmd = reader.GetSqlStringCommand(sql);
                int result = reader.ExecuteNonQuery(cmd);
                return result > 0;
            }
            return false;
        }

        private bool CheckDatabaseExist(string dbName)
        {
            string sql = "SELECT database_id FROM sys.databases WHERE Name  = '{0}'";
            sql = string.Format(sql, dbName);
            DbCommand cmd = reader.GetSqlStringCommand(sql);
            if (reader.ExecuteScalar(cmd) != null)
            {
                return Convert.ToInt32(reader.ExecuteScalar(cmd)) > 0;
            }
            else
            {
                return false;
            }
        }

        private bool Create_Wetland_RelTable(string tableName)
        {
            if (!CheckTableExist(tableName))
            {
                string sql = "CREATE TABLE [{0}].[dbo].[{1}]" +
                "([ID]  NVARCHAR(64)  NOT NULL," +
                "[SourceTableName] NVARCHAR(512) NOT NULL," +
                "[TableName] NVARCHAR(512) NOT NULL," +
                "[CreateTime] DATETIME NOT NULL," +
                "[LastPKValue] NVARCHAR(512) NOT NULL," +
                "CONSTRAINT [PK_B_TbUser_ID] PRIMARY KEY CLUSTERED ([ID] ASC));";
                sql = string.Format(sql, dbName, tableName);
                DbCommand cmd = reader.GetSqlStringCommand(sql);
                if (reader.ExecuteScalar(cmd) != null)
                {
                    return Convert.ToInt32(reader.ExecuteScalar(cmd)) > 0;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        private bool CheckTableExist(string tableName)
        {
            string sql = "SELECT COUNT(1) FROM [{0}].information_schema.TABLES WHERE TABLE_NAME ='{1}'";
            sql = string.Format(sql, dbName, tableName);
            DbCommand cmd = reader.GetSqlStringCommand(sql);
            if (reader.ExecuteScalar(cmd) != null)
            {
                return Convert.ToInt32(reader.ExecuteScalar(cmd)) > 0;
            }
            else
            {
                return false;
            }
        }

        private string GetTableNameRuleByDate<T>(T entity)
        {
            SYS_Wetland_Rel rel = null;
            string name = entity.GetClassToDbName() + "_" + DateTime.Now.ToString("yyyyMMdd");
            string sql = "SELECT TOP 1 * FROM  [{0}].[dbo].[{1}] WHERE TableName='{2}'";
            sql = string.Format(sql, dbName, relTable, name);
            DbCommand cmd = reader.GetSqlStringCommand(sql);
            using (IDataReader dataReader = writer.ExecuteReader(cmd))
            {
                if (dataReader.Read())
                {
                    rel = new SYS_Wetland_Rel()
                    {
                        ID = Convert.ToString(dataReader["ID"]),
                        SourceTableName = dataReader["SourceTableName"].ToString(),
                        TableName = dataReader["TableName"].ToString(),
                        CreateTime = Convert.ToDateTime(dataReader["CreateTime"].ToString()),
                        LastPKValue = dataReader["LastPKValue"].ToString(),
                    };
                }
            }
            if (rel != null)
            {
                bool cte = CheckTableExist(rel.TableName);
                if (cte)
                {
                    return rel.TableName;
                }
                else
                {
                    string createTableSql = entity.GetClassToCreateSql(dbName, name);
                    bool isCreate = Create_Wetland_Table(createTableSql, name);
                    if (isCreate)
                    {
                        SYS_Wetland_Rel relInfo = new SYS_Wetland_Rel();
                        relInfo.ID = System.Guid.NewGuid().ToString();
                        relInfo.SourceTableName = entity.GetClassToDbName();
                        relInfo.TableName = name;
                        relInfo.CreateTime = DateTime.Now;
                        relInfo.LastPKValue = null;
                        SqlServerBuilderHelper.InsertSql<SYS_Wetland_Rel>(relInfo, string.Format("[{0}].[dbo].[{1}]", dbName, relTable));
                    }
                    return name;
                }
            }
            else
            {
                string createTableSql = entity.GetClassToCreateSql(dbName, name);
                bool isCreate = Create_Wetland_Table(createTableSql, name);
                if (isCreate)
                {
                    SYS_Wetland_Rel relInfo = new SYS_Wetland_Rel();
                    relInfo.ID = System.Guid.NewGuid().ToString();
                    relInfo.SourceTableName = entity.GetClassToDbName();
                    relInfo.TableName = name;
                    relInfo.CreateTime = DateTime.Now;
                    relInfo.LastPKValue = null;
                    string relSql = SqlServerBuilderHelper.InsertSql<SYS_Wetland_Rel>(relInfo, string.Format("[{0}].[dbo].[{1}]", dbName, relTable));
                    Insert_SYS_Wetland_Rel(relSql);
                }
                return name;
            }
        }

        private bool Create_Wetland_Table(string sql, string tableName)
        {
            bool exist = CheckTableExist(tableName);
            if (!exist)
            {
                DbCommand cmd = reader.GetSqlStringCommand(sql);
                reader.ExecuteScalar(cmd);
                return true;
            }
            return false;
        }

        private bool Insert_SYS_Wetland_Rel(string sql)
        {
            DbCommand cmd = writer.GetSqlStringCommand(sql);
            var result = writer.ExecuteNonQuery(cmd);
            return result > 0;
        }

        private int GetMaxWetlandID(string tableName)
        {
            //获取当前表最大 WetlandID
            int maxID = 0;
            string maxIDSql = "SELECT MAX(WetlandID) FROM [{0}].[dbo].[{1}] ";
            maxIDSql = string.Format(maxIDSql, dbName, tableName);
            DbCommand maxIDComd = reader.GetSqlStringCommand(maxIDSql);
            if (reader.ExecuteScalar(maxIDComd) != null)
            {
                int.TryParse(reader.ExecuteScalar(maxIDComd).ToString(), out maxID);
            }
            return maxID;
        }

        private string GetPrevTableName(string tableName)
        {
            string prevTable = null;
            string maxIDSql = " SELECT  top 1 TableName FROM  [{0}].[dbo].[{1}] WHERE ";
            maxIDSql += " CreateTime < (SELECT TOP 1 CreateTime FROM [{0}].[dbo].[{1}] WHERE TableName='{2}') ";
            maxIDSql += " ORDER BY CreateTime DESC ";
            maxIDSql = string.Format(maxIDSql, dbName, relTable, tableName);
            DbCommand tempCmd = reader.GetSqlStringCommand(maxIDSql);
            if (reader.ExecuteScalar(tempCmd) != null)
            {
                var tempTableName = reader.ExecuteScalar(tempCmd).ToString();
                if (!string.IsNullOrEmpty(tempTableName))
                {
                    prevTable = reader.ExecuteScalar(tempCmd).ToString();
                }
            }
            return prevTable;
        }

        private int GetMaxWetlandIDRule(string tableName)
        {
            int maxID = GetMaxWetlandID(tableName);
            if (maxID == 0)
            {
                string prevTableName = GetPrevTableName(tableName);
                if (!string.IsNullOrEmpty(prevTableName))
                {
                    maxID = GetMaxWetlandID(prevTableName);
                }
            }
            return maxID;
        }

        private List<SYS_Wetland_Rel> Get_Wetland_RelTable<T>(string orderBy = "CreateTime DESC")
        {
            List<SYS_Wetland_Rel> list = new List<SYS_Wetland_Rel>();
            string sourceTableName = DBAttributeHelper.GetClassToDbName<T>();
            string sql = " SELECT  *  FROM  [{0}].[dbo].[{1}] WHERE SourceTableName='{2}' ORDER BY {3} ";
            sql = string.Format(sql, dbName, relTable, sourceTableName, orderBy);
            DbCommand dbCommand = reader.GetSqlStringCommand(sql);
            using (IDataReader dataReader = reader.ExecuteReader(dbCommand))
            {
                while (dataReader.Read())
                {
                    list.Add(new SYS_Wetland_Rel()
                    {
                        ID = dataReader["ID"].ToString(),
                        CreateTime = Convert.ToDateTime(dataReader["CreateTime"].ToString()),
                        LastPKValue = dataReader["LastPKValue"].ToString(),
                        TableName = dataReader["TableName"].ToString(),
                    });
                }
            }
            return list;
        }
    }
}
