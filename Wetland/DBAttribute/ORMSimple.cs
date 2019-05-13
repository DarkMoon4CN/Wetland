using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wetland.DBAttribute;

namespace Wetland.DBAttribute
{
    public static class ORMSimple
    {
        public static string CSharpToSqlType(this string type)
        {
            string reval = string.Empty;
            switch (type.ToLower())
            {
                case "string":
                    reval = "nvarchar";
                    break;
                case "int":
                    reval = "int";
                    break;
                case "int32":
                    reval = "int";
                    break;
                case "datetime":
                    reval = "datetime";
                    break;
                case "decimal":
                    reval = "decimal";
                    break;
                case "bool":
                    reval = "bit";
                    break;
                default:
                    reval = "nvarchar(512)";
                    break;
            }
            return reval;
        }
    }
}
