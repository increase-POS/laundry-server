using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class PackageModel
    {
        public int packageId { get; set; }
        public Nullable<int> parentIUId { get; set; }
        public Nullable<int> childIUId { get; set; }
        public int quantity { get; set; }
        public byte isActive { get; set; }
        public string notes { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }

        public Nullable<System.DateTime> updateDate { get; set; }
       
        public bool canDelete { get; set; }


        // item parent
        public Nullable<int> pitemId { get; set; }
        public string pcode { get; set; }
        public string pitemName { get; set; }
      
        public string type { get; set; }
        public string image { get; set; }
      

        //units
        public Nullable<int> punitId { get; set; }
        public string punitName { get; set; }

        //item chiled
       
        public Nullable<int>  citemId { get; set; }
        public string ccode { get; set; }
        public string citemName { get; set; }

        public string ctype { get; set; }
        public string cimage { get; set; }


        //units
        public Nullable<int> cunitId { get; set; }
        public string cunitName { get; set; }
        public Nullable<decimal> avgPurchasePrice { get; set; }
    }
}