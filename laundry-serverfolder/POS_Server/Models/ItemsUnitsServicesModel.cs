using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class ItemsUnitsServicesModel
    {
        public int itemUnitServiceId { get; set; }
        public decimal normalPrice { get; set; }
        public decimal instantPrice { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<int> serviceId { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public decimal cost { get; set; }



    }
}