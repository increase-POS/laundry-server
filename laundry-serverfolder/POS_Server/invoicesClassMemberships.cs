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
    
    public partial class invoicesClassMemberships
    {
        public int invClassMemberId { get; set; }
        public Nullable<int> membershipId { get; set; }
        public Nullable<int> invClassId { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
    
        public virtual invoicesClass invoicesClass { get; set; }
        public virtual memberships memberships { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}
