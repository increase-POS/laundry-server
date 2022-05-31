using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class GroupObjectModel
    {
        public int id { get; set; }
        public Nullable<int> groupId { get; set; }
        public Nullable<int> objectId { get; set; }
        public string notes { get; set; }
        public byte addOb { get; set; }
        public byte updateOb { get; set; }
        public byte deleteOb { get; set; }
        public byte showOb { get; set; }
        public byte reportOb { get; set; }
        public byte levelOb { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public int isActive { get; set; }


      
        public string objectName { get; set; }
        public string desc { get; set; }
      
        public Boolean canDelete { get; set; }
        public Nullable<int> parentObjectId { get; set; }
        public string objectType { get; set; }
        public string parentObjectName { get; set; }

        


    }
}