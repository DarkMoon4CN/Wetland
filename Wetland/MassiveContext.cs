using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq.Expressions;
using System.Text;
using Wetland.Models;

namespace Wetland
{
    public class MassiveContext<T> :IDisposable
    {
        private string SYS_Wetland_DB =string.Empty;
        private string SYS_Wetland_Rel = string.Empty; 
        private IDBContext DbContext;
        public MassiveContext(DBType type = DBType.SqlServer)
        {
            SYS_Wetland_DB = ConfigurationManager.AppSettings["SYS_Wetland_DB"].ToString();
            SYS_Wetland_Rel = ConfigurationManager.AppSettings["SYS_Wetland_Rel"].ToString();
            switch (type)
            {
                case DBType.SqlServer:
                    DbContext = new SqlServer.DBContext(SYS_Wetland_DB, SYS_Wetland_Rel); break;
                default:
                    DbContext = new SqlServer.DBContext(SYS_Wetland_DB, SYS_Wetland_Rel); break;
            }
        }

        public MassiveContext(string writerConn,string readerConn, DBType type = DBType.SqlServer)
        {
            SYS_Wetland_DB = ConfigurationManager.AppSettings["SYS_Wetland_DB"].ToString();
            SYS_Wetland_Rel = ConfigurationManager.AppSettings["SYS_Wetland_Rel"].ToString();
            switch (type)
            {
                case DBType.SqlServer:
                    DbContext = new SqlServer.DBContext(SYS_Wetland_DB, SYS_Wetland_Rel,writerConn,readerConn); break;
                default:
                    DbContext = new SqlServer.DBContext(SYS_Wetland_DB, SYS_Wetland_Rel,writerConn,readerConn); break;
            }
        }

        /// <summary>
        /// 初始化 构造时，勿指定数据库
        /// </summary>
        /// <param name="writerConn">写入connection对象</param>
        /// <param name="readerConn">读取connection对象</param>
        /// <param name="sys_wetland_db">水平分库  默认存放数据库名称</param>
        /// <param name="sys_wetland_rel">水平分库 默认关系表名称</param>
        /// <param name="type"></param>
        public MassiveContext(string writerConn, string readerConn,string sys_wetland_db, string sys_wetland_rel, DBType type = DBType.SqlServer)
        {
            SYS_Wetland_DB = sys_wetland_db;
            SYS_Wetland_Rel = sys_wetland_rel;
            switch (type)
            {
                case DBType.SqlServer:
                    DbContext = new SqlServer.DBContext(SYS_Wetland_DB, SYS_Wetland_Rel, writerConn, readerConn); break;
                default:
                    DbContext = new SqlServer.DBContext(SYS_Wetland_DB, SYS_Wetland_Rel, writerConn, readerConn); break;
            }
        }

        public void Add(T mdoel)
        {
            DbContext.Add<T>(mdoel);
        }

        public List<T> Get(Expression<Func<T, bool>> where, DBPageBase page)
        {
            return DbContext.Get<T>(where, page);        
        }

        public bool AddList(List<T> models)
        {
            return DbContext.AddList(models);
        }

        public void Dispose()
        {
            
        }
    }
}
