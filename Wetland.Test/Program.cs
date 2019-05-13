using Open.SchoolBase.ExpressionVisitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wetland.DBAttribute;
using Wetland.Models;
using Wetland.SqlServer;

namespace Wetland.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //YY_DATA_AUTO 为测试数据体,且继承 WetlandBaseModel  
                MassiveContext<YY_DATA_AUTO> context = new MassiveContext<YY_DATA_AUTO>();
                //测试写入，按天水平分库
                for (int i = 0; i < 100000; i++)
                {
                    YY_DATA_AUTO entity = new YY_DATA_AUTO();
                    entity.STCD = System.Guid.NewGuid().ToString();
                    entity.ItemID = "001";
                    entity.TM = DateTime.Now;
                    entity.DATAVALUE = 10;
                    entity.DOWNDATE = DateTime.Now;
                    entity.STTYPE = "";
                    entity.NFOINDEX = 1;
                    entity.CorrectionVALUE = 11;
                    context.Add(entity);
                    Console.WriteLine(entity.STCD + "写入成功!!...");
                }
                Expression<Func<YY_DATA_AUTO, bool>> where = p =>true;
                DBPageBase page = new DBPageBase();
                page.PageIndex = 1;
                page.PageSize = 100;
                var result = context.Get(where, page);
                result.ForEach(f => Console.WriteLine(f.WetlandID));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.ReadLine();
            }

            
        }


    }

}
