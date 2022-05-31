using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class BranchesUsersModel
    {
        public int branchsUsersId { get; set; }
        public Nullable<int> branchId { get; set; }
        public Nullable<int> userId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }




        // branch
        public int bbranchId { get; set; }
        public string bcode { get; set; }
        public string bname { get; set; }
        public string baddress { get; set; }
        public string bemail { get; set; }
        public string bphone { get; set; }
        public string bmobile { get; set; }
        public Nullable<System.DateTime> bcreateDate { get; set; }
        public Nullable<System.DateTime> bupdateDate { get; set; }
        public Nullable<int> bcreateUserId { get; set; }
        public Nullable<int> bupdateUserId { get; set; }
        public string bnotes { get; set; }
        public Nullable<int> bparentId { get; set; }
        public byte bisActive { get; set; }
        public string btype { get; set; }

        // user
        public int uuserId { get; set; }
        public string uusername { get; set; }
        public string upassword { get; set; }
        public string uname { get; set; }
        public string ulastname { get; set; }
        public string ujob { get; set; }
        public string uworkHours { get; set; }
        public DateTime? ucreateDate { get; set; }
        public DateTime? uupdateDate { get; set; }
        public int? ucreateUserId { get; set; }
        public int? uupdateUserId { get; set; }
        public string uphone { get; set; }
        public string umobile { get; set; }
        public string uemail { get; set; }
        public string unotes { get; set; }
        public string uaddress { get; set; }
        public byte uisActive { get; set; }
        public byte uisOnline { get; set; }
        public Boolean ucanDelete { get; set; }
        public string uimage { get; set; }

    }
    }