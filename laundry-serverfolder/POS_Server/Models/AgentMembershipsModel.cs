using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class AgentMembershipsModel
    {
        public int agentMembershipsId { get; set; }
        public Nullable<int> membershipId { get; set; }
        public Nullable<int> agentId { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public byte isActive { get; set; }



        public decimal Amount { get; set; }


        public bool canDelete { get; set; }

        public MembershipsModel memberShip {get; set;}
    }
}