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
        private string SYS_Wetland_DB = ConfigurationManager.AppSettings["SYS_Wetland_DB"].ToString();
        private string SYS_Wetland_Rel = ConfigurationManager.AppSettings["SYS_Wetland_Rel"].ToString(); 
        private IDBContext DbContext;
        public MassiveContext(DBType type = DBType.SqlServer)
        {
            switch (type)
            {
                case DBType.SqlServer:
                    DbContext = new SqlServer.DBContext(SYS_Wetland_DB, SYS_Wetland_Rel); break;
                default:
                    DbContext = new SqlServer.DBContext(SYS_Wetland_DB, SYS_Wetland_Rel); break;
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

        public void Dispose()
        {
            
        }
    }
}
