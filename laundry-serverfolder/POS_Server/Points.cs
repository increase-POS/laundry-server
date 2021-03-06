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
    
    public partial class Points
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Points()
        {
            this.agents = new HashSet<agents>();
        }
    
        public int pointId { get; set; }
        public decimal Cash { get; set; }
        public int CashPoints { get; set; }
        public int invoiceCount { get; set; }
        public int invoiceCountPoints { get; set; }
        public decimal CashArchive { get; set; }
        public int CashPointsArchive { get; set; }
        public int invoiceCountArchive { get; set; }
        public int invoiceCountPoinstArchive { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public byte isActive { get; set; }
        public Nullable<int> agentId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<agents> agents { get; set; }
        public virtual agents agents1 { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}
