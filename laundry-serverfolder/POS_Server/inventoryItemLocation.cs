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
    
    public partial class inventoryItemLocation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public inventoryItemLocation()
        {
            this.itemsTransfer = new HashSet<itemsTransfer>();
        }
    
        public int id { get; set; }
        public bool isDestroyed { get; set; }
        public int amount { get; set; }
        public int amountDestroyed { get; set; }
        public int realAmount { get; set; }
        public Nullable<int> itemLocationId { get; set; }
        public Nullable<int> inventoryId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public byte isActive { get; set; }
        public string notes { get; set; }
        public string cause { get; set; }
        public bool isFalls { get; set; }
        public string fallCause { get; set; }
    
        public virtual Inventory Inventory { get; set; }
        public virtual itemsLocations itemsLocations { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<itemsTransfer> itemsTransfer { get; set; }
    }
}
