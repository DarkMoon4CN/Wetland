using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wetland.DBAttribute
{
    internal class DBAttributeModel
    {
        public string Name { get; set; }

        public List<SqlPart> SqlPart { get; set; }
    }

    public class SqlPart
    {
        public int Key { get; set; }

        public string Value { get; set; }
    }
}
