using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class UsersLogsModel
    {
        public int logId { get; set; }
        public Nullable<System.DateTime> sInDate { get; set; }
        public Nullable<System.DateTime> sOutDate { get; set; }
        public Nullable<int> posId { get; set; }
        public Nullable<int> userId { get; set; }

            public bool canDelete { get; set; }
    }
}