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
    
    public partial class itemsProp
    {
        public int itemPropId { get; set; }
        public Nullable<int> propertyItemId { get; set; }
        public Nullable<int> itemId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
    
        public virtual items items { get; set; }
        public virtual propertiesItems propertiesItems { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}
