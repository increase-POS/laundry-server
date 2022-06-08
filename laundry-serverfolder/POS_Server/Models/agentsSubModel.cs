using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class agentsSubModel
    {
        public int agentSubId { get; set; }
        public Nullable<int> agentId { get; set; }
        public Nullable<int> subId { get; set; }
        public Nullable<byte> isLimited { get; set; }
        public Nullable<System.DateTime> endDate { get; set; }
        public byte isActive { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }

        public bool canDelete { get; set; }
 



    }
}