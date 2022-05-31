using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class UnitModel
    {
        public int unitId { get; set; }
        public string name { get; set; }
        public short isSmallest { get; set; }
        public Nullable<int> smallestId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> parentid { get; set; }
        public byte isActive { get; set; }
        public string notes { get; set; }
 
        public string smallestUnit { get; set; }

        public Boolean canDelete { get; set; }
    }
}