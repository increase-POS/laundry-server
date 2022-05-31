using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class MedalAgentModel
    {
        public int id { get; set; }
        public Nullable<int> medalId { get; set; }
        public Nullable<int> agentId { get; set; }
        public string notes { get; set; }
        public byte isActive { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }

      
        public Nullable<int> offerId { get; set; }
        public Nullable<int> couponId { get; set; }
      
        public string medalName { get; set; }
        public string agentName { get; set; }
        public string offerName { get; set; }
        public string couponName { get; set; }
        public string createUserName { get; set; }
    }
}