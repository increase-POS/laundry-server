using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class ItemLocationModel
    {
        public int itemsLocId { get; set; }
        public Nullable<int> locationId { get; set; }
        public long quantity { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<System.DateTime> startDate { get; set; }
        public Nullable<System.DateTime> endDate { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public string notes { get; set; }
        public Nullable<int> invoiceId { get; set; }

        public int sequence { get; set; }
       
        public Nullable<int> sectionId { get; set; }
      
     
        public string itemName { get; set; }
        public string location { get; set; }
        public string section { get; set; }
        public string unitName { get; set; }
        public string itemType { get; set; }
        public Nullable<decimal> storeCost { get; set; }
        public Nullable<byte> isFreeZone { get; set; }

        public string invNumber { get; set; }

    }
}