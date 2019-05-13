using System;
using System.Collections.Generic;
using System.Text;

namespace Wetland
{
    public class MassiveContent<T>
    {
        private string tableName = string.Empty;
        private string wetlandDBO = string.Empty;
        private string relTable = "SYS_Wetland_Rel";
        public MassiveContent(DBType type=DBType.SqlServer)
        {
              
        }

        /// <summary>
        /// 初始化 
        /// </summary>
        /// <param name="tableName">海量存储目标表</param>
        /// <param name="relTable">Wetland 将自动创建横向分库记录表</param>
        public void Init(string tableName, string relTable = "SYS_Wetland_Rel")
        {
            this.wetlandDBO = "dbo";
            this.tableName = tableName;
            this.relTable = relTable;
        }

        public void Add(T mdoel)
        {
            
        }

        public void Edit(T mdoel)
        {
            
        }
        public T Get(Dictionary<dynamic, dynamic> where)
        {
            return default(T);
        }

        public void Remove(T model)
        {

        }

        public void Remove(List<T> models)
        {

        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="where"></param>
        /// <param name="keyword"></param>
        /// <param name="order"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public List<T> Page( Dictionary<dynamic, dynamic> where,Dictionary<string,string[]> keyword,List<string> order,MassivePageBase page)
        {
            return null;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="where"></param>
        /// <param name="keyword"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public List<T> Page(Dictionary<dynamic, dynamic> where, Dictionary<string, List<string>> keyword, MassivePageBase page)
        {
            return null;
        }


        /// <summary>
        ///  创建横向分表记录表,及位置
        /// </summary>
        /// <param name="dbName"></param>
        private void Create_Wetland_RelTable(string dbName, string tableName = "sys_Wetland_Rel")
        {

            string tableSql = "CREATE TABLE {0}" +
            "( PRIMARY KEY," +
            "myName CHAR(50), myAddress CHAR(255), myBalance FLOAT)";

            var sql = "CREATE DATABASE {0}";
            sql = string.Format(sql, dbName);
        }

    }
}
