﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class ObjectsModel
    {
        public int objectId { get; set; }
        public string name { get; set; }
        public Nullable<int> parentObjectId { get; set; }
        public string objectType { get; set; }
        public string translate { get; set; }

      
        public string note { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public byte isActive { get; set; }
        public Boolean canDelete { get; set; }

        public string icon { get; set; }
        public string translateHint { get; set; }


    }
}