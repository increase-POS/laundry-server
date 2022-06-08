using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class subServicesModel
    {
        public int subServiceId { get; set; }
        public Nullable<int> agentSubId { get; set; }
        public Nullable<int> subId { get; set; }
        public Nullable<int> serviceId { get; set; }
        public string serviceType { get; set; }
        public int count { get; set; }
        public int remain { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }

        public bool canDelete { get; set; }
 



    }
}