﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class TagsModel
    {
        public int tagId { get; set; }
        public string tagName { get; set; }
        public Nullable<int> categoryId { get; set; }
        public string notes { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public byte isActive { get; set; }


        public bool canDelete { get; set; }

    }
}