//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace POS_Server
{
    using System;
    using System.Collections.Generic;
    
    public partial class sysEmails
    {
        public int emailId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public int port { get; set; }
        public bool isSSL { get; set; }
        public string smtpClient { get; set; }
        public string side { get; set; }
        public string notes { get; set; }
        public Nullable<int> branchId { get; set; }
        public byte isActive { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public bool isMajor { get; set; }
    
        public virtual branches branches { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}
