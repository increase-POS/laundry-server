using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class itransIUServicesModel
    {
        public int itransIUServiceId { get; set; }
        public Nullable<int> itemsTransId { get; set; }
        public Nullable<int> subServiceId { get; set; }
        public Nullable<int> itemUnitServiceId { get; set; }
        public Nullable<int> offerId { get; set; }
        public string offerType { get; set; }
        public decimal offerValue { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }



    }
}