using System;
using System.Collections.Generic;
using System.Text;

namespace Wetland
{
    public class MassivePageBase
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public int TotalPages
        {
            get
            {
                return (int)Math.Ceiling((double)(((float)this.TotalRows) / ((float)this.PageSize)));
            }
        }
        public int TotalRows { get; set; }
    }
}
