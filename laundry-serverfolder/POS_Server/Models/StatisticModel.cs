using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{

    public class OrderPreparingSTSModel
    {
        public int orderPreparingId { get; set; }
        public string orderNum { get; set; }
        public Nullable<System.DateTime> orderTime { get; set; }
        public Nullable<int> invoiceId { get; set; }
        public string notes { get; set; }
        public Nullable<decimal> preparingTime { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }


        // item
        public string itemName { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public int quantity { get; set; }
        //order
        public string status { get; set; }
        public int num { get; set; }
        public decimal remainingTime { get; set; }
        public string tables { get; set; }
        public string waiter { get; set; }
        //invoice

        public string invType { get; set; }
        public Nullable<int> shippingCompanyId { get; set; }
        public string branchName { get; set; }
        public Nullable<int> branchId { get; set; }

        public List<itemOrderPreparingModel> items { get; set; }
        //
        public Nullable<int> categoryId { get; set; }
        public string categoryName { get; set; }
        public Nullable<decimal> realDuration { get; set; }
        public string invNumber { get; set; }
        public string invBarcode { get; set; }

        public Nullable<int> tagId { get; set; }
        public string tagName { get; set; }
        public Nullable<System.DateTime> listedDate { get; set; }

        public string shipUserName { get; set; }
        public string shipUserLastName { get; set; }
        public string shippingCompanyName { get; set; }
        public Nullable<int> shipUserId { get; set; }
        //   agentId
        public Nullable<int> agentId { get; set; }
        public string agentName { get; set; }
        public string agentCompany { get; set; }
        public string agentType { get; set; }
        public string agentCode { get; set; }
        public List<orderPreparingStatusModel> orderStatusList { get; set; }
        public decimal orderDuration { get; set; }




    }
    public class orderPreparingStatusModel
    {
        public int orderStatusId { get; set; }
        public Nullable<int> orderPreparingId { get; set; }
        public string status { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }

        public string notes { get; set; }

    }

    public class ItemUnitInvoiceProfitModel
    {

        ///////////////
        public Nullable<decimal> avgPurchasePrice { get; set; }
        public string ITitemName { get; set; }
        public string ITunitName { get; set; }
        public int ITitemsTransId { get; set; }
        public Nullable<int> ITitemUnitId { get; set; }

        public Nullable<int> ITitemId { get; set; }
        public Nullable<int> ITunitId { get; set; }
        public Nullable<long> ITquantity { get; set; }

        public Nullable<System.DateTime> ITupdateDate { get; set; }
        //  public Nullable<int> IT.createUserId { get; set; } 
        public Nullable<int> ITupdateUserId { get; set; }

        public Nullable<decimal> ITprice { get; set; }
        public string ITbarcode { get; set; }

        public string ITUpdateuserNam { get; set; }
        public string ITUpdateuserLNam { get; set; }
        public string ITUpdateuserAccNam { get; set; }
        public int invoiceId { get; set; }
        public string invNumber { get; set; }
        public string invBarcode { get; set; }
        public Nullable<int> agentId { get; set; }
        public Nullable<int> posId { get; set; }
        public string invType { get; set; }
        public Nullable<decimal> total { get; set; }
        public Nullable<decimal> totalNet { get; set; }

        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<System.DateTime> invDate { get; set; }
        
        public Nullable<int> updateUserId { get; set; }
        public Nullable<int> branchId { get; set; }
        public Nullable<decimal> discountValue { get; set; }
        public string discountType { get; set; }
        public Nullable<decimal> tax { get; set; }
        // public string name { get; set; }
        //  isApproved { get; set; }


        public Nullable<int> branchCreatorId { get; set; }
        public string branchCreatorName { get; set; }


        public string posName { get; set; }
        public string posCode { get; set; }
        public string agentName { get; set; }
        public string agentCode { get; set; }
        public string agentType { get; set; }

        public string uuserName { get; set; }
        public string uuserLast { get; set; }
        public string uUserAccName { get; set; }
        public string agentCompany { get; set; }
        public Nullable<decimal> subTotal { get; set; }
        public decimal purchasePrice { get; set; }
        public decimal totalwithTax { get; set; }
        public decimal subTotalNet { get; set; } // with invoice discount 
        public decimal itemunitProfit { get; set; }
        public decimal invoiceProfit { get; set; }
        public decimal shippingCost { get; set; }
        public decimal realShippingCost { get; set; }
        public decimal shippingProfit { get; set; }
        public decimal totalNoShip { get; set; }
        public decimal totalNetNoShip { get; set; }
        public string itemType { get; set; }
        //  public Nullable<decimal> ITdiscountpercent { get; set; }
    }

    public class ItemTransferInvoiceTax
    {// new properties
        public Nullable<System.DateTime> updateDate { get; set; }
       
        
        public string agentCompany { get; set; }

        // ItemTransfer
        public int ITitemsTransId { get; set; }
        public Nullable<int> ITitemUnitId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<int> ITitemId { get; set; }
        public Nullable<int> ITunitId { get; set; }

        public Nullable<decimal> ITprice { get; set; }

        public string ITnotes { get; set; }

        public string ITbarcode { get; set; }

        //invoice
        public int invoiceId { get; set; }

        public Nullable<int> agentId { get; set; }

        public string invType { get; set; }
        public string discountType { get; set; }

        public Nullable<decimal> discountValue { get; set; }
        public Nullable<decimal> total { get; set; }
        public Nullable<decimal> totalNet { get; set; }
        public Nullable<decimal> paid { get; set; }
        public Nullable<decimal> deserved { get; set; }
        public Nullable<System.DateTime> deservedDate { get; set; }
        public Nullable<System.DateTime> invDate { get; set; }

        public Nullable<int> IupdateUserId { get; set; }

        public string invCase { get; set; }

        public string Inotes { get; set; }
        public string vendorInvNum { get; set; }

        public string branchName { get; set; }
        public string posName { get; set; }
        public Nullable<System.DateTime> vendorInvDate { get; set; }
        public Nullable<int> branchId { get; set; }


        public Nullable<int> taxtype { get; set; }
        public Nullable<int> posId { get; set; }

        public string ITtype { get; set; }

        public string branchType { get; set; }

        public string posCode { get; set; }
        public string agentName { get; set; }

        public string agentType { get; set; }
        public string agentCode { get; set; }

        public string uuserName { get; set; }
        public string uuserLast { get; set; }
        public string uUserAccName { get; set; }
        public Nullable<decimal> itemUnitPrice { get; set; }


        public Nullable<decimal> subTotalTax { get; set; }


        public Nullable<decimal> OneitemUnitTax { get; set; }


        public Nullable<decimal> OneItemOfferVal { get; set; }
        public Nullable<decimal> OneItemPriceNoTax { get; set; }

        public Nullable<decimal> OneItemPricewithTax { get; set; }

        public Nullable<decimal> itemsTaxvalue { get; set; }


        //invoice

        public Nullable<decimal> tax { get; set; }//نسبة الضريبة
        public Nullable<decimal> totalwithTax { get; set; }//قيمة الفاتورة النهائية Totalnet
        public Nullable<decimal> totalNoTax { get; set; }//قيمة الفاتورة قبل الضريبة total
        public Nullable<decimal> invTaxVal { get; set; }//قيمة ضريبة الفاتورة TAX
        public Nullable<int> itemsRowsCount { get; set; }//عدداسطر الفاتورة

        //item
        public string ITitemName { get; set; }//اسم العنصر
        public string ITunitName { get; set; }//وحدة العنصر

        public Nullable<long> ITquantity { get; set; }//الكمية
        public Nullable<decimal> subTotalNotax { get; set; }//سعر العناصر قبل الضريبة Price
        public Nullable<decimal> itemUnitTaxwithQTY { get; set; }//قيم الضريبة للعناصر
        public string invNumber { get; set; }//رقم الفاتورة//item
        public string invBarcode { get; set; }//barcode الفاتورة//item
        public Nullable<System.DateTime> IupdateDate { get; set; }//تاريخ الفاتورة//item

        public Nullable<decimal> ItemTaxes { get; set; }//نسبة ضريبة العنصر

        //public string invNumber { get; set; }//رقم الفاتورة
        //public Nullable<System.DateTime> IupdateDate { get; set; }//تاريخ الفاتورة
        //public Nullable<decimal> tax { get; set; }//نسبة الضريبة
        //public Nullable<decimal> totalwithTax { get; set; }//قيمة الفاتورة النهائية Totalnet
        //public Nullable<decimal> totalNoTax { get; set; }//قيمة الفاتورة قبل الضريبة total
        //public Nullable<decimal> invTaxVal { get; set; }//قيمة ضريبة الفاتورة TAX
        //public Nullable<int> itemsRowsCount { get; set; }//عدداسطر الفاتورة
        // public Nullable<decimal> totalNet { get; set; }

    }

    public class OpenClosOperatinModel
    {
        public int cashTransId { get; set; }
        public string transType { get; set; }
        public Nullable<int> posId { get; set; }
        public Nullable<int> userId { get; set; }
        public Nullable<int> agentId { get; set; }
        public Nullable<int> invId { get; set; }
        public string transNum { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<decimal> cash { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<int> createUserId { get; set; }
        public string notes { get; set; }
        public Nullable<int> posIdCreator { get; set; }
        public Nullable<byte> isConfirm { get; set; }
        public Nullable<int> cashTransIdSource { get; set; }
        public string side { get; set; }
        public string opSideNum { get; set; }
        public string docName { get; set; }
        public string docNum { get; set; }
        public string docImage { get; set; }
        public Nullable<int> bankId { get; set; }
        public string bankName { get; set; }
        public string agentName { get; set; }
        public string usersName { get; set; }
        public string usersLName { get; set; }
        public string posName { get; set; }
        public string posCreatorName { get; set; }
        public Nullable<byte> isConfirm2 { get; set; }
        public int cashTrans2Id { get; set; }
        public Nullable<int> pos2Id { get; set; }

        public string pos2Name { get; set; }
        public string processType { get; set; }
        public Nullable<int> cardId { get; set; }
        public Nullable<int> bondId { get; set; }
        public string createUserName { get; set; }
        public string updateUserName { get; set; }
        public string updateUserJob { get; set; }
        public string updateUserAcc { get; set; }
        public string createUserJob { get; set; }
        public string createUserLName { get; set; }
        public string updateUserLName { get; set; }
        public string cardName { get; set; }
        public Nullable<System.DateTime> bondDeserveDate { get; set; }
        public Nullable<byte> bondIsRecieved { get; set; }
        public string agentCompany { get; set; }
        public Nullable<int> shippingCompanyId { get; set; }
        public string shippingCompanyName { get; set; }
        public string userAcc { get; set; }

        public Nullable<int> branchCreatorId { get; set; }
        public string branchCreatorname { get; set; }
        public Nullable<int> branchId { get; set; }
        public string branchName { get; set; }
        public Nullable<int> branch2Id { get; set; }
        public string branch2Name { get; set; }




    }
    public class POSOpenCloseModel
    {
        public int cashTransId { get; set; }
        public string transType { get; set; }
        public Nullable<int> posId { get; set; }

        public string transNum { get; set; }

        public Nullable<decimal> cash { get; set; }

        public string notes { get; set; }

        public Nullable<byte> isConfirm { get; set; }
        public Nullable<int> cashTransIdSource { get; set; }
        public string side { get; set; }

        public string posName { get; set; }



        public string processType { get; set; }


        public Nullable<int> branchId { get; set; }
        public string branchName { get; set; }

        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<System.DateTime> openDate { get; set; }
        public Nullable<decimal> openCash { get; set; }
        public Nullable<int> openCashTransId { get; set; }



    }

    public class ItemTransferInvoiceSTS
    {// new properties

        public Nullable<int> membershipId { get; set; }
        public string membershipsName { get; set; }
        public string membershipsCode { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }

        public string agentCompany { get; set; }

        // ItemTransfer
        public int ITitemsTransId { get; set; }
        public Nullable<int> ITitemUnitId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<int> ITitemId { get; set; }
        public Nullable<int> ITunitId { get; set; }

        public Nullable<decimal> ITprice { get; set; }

        public string ITnotes { get; set; }

        public string ITbarcode { get; set; }

        //invoice
        public int invoiceId { get; set; }

        public Nullable<int> agentId { get; set; }

        public string invType { get; set; }
        public string discountType { get; set; }

        public Nullable<decimal> discountValue { get; set; }
        public Nullable<decimal> total { get; set; }
        public Nullable<decimal> totalNet { get; set; }
        public Nullable<decimal> paid { get; set; }
        public Nullable<decimal> deserved { get; set; }
        public Nullable<System.DateTime> deservedDate { get; set; }
        public Nullable<System.DateTime> invDate { get; set; }

        public Nullable<int> IupdateUserId { get; set; }

        public string invCase { get; set; }

        public string Inotes { get; set; }
        public string vendorInvNum { get; set; }

        public string branchName { get; set; }
        public string posName { get; set; }
        public Nullable<System.DateTime> vendorInvDate { get; set; }
        public Nullable<int> branchId { get; set; }


        public Nullable<int> taxtype { get; set; }
        public Nullable<int> posId { get; set; }

        public string ITtype { get; set; }

        public string branchType { get; set; }

        public string posCode { get; set; }
        public string agentName { get; set; }

        public string agentType { get; set; }
        public string agentCode { get; set; }

        public string uuserName { get; set; }
        public string uuserLast { get; set; }
        public string uUserAccName { get; set; }
        public Nullable<decimal> itemUnitPrice { get; set; }


        public Nullable<decimal> subTotalTax { get; set; }


        public Nullable<decimal> OneitemUnitTax { get; set; }


        public Nullable<decimal> OneItemOfferVal { get; set; }
        public Nullable<decimal> OneItemPriceNoTax { get; set; }

        public Nullable<decimal> OneItemPricewithTax { get; set; }

        public Nullable<decimal> itemsTaxvalue { get; set; }


        //invoice

        public Nullable<decimal> tax { get; set; }//نسبة الضريبة
        public Nullable<decimal> totalwithTax { get; set; }//قيمة الفاتورة النهائية Totalnet
        public Nullable<decimal> totalNoTax { get; set; }//قيمة الفاتورة قبل الضريبة total
        public Nullable<decimal> invTaxVal { get; set; }//قيمة ضريبة الفاتورة TAX
        public Nullable<int> itemsRowsCount { get; set; }//عدداسطر الفاتورة

        //item
        public string ITitemName { get; set; }//اسم العنصر
        public string ITunitName { get; set; }//وحدة العنصر

        public Nullable<long> ITquantity { get; set; }//الكمية
        public Nullable<decimal> subTotalNotax { get; set; }//سعر العناصر قبل الضريبة Price
        public Nullable<decimal> itemUnitTaxwithQTY { get; set; }//قيم الضريبة للعناصر
        public string invNumber { get; set; }//رقم الفاتورة//item
        public Nullable<System.DateTime> IupdateDate { get; set; }//تاريخ الفاتورة//item

        public Nullable<decimal> ItemTaxes { get; set; }//نسبة ضريبة العنصر

        public List<invoiceClassDiscount> invoiceClassDiscountList { get; set; }

    }

    public class SalesMembership
    {

        //invoice
        public int invoiceId { get; set; }
        public string invNumber { get; set; }

        public string invBarcode { get; set; }
        public string invType { get; set; }
        public string discountType { get; set; }

        public Nullable<decimal> discountValue { get; set; }
        public Nullable<decimal> total { get; set; }
        public Nullable<decimal> totalNet { get; set; }
        public Nullable<decimal> paid { get; set; }
        public Nullable<decimal> deserved { get; set; }
        public Nullable<System.DateTime> deservedDate { get; set; }
        public Nullable<System.DateTime> invDate { get; set; }

        public Nullable<int> invoiceMainId { get; set; }
        public string invCase { get; set; }
        public Nullable<System.TimeSpan> invTime { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }//
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<byte> isApproved { get; set; }
        public Nullable<decimal> tax { get; set; }

        public Nullable<int> updateUserId { get; set; }
        public int count { get; set; }



        //pos
        public Nullable<int> posId { get; set; }
        public string posName { get; set; }
        public string posCode { get; set; }
        //branch

        public Nullable<int> branchCreatorId { get; set; }
        public string branchCreatorName { get; set; }
        public Nullable<int> branchId { get; set; }
        public string branchName { get; set; }
        public string branchType { get; set; }


        //agent
        public Nullable<int> agentId { get; set; }
        public string agentCompany { get; set; }
        public string agentName { get; set; }

        public string agentType { get; set; }
        public string agentCode { get; set; }
        public string vendorInvNum { get; set; }


        public Nullable<System.DateTime> vendorInvDate { get; set; }
        //user
        public Nullable<int> createUserId { get; set; }
        public string cuserName { get; set; }
        public string cuserLast { get; set; }
        public string cUserAccName { get; set; }
        public string uuserName { get; set; }
        public string uuserLast { get; set; }
        public string uUserAccName { get; set; }
        public Nullable<int> userId { get; set; }
        //membership

        public Nullable<int> membershipId { get; set; }
        public string membershipsCode { get; set; }
        public string membershipsName { get; set; }
        public List<CouponInvoiceModel> CouponInvoiceList { get; set; }
        public List<ItemTransferModel> itemsTransferList { get; set; }
        public List<invoicesClassModel> invoiceClassDiscountList { get; set; }
        public decimal invclassDiscount { get; set; }
        public decimal couponDiscount { get; set; }
        public decimal offerDiscount { get; set; }
        public decimal totalDiscount { get; set; }

        public Nullable<System.DateTime> endDate { get; set; }
        public string subscriptionType { get; set; }
        public AgentMembershipCashModel agentMembershipcashobj { get; set; }
        public List<AgentMembershipCashModel> agentMembershipcashobjList { get; set; }

        public string invoicesClassName { get; set; }
        public Nullable<int> invClassDiscountId { get; set; }

        public Nullable<int> invClassId { get; set; }
        public byte invClassdiscountType { get; set; }
        public decimal invClassdiscountValue { get; set; }
        public decimal finalDiscount { get; set; }

    }

}