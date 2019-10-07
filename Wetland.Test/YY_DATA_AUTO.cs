
namespace Wetland.Test
{
    using DBAttribute;
    using Models;
    using SqlServer;
    using System;
    using System.Collections.Generic;

    public  class YY_DATA_AUTO
    {
        public int? NFOINDEX { get; set; }

        public string STCD { get; set; }
        public string ItemID { get; set; }
        public System.DateTime TM { get; set; }

        public System.DateTime DOWNDATE { get; set; }

        [DBAttribute.DECIMAL(18,10)]
        public decimal DATAVALUE { get; set; }
        public decimal CorrectionVALUE { get; set; }
        public int DATATYPE { get; set; }
        public string STTYPE { get; set; }
    }
}
