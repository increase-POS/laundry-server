﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class incposdbEntities : DbContext
    {
        public incposdbEntities()
            : base("name=incposdbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<agentMembershipCash> agentMembershipCash { get; set; }
        public virtual DbSet<agents> agents { get; set; }
        public virtual DbSet<banks> banks { get; set; }
        public virtual DbSet<bondes> bondes { get; set; }
        public virtual DbSet<branches> branches { get; set; }
        public virtual DbSet<branchesUsers> branchesUsers { get; set; }
        public virtual DbSet<branchStore> branchStore { get; set; }
        public virtual DbSet<cards> cards { get; set; }
        public virtual DbSet<cashTransfer> cashTransfer { get; set; }
        public virtual DbSet<categories> categories { get; set; }
        public virtual DbSet<cities> cities { get; set; }
        public virtual DbSet<countriesCodes> countriesCodes { get; set; }
        public virtual DbSet<coupons> coupons { get; set; }
        public virtual DbSet<couponsInvoices> couponsInvoices { get; set; }
        public virtual DbSet<couponsMemberships> couponsMemberships { get; set; }
        public virtual DbSet<dishIngredients> dishIngredients { get; set; }
        public virtual DbSet<docImages> docImages { get; set; }
        public virtual DbSet<error> error { get; set; }
        public virtual DbSet<Expenses> Expenses { get; set; }
        public virtual DbSet<groupObject> groupObject { get; set; }
        public virtual DbSet<groups> groups { get; set; }
        public virtual DbSet<hallSections> hallSections { get; set; }
        public virtual DbSet<Inventory> Inventory { get; set; }
        public virtual DbSet<inventoryItemLocation> inventoryItemLocation { get; set; }
        public virtual DbSet<invoiceClassDiscount> invoiceClassDiscount { get; set; }
        public virtual DbSet<invoiceOrder> invoiceOrder { get; set; }
        public virtual DbSet<invoices> invoices { get; set; }
        public virtual DbSet<invoicesClass> invoicesClass { get; set; }
        public virtual DbSet<invoicesClassMemberships> invoicesClassMemberships { get; set; }
        public virtual DbSet<invoiceStatus> invoiceStatus { get; set; }
        public virtual DbSet<invoiceTables> invoiceTables { get; set; }
        public virtual DbSet<itemOrderPreparing> itemOrderPreparing { get; set; }
        public virtual DbSet<items> items { get; set; }
        public virtual DbSet<itemsLocations> itemsLocations { get; set; }
        public virtual DbSet<itemsMaterials> itemsMaterials { get; set; }
        public virtual DbSet<itemsOffers> itemsOffers { get; set; }
        public virtual DbSet<itemsProp> itemsProp { get; set; }
        public virtual DbSet<itemsTransfer> itemsTransfer { get; set; }
        public virtual DbSet<itemsUnits> itemsUnits { get; set; }
        public virtual DbSet<itemTransferOffer> itemTransferOffer { get; set; }
        public virtual DbSet<itemUnitUser> itemUnitUser { get; set; }
        public virtual DbSet<locations> locations { get; set; }
        public virtual DbSet<medalAgent> medalAgent { get; set; }
        public virtual DbSet<medals> medals { get; set; }
        public virtual DbSet<memberships> memberships { get; set; }
        public virtual DbSet<membershipsOffers> membershipsOffers { get; set; }
        public virtual DbSet<menuSettings> menuSettings { get; set; }
        public virtual DbSet<notification> notification { get; set; }
        public virtual DbSet<notificationUser> notificationUser { get; set; }
        public virtual DbSet<objects> objects { get; set; }
        public virtual DbSet<offers> offers { get; set; }
        public virtual DbSet<orderPreparing> orderPreparing { get; set; }
        public virtual DbSet<orderPreparingStatus> orderPreparingStatus { get; set; }
        public virtual DbSet<packages> packages { get; set; }
        public virtual DbSet<paperSize> paperSize { get; set; }
        public virtual DbSet<Points> Points { get; set; }
        public virtual DbSet<pos> pos { get; set; }
        public virtual DbSet<posSerials> posSerials { get; set; }
        public virtual DbSet<posSetting> posSetting { get; set; }
        public virtual DbSet<posUsers> posUsers { get; set; }
        public virtual DbSet<printers> printers { get; set; }
        public virtual DbSet<ProgramDetails> ProgramDetails { get; set; }
        public virtual DbSet<properties> properties { get; set; }
        public virtual DbSet<propertiesItems> propertiesItems { get; set; }
        public virtual DbSet<reservations> reservations { get; set; }
        public virtual DbSet<residentialSectors> residentialSectors { get; set; }
        public virtual DbSet<residentialSectorsUsers> residentialSectorsUsers { get; set; }
        public virtual DbSet<sections> sections { get; set; }
        public virtual DbSet<serials> serials { get; set; }
        public virtual DbSet<setting> setting { get; set; }
        public virtual DbSet<setValues> setValues { get; set; }
        public virtual DbSet<shippingCompanies> shippingCompanies { get; set; }
        public virtual DbSet<storageCost> storageCost { get; set; }
        public virtual DbSet<subscriptionFees> subscriptionFees { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<sysEmails> sysEmails { get; set; }
        public virtual DbSet<tables> tables { get; set; }
        public virtual DbSet<tablesReservations> tablesReservations { get; set; }
        public virtual DbSet<tags> tags { get; set; }
        public virtual DbSet<units> units { get; set; }
        public virtual DbSet<users> users { get; set; }
        public virtual DbSet<userSetValues> userSetValues { get; set; }
        public virtual DbSet<usersLogs> usersLogs { get; set; }
    }
}
