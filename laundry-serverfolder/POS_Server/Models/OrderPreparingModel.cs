using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class OrderPreparingModel
    {
        public int orderPreparingId { get; set; }
        public string orderNum { get; set; }
        public Nullable<int> invoiceId { get; set; }
        public string notes { get; set; }
        public Nullable<decimal> preparingTime { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }


        // item
        public string itemName { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public int quantity { get; set; }
        //order
        public string status { get; set; }
        public int num { get; set; }
        public decimal remainingTime { get; set; }
        public string tables { get; set; }
        public string waiter { get; set; }
        public Nullable<System.DateTime> preparingStatusDate { get; set; }

        //invoice
        public string invNum { get; set; }
        public string invType { get; set; }
        public Nullable<int> shippingCompanyId { get; set; }


        public List<itemOrderPreparingModel> items { get; set; }
        public Nullable<int> branchId { get; set; }
        public string branchName { get; set; }
        public Nullable<System.DateTime> invDate { get; set; }
        public Nullable<System.TimeSpan> invTime { get; set; }
        //category
        public Nullable<int> categoryId { get; set; }
        public string categoryCode { get; set; }
        public string categoryName { get; set; }

    }
    public class itemOrderPreparingModel
    {
        public int itemOrderId { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public Nullable<int> orderPreparingId { get; set; }
        public Nullable<int> quantity { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }


        public string itemName { get; set; }
        public Nullable<int> itemId { get; set; }

        public decimal remainingTime { get; set; }
        public int sequence { get; set; }
        //
        public Nullable<int> categoryId { get; set; }
        public string categoryName { get; set; }

    }
}