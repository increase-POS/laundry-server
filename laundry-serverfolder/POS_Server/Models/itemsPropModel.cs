using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class itemsPropModel
    {
        public int itemPropId { get; set; }
        public Nullable<int> propertyItemId { get; set; }
        public Nullable<int> itemId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }

        public Nullable<int> updateUserId { get; set; }
 
        public Nullable<int> isActive { get; set; }
        public string propValue { get; set; }
        public string propName { get; set; }
   
    }
}