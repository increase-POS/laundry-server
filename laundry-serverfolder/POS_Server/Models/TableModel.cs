using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class TableModel
    {
        public int tableId { get; set; }
        public string name { get; set; }
        public Nullable<int> sectionId { get; set; }
        public Nullable<int> branchId { get; set; }
        public string notes { get; set; }
        public string status { get; set; }
        public int personsCount { get; set; }
        public byte isActive { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }


        public Boolean canDelete { get; set; }
        public string sectionName { get; set; }
        public string branchName { get; set; }
    }
}