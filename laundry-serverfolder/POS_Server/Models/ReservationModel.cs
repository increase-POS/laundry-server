using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class ReservationModel
    {
        public long reservationId { get; set; }
        public string code { get; set; }
        public Nullable<int> customerId { get; set; }
        public string customerName { get; set; }

        public Nullable<int> branchId { get; set; }
        public Nullable<System.DateTime> reservationDate { get; set; }
        public Nullable<System.DateTime> reservationTime { get; set; }
        public Nullable<int> personsCount { get; set; }
        public string status { get; set; }
        public string notes { get; set; }
        public byte isActive { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<System.DateTime> endTime { get; set; }


        public string isExceed { get; set; }
        public IEnumerable<TableModel> tables { get; set; }


    }
}