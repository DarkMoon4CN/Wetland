using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wetland.DBAttribute
{
    public static class DBAttributeHelper
    {
        public static string GetDes(PropertyInfo field)
        {
            string result = string.Empty;
            var dbKey = (KEY)Attribute.GetCustomAttribute(field, typeof(KEY));
            if (dbKey != null) result = dbKey.Description;
            return result;
        }
        public static string GetClassToDbName<T>(this T t)
        {
            Type type = typeof(T);
            string className = string.Empty;
            object[] attributes = type.GetCustomAttributes(false);
            foreach (var attr in attributes)
            {
                if (attr is DBAttribute.TABLE)
                {
                    TABLE tableAttribute = attr as TABLE;
                    className = tableAttribute.Name;
                }
            }
            if (string.IsNullOrEmpty(className))
            {
                className = type.Name;
            }
            return className;
        }


        public static string GetClassToDbName<T>()
        {
            Type type = typeof(T);
            string className = string.Empty;
            object[] attributes = type.GetCustomAttributes(false);
            foreach (var attr in attributes)
            {
                if (attr is DBAttribute.TABLE)
                {
                    TABLE tableAttribute = attr as TABLE;
                    className = tableAttribute.Name;
                }
            }
            if (string.IsNullOrEmpty(className))
            {
                className = type.Name;
            }
            return className;
        }


        public static string GetClassToCreateSql<T>(this T t,string dbName,string tableName)
        {
            string createStr =string.Empty;
            string bodyStr = string.Empty;
            if (string.IsNullOrEmpty(tableName))
            {
                tableName = t.GetClassToDbName();
            }
            createStr = "CREATE TABLE [{0}].[dbo].[{1}]({2});";

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            foreach (var item in properties)
            {
                DBAttributeModel model = new DBAttributeModel();
                model.SqlPart = new List<SqlPart>();
                model.Name = item.Name;
                model.SqlPart.Add(new SqlPart {Key=1,Value= " [" + item.Name + "] " });
                model.SqlPart.Add(new SqlPart { Key = 2, Value = " " + item.PropertyType.Name.CSharpToSqlType() });
                object[] attributes = item.GetCustomAttributes(false);
                foreach (var attr in attributes)
                {
                    if (attr is DBAttribute.LENGTH)
                    {
                        LENGTH temp = attr as LENGTH;
                        model.SqlPart.Add(new SqlPart { Key = 3, Value = "(" + temp.Value + ") " });
                    }

                    if (attr is DBAttribute.DECIMAL)
                    {
                        DECIMAL temp = attr as DECIMAL;
                        model.SqlPart.Add(new SqlPart { Key = 3, Value = "(" + temp.Precision + ","+temp.Places+") " });
                    }

                    if (attr is DBAttribute.IDENTITY)
                    {
                        IDENTITY temp = attr as IDENTITY;
                        model.SqlPart.Add(new SqlPart { Key = 4, Value = " IDENTITY(" + temp.Seed + "," + temp.Increment + ") " });
                    }

                    if (attr is DBAttribute.KEY)
                    {
                        model.SqlPart.Add(new SqlPart { Key = 5, Value = " PRIMARY KEY " });
                        model.SqlPart.Add(new SqlPart { Key = 6, Value = " NOT NULL," });
                    }

                    if (attr is DBAttribute.NOTNULL)
                    {
                        model.SqlPart.Add(new SqlPart { Key = 6, Value = " NOT NULL," });
                    }
                }
                var exist=model.SqlPart.Where(p => p.Key == 3).FirstOrDefault();
                //类型大小设置
                if (item.PropertyType.Name.ToLower() == "string" && exist == null)
                {
                    model.SqlPart.Add(new SqlPart { Key = 3, Value = "(" + 50 + ") " });
                }
                if (item.PropertyType.Name.ToLower() == "decimal" && exist == null)
                {
                    model.SqlPart.Add(new SqlPart { Key = 3, Value = "(" + 18 + "," + 0 + ") " });
                }
                //是否为空设置
                exist = model.SqlPart.Where(p => p.Key ==6).FirstOrDefault();
                if (exist==null)
                {
                    model.SqlPart.Add(new SqlPart { Key = 6, Value = " NULL," });
                }
                var list=model.SqlPart.OrderBy(o=>o.Key).ToList();
                string str = string.Empty;
                foreach (var sp in list)
                {
                    str += sp.Value;
                }
                bodyStr += str;
            }
            bodyStr= bodyStr.TrimEnd(',');
            createStr = string.Format(createStr,dbName,tableName,bodyStr);
            return createStr;
        }
    }
}
