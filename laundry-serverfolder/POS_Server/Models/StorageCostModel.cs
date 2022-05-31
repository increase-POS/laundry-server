using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class StorageCostModel
    {
        public int storageCostId { get; set; }
        public string name { get; set; }
        public decimal cost { get; set; }
        public string notes { get; set; }
        public byte isActive { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }

       
        public bool canDelete { get; set; }
    }
}