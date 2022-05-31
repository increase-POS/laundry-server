using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class ItemTransferModel
    {
        public int itemsTransId { get; set; }
        public long quantity { get; set; }
        public Nullable<int> invoiceId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public string notes { get; set; }
        public decimal price { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public string itemSerial { get; set; }
        public Nullable<int> inventoryItemLocId { get; set; }
        public Nullable<int> offerId { get; set; }
        public string forAgents { get; set; }

        public decimal profit { get; set; }
        public decimal purchasePrice { get; set; }
        public string cause { get; set; }


      
        public Nullable<int> itemId { get; set; }
        public string itemName { get; set; }
       
        public Nullable<long> lockedQuantity { get; set; }
        public Nullable<long> availableQuantity { get; set; }
        public Nullable<long> newLocked { get; set; }
       
        public string invNumber { get; set; }
        //public Nullable<int> locationIdNew { get; set; }
        //public Nullable<int> locationIdOld { get; set; }
  
     
        public string unitName { get; set; }
        public Nullable<int> unitId { get; set; }
        public string barcode { get; set; }

        public string itemType { get; set; }

        public Nullable<decimal> itemTax { get; set; }
        public Nullable<decimal> itemUnitPrice { get; set; }
        public Nullable<decimal> offerValue { get; set; }
        public Nullable<decimal>  offerType { get; set; }
        public string offerCode { get; set; }
        public string offerName { get; set; }
        public Nullable<decimal> finalDiscount { get; set; }

    }
}