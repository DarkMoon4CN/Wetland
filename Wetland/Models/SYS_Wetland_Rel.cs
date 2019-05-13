using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wetland.Models
{
    public class SYS_Wetland_Rel
    {
        public string ID { get; set; }

        public string SourceTableName { get; set; }
        public string TableName { get; set; }

        public DateTime CreateTime { get; set; }

        public string LastPKValue { get; set; }
    }
}
