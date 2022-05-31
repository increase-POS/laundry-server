using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class PosSettingModel
    {
        public int posSettingId { get; set; }
        public Nullable<int> posId { get; set; }
        public Nullable<int> saleInvPrinterId { get; set; }
        public Nullable<int> reportPrinterId { get; set; }
        public Nullable<int> saleInvPapersizeId { get; set; }
        public string posSerial { get; set; }
        public Nullable<int> docPapersizeId { get; set; }
        public string posDeviceCode { get; set; }
        public Nullable<int> posSerialId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }

       

        public Nullable<int> repprinterId { get; set; }
        public string repname { get; set; }
        public string repprintFor { get; set; }

        public Nullable<int> salprinterId { get; set; }
        public string salname { get; set; }
        public string salprintFor { get; set; }

        public Nullable<int> sizeId { get; set; }
        public string paperSize1 { get; set; }

        public string docPapersize { get; set; }
        public string saleSizeValue { get; set; }
        public string docSizeValue { get; set; }

        public Nullable<int> kitchenPrinterId { get; set; }
        public Nullable<int> kitchenPapersizeId { get; set; }
        public string kitchenPrinter { get; set; }
        public string kitchenPapersize { get; set; }
        public string kitchenprintFor { get; set; }
        public string kitchenSizeValue { get; set; }

    }
}