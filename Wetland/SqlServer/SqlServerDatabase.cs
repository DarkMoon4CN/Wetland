using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wetland.SqlServer
{
    public class SqlServerDatabase
    {
        public static Database Writer
        {
            get
            {
                try
                {
                    return DatabaseFactory.CreateDatabase("SYS_Wetland_DBWrite");
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public static Database Reader
        {
            get
            {
                try
                {
                    return DatabaseFactory.CreateDatabase("SYS_Wetland_DBReader");
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
