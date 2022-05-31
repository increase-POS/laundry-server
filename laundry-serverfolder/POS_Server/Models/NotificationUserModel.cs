using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class NotificationUserModel
    {
        public int notUserId { get; set; }
        public Nullable<int> notId { get; set; }
        public Nullable<int> userId { get; set; }
        public bool isRead { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<int> posId { get; set; }


     
        public string title { get; set; }
        public string ncontent { get; set; }
        public string side { get; set; }
        public string msgType { get; set; }
        public string path { get; set; }
    }
}