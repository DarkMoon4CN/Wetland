using Open.ExpressionVisitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                //创建实例方式 1
                //string r = "data source=.;uid=sa;pwd=123456;";
                //string w = "data source=.;uid=sa;pwd=123456;";
                //string SYS_Wetland_DB = "SYS_Wetland_DB";
                //string SYS_Wetland_Rel = "SYS_Wetland_Rel";
                //MassiveContext<YY_DATA_AUTO> context = new MassiveContext<YY_DATA_AUTO>(r, w, SYS_Wetland_DB, SYS_Wetland_Rel);



                /*创建实例方式 2
                    <connectionStrings>
                       <add name="SYS_Wetland_DBWrite" connectionString="data source=.;uid=sa;pwd=123456" providerName="System.Data.SqlClient" />
                       <add name="SYS_Wetland_DBReader" connectionString="data source=.;uid=sa;pwd=123456" providerName="System.Data.SqlClient" />
                    </connectionStrings>
                    <appSettings>
                       <add key="SYS_Wetland_DB" value="SYS_Wetland_DB"/>
                       <add key="SYS_Wetland_Rel" value="SYS_Wetland_Rel"/>
                    </appSettings>
                 */
                MassiveContext<YY_DATA_AUTO> context = new MassiveContext<YY_DATA_AUTO>();
                //for (int i = 0; i < 10; i++)
                //{
                //    YY_DATA_AUTO entity = new YY_DATA_AUTO();
                //    entity.STCD = System.Guid.NewGuid().ToString();
                //    entity.ItemID = "001";
                //    entity.TM = DateTime.Now;
                //    entity.DATAVALUE = 10;
                //    entity.DOWNDATE = DateTime.Now;
                //    entity.STTYPE = "";
                //    entity.NFOINDEX = 1;
                //    entity.CorrectionVALUE = 11;
                //    context.Add(entity);
                //    Console.WriteLine(entity.STCD + "写入成功!!...");
                //    entitys.Add(entity);
                //}
                

                //查询
                var startTime = DateTime.Parse("2019-7-29 00:00:00");
                var endTime = DateTime.Parse("2019-7-31 00:00:00");
                Expression<Func<YY_DATA_AUTO, bool>> where = p => p.TM > startTime && p.TM <endTime;
                DBPageBase page = new DBPageBase();
                page.PageIndex =1;
                page.PageSize =10000;


                Stopwatch sw = new Stopwatch();
                sw.Start();
                var result = context.Get(where, page);
                sw.Stop();
                Console.WriteLine("用时:" + sw.ElapsedMilliseconds);
                result.ForEach(f => Console.WriteLine(f.TM));
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
