using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wetland.Models
{
    public class WetlandPrimaryKeyModel
    {
        [DBAttribute.KEY]
        public int WetlandID { get; set; }
    }
}
