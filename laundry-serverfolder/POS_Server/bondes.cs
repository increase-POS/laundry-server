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
    
    public partial class bondes
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public bondes()
        {
            this.cashTransfer1 = new HashSet<cashTransfer>();
        }
    
        public int bondId { get; set; }
        public string number { get; set; }
        public decimal amount { get; set; }
        public Nullable<System.DateTime> deserveDate { get; set; }
        public string type { get; set; }
        public byte isRecieved { get; set; }
        public string notes { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public byte isActive { get; set; }
        public Nullable<int> cashTransId { get; set; }
    
        public virtual cashTransfer cashTransfer { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cashTransfer> cashTransfer1 { get; set; }
    }
}