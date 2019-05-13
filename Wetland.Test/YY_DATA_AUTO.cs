
namespace Wetland.Test
{
    using DBAttribute;
    using Models;
    using SqlServer;
    using System;
    using System.Collections.Generic;

    [DBAttribute.TABLE]
    public partial class YY_DATA_AUTO:WetlandBaseModel
    {
        [DBAttribute.LENGTH(256)]
        public string STCD { get; set; }
     
        [DBAttribute.LENGTH, DBAttribute.NOTNULL]
        public string ItemID { get; set; }

        [DBAttribute.NOTNULL]
        public System.DateTime TM { get; set; }
        public System.DateTime DOWNDATE { get; set; }
        public int NFOINDEX { get; set; }
        public decimal DATAVALUE { get; set; }
        public decimal CorrectionVALUE { get; set; }
        public int DATATYPE { get; set; }
        public string STTYPE { get; set; }
    }
}
