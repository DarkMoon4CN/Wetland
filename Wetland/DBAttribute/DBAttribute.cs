using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wetland.DBAttribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TABLE : Attribute
    {
         public string Name { get; set; }

        public TABLE(string name)
        {
            this.Name = name;
        }
        public TABLE()
        {
           
        }
    }

    /// <summary>
    /// 主键
    /// </summary>
    public class KEY : Attribute
    {
        public string Description { get; set; }
        public KEY()
        {
        }
        public KEY(string description)
        {
            this.Description = description;
        }
    }

    /// <summary>
    ///  Seed=1,Increment=1
    /// </summary>
    public class IDENTITY : Attribute
    {
        /// <summary>
        /// 增量
        /// </summary>
        public int Increment { get; set; }

        /// <summary>
        /// 种子
        /// </summary>
        public int Seed { get; set; }

        /// <summary>
        /// 自增标记
        /// </summary>
        /// <param name="seed">种子</param>
        /// <param name="increment">增量</param>
        public IDENTITY(int seed=1,int increment=1)
        {
            this.Seed = seed;
            this.Increment = increment;
        }
    }

    /// <summary>
    ///  不可为空
    /// </summary>
    public class NOTNULL : Attribute
    {
         
    }

    /// <summary>
    /// 长度
    /// </summary>
    public class LENGTH: Attribute
    {
        public int Value { get; set; }
        public LENGTH(int value=50)
        {
            this.Value = value;
        }
    }

    /// <summary>
    /// 时间
    /// </summary>
    public class DATATIME{
        public DATATIME()
        {
           
        }
    }

    /// <summary>
    /// 货币
    /// </summary>
    public class DECIMAL
    {
        /// <summary>
        /// 精度
        /// </summary>
        public int Precision { get; set; }

        /// <summary>
        /// 小数位
        /// </summary>
        public int Places { get; set; }
        public DECIMAL(int precision=18,int places = 0)
        {
            this.Precision = precision;
            this.Places = places;
        }
    }

}
