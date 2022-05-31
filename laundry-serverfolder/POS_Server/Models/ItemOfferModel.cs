using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class ItemOfferModel
    {
        public int ioId { get; set; }
        public Nullable<int> iuId { get; set; }
        public Nullable<int> offerId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public int quantity { get; set; }
        public int used { get; set; }

 
       
        public string offerName { get; set; }
        public string unitName { get; set; }
        public string code { get; set; }
        public Nullable<int> itemId { get; set; }
        public Nullable<int>  unitId { get; set; }
    
        public string itemName { get; set; }



    }
}