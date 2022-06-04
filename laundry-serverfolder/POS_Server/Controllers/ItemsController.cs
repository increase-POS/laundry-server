using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using POS_Server.Models;
using System.Web;
using System.IO;
using LinqKit;
using Microsoft.Ajax.Utilities;
using POS_Server.Classes;
using POS_Server.Models.VM;
using System.Security.Claims;
using Newtonsoft.Json.Converters;
using System.Web;
using System.Data.Entity.Validation;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/Items")]
    public class ItemsController : ApiController
    {
        private Classes.Calculate Calc = new Classes.Calculate();

        List<int> categoriesId = new List<int>();
        List<string> purchaseTypes = new List<string>() { "PurchaseNormal", "PurchaseExpire" };
        List<string> salesTypes = new List<string>() { "SalesNormal", "packageItems" };
       
        [HttpPost]
        [Route("GetPurchaseItems")]
        public string GetPurchaseItems(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                Boolean canDelete = false;
                DateTime cmpdate = DateTime.Now.AddDays(newdays);
              
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemsList = (from I in entity.items.Where(x => purchaseTypes.Contains(x.type) && x.isActive == 1)
                                     select new ItemModel()
                                     {
                                         itemId = I.itemId,
                                         name = I.name,
                                         code = I.code,
                                         categoryId = I.categoryId,
                                         max = I.max,
                                         maxUnitId = I.maxUnitId,
                                         minUnitId = I.minUnitId,
                                         min = I.min,

                                         parentId = I.parentId,
                                         isActive = I.isActive,
                                         image = I.image,
                                         type = I.type,
                                         details = I.details,
                                         taxes = I.taxes,
                                         createDate = I.createDate,
                                         updateDate = I.updateDate,
                                         createUserId = I.createUserId,
                                         updateUserId = I.updateUserId,
                                         isNew = 0,
                                         parentName = entity.items.Where(m => m.itemId == I.parentId).FirstOrDefault().name,
                                         minUnitName = entity.units.Where(m => m.unitId == I.minUnitId).FirstOrDefault().name,
                                         maxUnitName = entity.units.Where(m => m.unitId == I.minUnitId).FirstOrDefault().name,
                                         avgPurchasePrice = I.avgPurchasePrice,
                                         notes = I.notes,
                                         categoryString = I.categoryString,
                                         categoryName=I.categories.name,
                                         itemUnitId = entity.itemsUnits.Where(m => m.itemId == I.itemId && m.defaultPurchase == 1).FirstOrDefault().itemUnitId,
                                     })
                                   .ToList();

                    var itemsofferslist = (from off in entity.offers

                                           join itof in entity.itemsOffers on off.offerId equals itof.offerId // itemsOffers and offers 

                                           //  join iu in entity.itemsUnits on itof.iuId  equals  iu.itemUnitId //itemsUnits and itemsOffers
                                           join iu in entity.itemsUnits on itof.iuId equals iu.itemUnitId
                                           //from un in entity.units
                                           select new ItemSalePurModel()
                                           {
                                               itemId = iu.itemId,
                                               itemUnitId = itof.iuId,
                                               offerName = off.name,
                                               offerId = off.offerId,
                                               discountValue = off.discountValue,
                                               isNew = 0,
                                               isOffer = 1,
                                               isActiveOffer = off.isActive,
                                               startDate = off.startDate,
                                               endDate = off.endDate,
                                               unitId = iu.unitId,

                                               price = iu.price,
                                               discountType = off.discountType,
                                               desPrice = iu.price,
                                               defaultSale = iu.defaultSale,
                                       

                                           }).Where(IO => IO.isActiveOffer == 1 && DateTime.Compare((DateTime)IO.startDate, DateTime.Now) <= 0 && System.DateTime.Compare((DateTime)IO.endDate, DateTime.Now) >= 0 && IO.defaultSale == 1).Distinct().ToList();
                    //.Where(IO => IO.isActiveOffer == 1 && DateTime.Compare(IO.startDate,DateTime.Now)<0 && System.DateTime.Compare(IO.endDate, DateTime.Now) > 0).ToList();

                    // test

                    var unt = (from unitm in entity.itemsUnits
                               join untb in entity.units on unitm.unitId equals untb.unitId
                               join itemtb in entity.items on unitm.itemId equals itemtb.itemId

                               select new ItemSalePurModel()
                               {
                                   itemId = itemtb.itemId,
                                   name = itemtb.name,
                                   code = itemtb.code,


                                   max = itemtb.max,
                                   maxUnitId = itemtb.maxUnitId,
                                   minUnitId = itemtb.minUnitId,
                                   min = itemtb.min,

                                   parentId = itemtb.parentId,
                                   isActive = itemtb.isActive,

                                   isOffer = 0,
                                   desPrice = 0,

                                   offerName = "",
                                   createDate = itemtb.createDate,
                                   defaultSale = unitm.defaultSale,
                                   unitName = untb.name,
                                   unitId = untb.unitId,
                                   price = unitm.price,
                        
                               }).Where(a => a.defaultSale == 1).Distinct().ToList();

                    if (itemsList.Count > 0)
                    {
                        for (int i = 0; i < itemsList.Count; i++)
                        {
                            canDelete = false;
                            if (itemsList[i].isActive == 1)
                            {
                                int itemId = (int)itemsList[i].itemId;
                                var childItemL = entity.items.Where(x => x.parentId == itemId).Select(b => new { b.itemId }).FirstOrDefault();
                                var itemUnitsL = entity.itemsUnits.Where(x => x.itemId == itemId).Select(b => new { b.itemUnitId }).FirstOrDefault();
                                string itemType = itemsList[i].type;
                                int isInInvoice = 0;
                                if (itemUnitsL != null)
                                {
                                    isInInvoice = entity.itemsTransfer.Where(x => x.itemUnitId == itemUnitsL.itemUnitId).Select(x => x.itemsTransId).FirstOrDefault();
                                }

                                if (childItemL is null && (itemUnitsL is null || itemUnitsL != null && isInInvoice == 0))
                                    canDelete = true;
                            }
                            itemsList[i].canDelete = canDelete;

                            foreach (var itofflist in itemsofferslist)
                            {


                                if (itemsList[i].itemId == itofflist.itemId)
                                {

                                    // get unit name of item that has the offer
                                    using (incposdbEntities entitydb = new incposdbEntities())
                                    { // put it in item
                                        var un = entitydb.units
                                         .Where(a => a.unitId == itofflist.unitId)
                                            .Select(u => new
                                            {
                                                u.name
                                           ,
                                                u.unitId
                                            }).FirstOrDefault();
                                        itemsList[i].unitName = un.name;
                                    }

                                    itemsList[i].offerName = itemsList[i].offerName + "- " + itofflist.offerName;
                                    itemsList[i].isOffer = 1;
                                    itemsList[i].startDate = itofflist.startDate;
                                    itemsList[i].endDate = itofflist.endDate;
                                    itemsList[i].itemUnitId = itofflist.itemUnitId;
                                    itemsList[i].offerId = itofflist.offerId;
                                    itemsList[i].isActiveOffer = itofflist.isActiveOffer;

                                    itemsList[i].price = itofflist.price;
                                    itemsList[i].priceTax = itemsList[i].price + (itemsList[i].price * itemsList[i].taxes / 100);

                                    itemsList[i].avgPurchasePrice = itemsList[i].avgPurchasePrice;
                                }
                            }
                            // is new
                            int res = DateTime.Compare((DateTime)itemsList[i].createDate, cmpdate);
                            if (res >= 0)
                            {
                                itemsList[i].isNew = 1;
                            }

                        }
                    }
                    return TokenManager.GenerateToken(itemsList);
                }
            }
        }

        [HttpPost]
        [Route("GetAllSalesItems")]
        public string GetAllSalesItems(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                DateTime cmpdate = DateTime.Now.AddDays(newdays);

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemsList = (from I in entity.items.Where(x => salesTypes.Contains(x.type) && x.isActive == 1)
                                     select new ItemModel()
                                     {
                                         itemId = I.itemId,
                                         name = I.name,
                                         code = I.code,
                                         categoryId = I.categoryId,
                                         max = I.max,
                                         maxUnitId = I.maxUnitId,
                                         minUnitId = I.minUnitId,
                                         min = I.min,
                                         tagId = I.tagId,
                                         parentId = I.parentId,
                                         isActive = I.isActive,
                                         image = I.image,
                                         type = I.type,
                                         details = I.details,
                                         taxes = I.taxes,
                                         createDate = I.createDate,
                                         updateDate = I.updateDate,
                                         createUserId = I.createUserId,
                                         updateUserId = I.updateUserId,
                                         isNew = 0,
                                         parentName = entity.items.Where(m => m.itemId == I.parentId).FirstOrDefault().name,
                                         minUnitName = entity.units.Where(m => m.unitId == I.minUnitId).FirstOrDefault().name,
                                         maxUnitName = entity.units.Where(m => m.unitId == I.minUnitId).FirstOrDefault().name,
                                         avgPurchasePrice = I.avgPurchasePrice,
                                         notes = I.notes,
                                         categoryString = I.categoryString,
                                         itemUnitId = entity.itemsUnits.Where(m => m.itemId == I.itemId && m.defaultSale == 1).FirstOrDefault().itemUnitId,
                                         price = entity.itemsUnits.Where(m => m.itemId == I.itemId && m.defaultSale == 1).FirstOrDefault().price   ,
                                         priceWithService = entity.itemsUnits.Where(m => m.itemId == I.itemId && m.defaultSale == 1).FirstOrDefault().priceWithService ,
                                     })
                                   .ToList();                  

                    if (itemsList.Count > 0)
                    {
                        for (int i = 0; i < itemsList.Count; i++)
                        {
                            // is new
                            int res = DateTime.Compare((DateTime)itemsList[i].createDate, cmpdate);
                            if (res >= 0)
                            {
                                itemsList[i].isNew = 1;
                            }

                        }
                    }
                    return TokenManager.GenerateToken(itemsList);
                }
            }
        }

        [HttpPost]
        [Route("GetAllSalesItemsInv")]
        public string GetAllSalesItemsInv(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region params
                string day = "";
                string invType = "";
                int branchId = 0;
                int membershipId = 0;
                DateTime cmpdate = DateTime.Now.AddDays(newdays);
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "day")
                    {
                        day = c.Value;
                    }
                    else if (c.Type == "invType")
                    {
                        invType = c.Value;
                    }
                    else if (c.Type == "membershipId")
                    {
                        membershipId = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }
                #endregion
                #region get day column in db
                var searchPredicate = PredicateBuilder.New<menuSettings>();
                searchPredicate = searchPredicate.And(x => x.isActive == 1 && x.branchId == branchId);
                switch (day)
                {
                    case "saturday":
                        searchPredicate = searchPredicate.And(x => x.sat == true);
                        break;
                    case "sunday":
                        searchPredicate = searchPredicate.And(x => x.sun == true);
                        break;
                    case "monday":
                        searchPredicate = searchPredicate.And(x => x.mon == true);
                        break;
                    case "tuesday":
                        searchPredicate = searchPredicate.And(x => x.tues == true);
                        break;
                    case "wednsday":
                        searchPredicate = searchPredicate.And(x => x.wed == true);
                        break;
                    case "thursday":
                        searchPredicate = searchPredicate.And(x => x.thur == true);
                        break;
                    case "friday":
                        searchPredicate = searchPredicate.And(x => x.fri == true);
                        break;
                }
                #endregion
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemsList = (from I in entity.items.Where(x => salesTypes.Contains(x.type) && x.isActive == 1)
                                     join IU in entity.itemsUnits on I.itemId equals IU.itemId
                                     join ms in entity.menuSettings.Where(searchPredicate) on IU.itemUnitId equals ms.itemUnitId
                                     select new ItemSalePurModel()
                                     {
                                         itemId = I.itemId,
                                         name = I.name,
                                         code = I.code,
                                         categoryId = I.categoryId,
                                         max = I.max,
                                         maxUnitId = I.maxUnitId,
                                         minUnitId = I.minUnitId,
                                         min = I.min,
                                         tagId = I.tagId,
                                         parentId = I.parentId,
                                         isActive = I.isActive,
                                         image = I.image,
                                         type = I.type,
                                         details = I.details,
                                         taxes = I.taxes,
                                         createDate = I.createDate,
                                         updateDate = I.updateDate,
                                         createUserId = I.createUserId,
                                         updateUserId = I.updateUserId,
                                         isNew = 0,
                                         parentName = entity.items.Where(m => m.itemId == I.parentId).FirstOrDefault().name,
                                         minUnitName = entity.units.Where(m => m.unitId == I.minUnitId).FirstOrDefault().name,
                                         maxUnitName = entity.units.Where(m => m.unitId == I.minUnitId).FirstOrDefault().name,
                                         avgPurchasePrice = I.avgPurchasePrice,
                                         notes = I.notes,
                                         categoryString = I.categoryString,
                                         
                                         itemUnitId = entity.itemsUnits.Where(m => m.itemId == I.itemId && m.defaultSale == 1).FirstOrDefault().itemUnitId,
                                         price = entity.itemsUnits.Where(m => m.itemId == I.itemId && m.defaultSale == 1).FirstOrDefault().price,
                                         basicPrice = entity.itemsUnits.Where(m => m.itemId == I.itemId && m.defaultSale == 1).FirstOrDefault().price,
                                         priceWithService = entity.itemsUnits.Where(m => m.itemId == I.itemId && m.defaultSale == 1).FirstOrDefault().priceWithService,
                                     })
                                   .ToList();
                    #region offers

                    var offerslist = (from off in entity.offers

                                      join itof in entity.itemsOffers on off.offerId equals itof.offerId // itemsOffers and offers 

                                      //  join iu in entity.itemsUnits on itof.iuId  equals  iu.itemUnitId //itemsUnits and itemsOffers
                                      join iu in entity.itemsUnits on itof.iuId equals iu.itemUnitId
                                      //from un in entity.units
                                      select new ItemSalePurModel()
                                      {
                                          itemId = iu.itemId,
                                          itemUnitId = itof.iuId,
                                          offerName = off.name,
                                          offerId = off.offerId,
                                          discountValue = off.discountValue,
                                          isNew = 0,
                                          isOffer = 1,
                                          isActiveOffer = off.isActive,
                                          startDate = off.startDate,
                                          endDate = off.endDate,
                                          unitId = iu.unitId,
                                          itemCount = itof.quantity,
                                          price = iu.price,
                                          priceWithService = iu.priceWithService,
                                          discountType = off.discountType,
                                          desPrice = iu.price,
                                          defaultSale = iu.defaultSale,
                                          used = itof.used,
                                          forAgent = off.forAgents,
                                          isActive = off.isActive,

                                      }).Where(IO => IO.isActive == 1 && IO.isActiveOffer == 1 && IO.forAgent == "pb" && DateTime.Compare((DateTime)IO.startDate, DateTime.Now) <= 0
                                                   && System.DateTime.Compare((DateTime)IO.endDate, DateTime.Now) >= 0 && IO.defaultSale == 1 && IO.itemCount > IO.used).Distinct().ToList();

                    var membershipOffers = (from off in entity.offers
                                            join mo in entity.membershipsOffers.Where(x => x.membershipId == membershipId) on off.offerId equals mo.offerId

                                            join itof in entity.itemsOffers on off.offerId equals itof.offerId // itemsOffers and offers 

                                            //  join iu in entity.itemsUnits on itof.iuId  equals  iu.itemUnitId //itemsUnits and itemsOffers
                                            join iu in entity.itemsUnits on itof.iuId equals iu.itemUnitId
                                            //from un in entity.units
                                            select new ItemSalePurModel()
                                            {
                                                itemId = iu.itemId,
                                                itemUnitId = itof.iuId,
                                                offerName = off.name,
                                                offerId = off.offerId,
                                                discountValue = off.discountValue,
                                                isNew = 0,
                                                isOffer = 1,
                                                isActiveOffer = off.isActive,
                                                startDate = off.startDate,
                                                endDate = off.endDate,
                                                unitId = iu.unitId,
                                                itemCount = itof.quantity,
                                                price = iu.price,
                                                priceWithService = iu.priceWithService,
                                                discountType = off.discountType,
                                                desPrice = iu.price,
                                                defaultSale = iu.defaultSale,
                                                used = itof.used,
                                                forAgent = off.forAgents,
                                                isActive = off.isActive,

                                            }).Where(IO => IO.isActive == 1 && IO.isActiveOffer == 1 && DateTime.Compare((DateTime)IO.startDate, DateTime.Now) <= 0
                                                         && System.DateTime.Compare((DateTime)IO.endDate, DateTime.Now) >= 0 && IO.defaultSale == 1 && IO.itemCount > IO.used).Distinct().ToList();

                    // return membershipOffers.Count.ToString();
                    offerslist.AddRange(membershipOffers);

                    #endregion

                    for (int i = 0; i < itemsList.Count; i++)
                    {
                        if (invType == "diningHall")
                        {
                            itemsList[i].price = itemsList[i].priceWithService;
                        }

                       itemsList[i].priceTax = itemsList[i].price + (itemsList[i].price * itemsList[i].priceTax) / 100;
                        // is new
                        int res = DateTime.Compare((DateTime)itemsList[i].createDate, cmpdate);
                        if (res >= 0)
                        {
                            itemsList[i].isNew = 1;
                        }

                        decimal totaldis = 0;
                        foreach (var itofflist in offerslist)
                        {


                            if (itemsList[i].itemId == itofflist.itemId)
                            {

                                // get unit name of item that has the offer
                                using (incposdbEntities entitydb = new incposdbEntities())
                                { // put it in item
                                    var un = entitydb.units
                                        .Where(a => a.unitId == itofflist.unitId)
                                        .Select(u => new
                                        {
                                            u.name
                                        ,
                                            u.unitId
                                        }).FirstOrDefault();
                                    itemsList[i].unitName = un.name;
                                }

                                itemsList[i].offerName = itemsList[i].offerName + "- " + itofflist.offerName;
                                itemsList[i].isOffer = 1;
                                itemsList[i].startDate = itofflist.startDate;
                                itemsList[i].endDate = itofflist.endDate;
                                itemsList[i].itemUnitId = itofflist.itemUnitId;
                                itemsList[i].offerId = itofflist.offerId;
                                itemsList[i].isActiveOffer = itofflist.isActiveOffer;
                                itemsList[i].forAgent = itofflist.forAgent;

                                if (invType == "diningHall")
                                {
                                    itofflist.price = itofflist.priceWithService;
                                }

                                itemsList[i].price = itofflist.price;
                                itemsList[i].priceTax = itemsList[i].price + (itemsList[i].price * itemsList[i].taxes / 100);
                                itemsList[i].avgPurchasePrice = itemsList[i].avgPurchasePrice;
                                itemsList[i].discountType = itofflist.discountType;
                                itemsList[i].discountValue = itofflist.discountValue;

                                if (itofflist.used == null)
                                    itofflist.used = 0;

                                if (itemsList[i].itemCount >= (itofflist.itemCount - itofflist.used))
                                    itemsList[i].itemCount = (itofflist.itemCount - itofflist.used);

                                if (itemsList[i].discountType == "1") // value
                                {

                                    totaldis = totaldis + (decimal)itemsList[i].discountValue;
                                }
                                else if (itemsList[i].discountType == "2") // percent
                                {

                                    totaldis = totaldis + Calc.percentValue(itemsList[i].price, itemsList[i].discountValue);

                                }
                            }
                        }
                        itemsList[i].price = (decimal)itemsList[i].price - totaldis;
                        itemsList[i].priceTax = itemsList[i].price + (itemsList[i].price * itemsList[i].taxes / 100);

                        if(itemsList[i].price < 0)
                        {
                            itemsList[i].price = 0;
                            itemsList[i].priceTax = 0;
                        }
                    }


                    return TokenManager.GenerateToken(itemsList);
                }
            }
        }

        [HttpPost]
        [Route("GetItemsMenuSetting")]
        public string GetItemsMenuSetting(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int branchId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }
                DateTime cmpdate = DateTime.Now.AddDays(newdays);

                using (incposdbEntities entity = new incposdbEntities())
                {
                    try
                    {
                        var itemsList = (from I in entity.items.Where(x => salesTypes.Contains(x.type) && x.isActive == 1)
                                         join iu in entity.itemsUnits on I.itemId equals iu.itemId
                                         join m in entity.menuSettings.Where(x => x.branchId == branchId ) on iu.itemUnitId equals m.itemUnitId into yj
                                         from ms in yj.DefaultIfEmpty()
                                         select new MenuSettingModel()
                                         {
                                             menuSettingId = ms.menuSettingId,
                                             isActive =  ms.isActive == null ? (byte)0: ms.isActive,
                                             preparingTime = ms.preparingTime,
                                             sat = ms.sat == null ? false : ms.sat,
                                             sun = ms.sun == null ? false : ms.sun,
                                             mon = ms.mon == null ? false : ms.mon,
                                             tues = ms.tues == null ? false : ms.tues,
                                             wed = ms.wed == null ? false : ms.wed,
                                             thur = ms.thur == null ? false : ms.thur,
                                             fri = ms.fri == null ? false : ms.fri,
                                             branchId = ms.branchId,
                                             itemId = I.itemId,
                                             name = I.name,
                                             code = I.code,
                                             categoryId = I.categoryId,
                                             tagId = I.tagId,
                                             image = I.image,
                                             type = I.type,
                                             details = I.details,
                                             createDate = I.createDate,
                                             updateDate = I.updateDate,
                                             createUserId = ms.createUserId,
                                             updateUserId = ms.updateUserId,
                                             itemUnitId = iu.itemUnitId,
                                             price = iu.price,
                                             priceWithService = iu.priceWithService,
                                         }).Distinct()
                                       .ToList();

                        for (int i = 0; i < itemsList.Count; i++)
                        {
                            // is new
                            int res = DateTime.Compare((DateTime)itemsList[i].createDate, cmpdate);
                            if (res >= 0)
                            {
                                itemsList[i].isNew = 1;
                            }

                        }

                        return TokenManager.GenerateToken(itemsList);
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        return TokenManager.GenerateToken("0");
                        //string message = "";
                        //foreach (var validationErrors in dbEx.EntityValidationErrors)
                        //{
                        //    foreach (var validationError in validationErrors.ValidationErrors)
                        //    {
                        //       message += "Property: {0} Error: {1}"+ validationError.PropertyName+ validationError.ErrorMessage;
                        //    }
                        //}
                        //return message;
                    }
                }
            }
        }

        [HttpPost]
        [Route("GetSalesItems")]
        public string GetSalesItems(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                Boolean canDelete = false;
                DateTime cmpdate = DateTime.Now.AddDays(newdays);
                string type = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                }
               
                using (incposdbEntities entity = new incposdbEntities())
                {
                        var searchPredicate = PredicateBuilder.New<items>();
                    searchPredicate = searchPredicate.And(x => true);
                    if (type != "")
                        searchPredicate = searchPredicate.And(x => x.type == type);
                    else
                        searchPredicate = searchPredicate.And(x => salesTypes.Contains(x.type));
                        var itemsList = (from I in entity.items.Where(searchPredicate)
                                     join u in entity.itemsUnits on I.itemId equals u.itemId
                                     join c in entity.categories on I.categoryId equals c.categoryId into lj
                                     from x in lj.DefaultIfEmpty()
                                     select new ItemModel()
                                     {
                                         itemId = I.itemId,
                                         name = I.name,
                                         code = I.code,
                                         categoryId = I.categoryId,
                                         categoryName = x.name,
                                         max = I.max,
                                         maxUnitId = I.maxUnitId,
                                         minUnitId = I.minUnitId,
                                         min = I.min,
                                         barcode = u.barcode,
                                         parentId = I.parentId,
                                         isActive = I.isActive,
                                         image = I.image,
                                         type = I.type,
                                         details = I.details,
                                         taxes = I.taxes,
                                         createDate = I.createDate,
                                         updateDate = I.updateDate,
                                         createUserId = I.createUserId,
                                         updateUserId = I.updateUserId,
                                         isNew = 0,
                                         parentName = entity.items.Where(m => m.itemId == I.parentId).FirstOrDefault().name,
                                         avgPurchasePrice = I.avgPurchasePrice,
                                         notes = I.notes,
                                         categoryString = I.categoryString,
                                         tagId = I.tagId,
                                         priceWithService = entity.itemsUnits.Where(m => m.itemId == I.itemId).FirstOrDefault().priceWithService,
                                         price = entity.itemsUnits.Where(m => m.itemId == I.itemId).FirstOrDefault().price,
                                     })
                                   .ToList();

                    var itemsofferslist = (from off in entity.offers

                                           join itof in entity.itemsOffers on off.offerId equals itof.offerId // itemsOffers and offers 

                                           //  join iu in entity.itemsUnits on itof.iuId  equals  iu.itemUnitId //itemsUnits and itemsOffers
                                           join iu in entity.itemsUnits on itof.iuId equals iu.itemUnitId
                                           //from un in entity.units
                                           select new ItemSalePurModel()
                                           {
                                               itemId = iu.itemId,
                                               itemUnitId = itof.iuId,
                                               offerName = off.name,
                                               offerId = off.offerId,
                                               discountValue = off.discountValue,
                                               isNew = 0,
                                               isOffer = 1,
                                               isActiveOffer = off.isActive,
                                               startDate = off.startDate,
                                               endDate = off.endDate,
                                               unitId = iu.unitId,

                                               price = iu.price,
                                               discountType = off.discountType,
                                               desPrice = iu.price,
                                               defaultSale = iu.defaultSale,
                                       

                                           }).Where(IO => IO.isActiveOffer == 1 && DateTime.Compare((DateTime)IO.startDate, DateTime.Now) <= 0 && System.DateTime.Compare((DateTime)IO.endDate, DateTime.Now) >= 0 && IO.defaultSale == 1).Distinct().ToList();

                    //var unt = (from unitm in entity.itemsUnits
                    //           join untb in entity.units on unitm.unitId equals untb.unitId
                    //           join itemtb in entity.items on unitm.itemId equals itemtb.itemId

                    //           select new ItemSalePurModel()
                    //           {
                    //               itemId = itemtb.itemId,
                    //               name = itemtb.name,
                    //               code = itemtb.code,


                    //               max = itemtb.max,
                    //               maxUnitId = itemtb.maxUnitId,
                    //               minUnitId = itemtb.minUnitId,
                    //               min = itemtb.min,

                    //               parentId = itemtb.parentId,
                    //               isActive = itemtb.isActive,

                    //               isOffer = 0,
                    //               desPrice = 0,

                    //               offerName = "",
                    //               createDate = itemtb.createDate,
                    //               defaultSale = unitm.defaultSale,
                    //               unitName = untb.name,
                    //               unitId = untb.unitId,
                    //               price = unitm.price,
                        
                    //           }).Where(a => a.defaultSale == 1).Distinct().ToList();

                    if (itemsList.Count > 0)
                    {
                        for (int i = 0; i < itemsList.Count; i++)
                        {
                            canDelete = false;
                            if (itemsList[i].isActive == 1)
                            {
                                int itemId = (int)itemsList[i].itemId;
                                var childItemL = entity.items.Where(x => x.parentId == itemId).Select(b => new { b.itemId }).FirstOrDefault();
                                var itemUnitsL = entity.itemsUnits.Where(x => x.itemId == itemId).Select(b => new { b.itemUnitId }).FirstOrDefault();
                                string itemType = itemsList[i].type;
                                int isInInvoice = 0;
                                if ( itemUnitsL != null)
                                {
                                    isInInvoice = entity.itemsTransfer.Where(x => x.itemUnitId == itemUnitsL.itemUnitId).Select(x => x.itemsTransId).FirstOrDefault();
                                }

                                if (childItemL is null  && (itemUnitsL is null || itemUnitsL != null && isInInvoice == 0))
                                    canDelete = true;
                            }
                            itemsList[i].canDelete = canDelete;

                            foreach (var itofflist in itemsofferslist)
                            {


                                if (itemsList[i].itemId == itofflist.itemId)
                                {

                                    // get unit name of item that has the offer
                                    using (incposdbEntities entitydb = new incposdbEntities())
                                    { // put it in item
                                        var un = entitydb.units
                                         .Where(a => a.unitId == itofflist.unitId)
                                            .Select(u => new
                                            {
                                                u.name
                                           ,
                                                u.unitId
                                            }).FirstOrDefault();
                                        itemsList[i].unitName = un.name;
                                    }

                                    itemsList[i].offerName = itemsList[i].offerName + "- " + itofflist.offerName;
                                    itemsList[i].isOffer = 1;
                                    itemsList[i].startDate = itofflist.startDate;
                                    itemsList[i].endDate = itofflist.endDate;
                                    itemsList[i].itemUnitId = itofflist.itemUnitId;
                                    itemsList[i].offerId = itofflist.offerId;
                                    itemsList[i].isActiveOffer = itofflist.isActiveOffer;

                                    itemsList[i].price = itofflist.price;
                                    itemsList[i].priceTax = itemsList[i].price + (itemsList[i].price * itemsList[i].taxes / 100);

                                    itemsList[i].avgPurchasePrice = itemsList[i].avgPurchasePrice;
                                }
                            }
                            //itemsList[i].desPrice = itemsList[i].priceTax - totaldis;
                            // is new
                            int res = DateTime.Compare((DateTime)itemsList[i].createDate, cmpdate);
                            if (res >= 0)
                            {
                                itemsList[i].isNew = 1;
                            }

                        }
                    }
                    return TokenManager.GenerateToken(itemsList);
                }
            }
        }        


       
        [HttpPost]
        [Route("GetKitchenItems")]
        public string GetKitchenItems(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                ItemsLocationsController ilc = new ItemsLocationsController();
                int branchId = 0;
                int categoryId = 0;
                List<string> typeLst = new List<string>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "categoryId")
                    {
                        categoryId = int.Parse(c.Value);
                    }
                }

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var searchPredicate = PredicateBuilder.New<items>();
                    searchPredicate = searchPredicate.And(x => true);
                    if (categoryId != 0)
                        searchPredicate = searchPredicate.And(x => x.categoryId == categoryId);
                    var itemsList = (from I in entity.items.Where(searchPredicate)
                                     join u in entity.itemsUnits on I.itemId equals u.itemId
                                     join l in entity.itemsLocations.Where(x => x.locations.branchId == branchId && x.locations.isKitchen == 1 && x.quantity > 0) on u.itemUnitId equals l.itemUnitId 
                                     select new ItemModel()
                                     {
                                         itemId = I.itemId,
                                         name = I.name,
                                         code = I.code,
                                         type = I.type,
                                         isActive = I.isActive,
                                         updateDate = I.updateDate,
                                         categoryId = I.categoryId,
                                         itemUnitId = I.itemsUnits.Where(iu => iu.itemId == I.itemId && iu.defaultPurchase == 1).Select(iu => iu.itemUnitId).FirstOrDefault(),
                                         unitName= entity.units.Where(u => u.unitId==
                                         I.itemsUnits.Where(iu => iu.itemId == I.itemId && iu.defaultPurchase == 1).Select(iu => iu.unitId).FirstOrDefault()
                                         ).Select(u => u.name).FirstOrDefault() ,
                                     }).Where(x => x.isActive == 1).Distinct().ToList();

                   foreach(ItemModel item in itemsList)
                    {
                        var itemO = entity.items.Find(item.itemId);
                        if (item.itemUnitId != null && item.itemUnitId != 0)
                        {
                            int itemUnitId = (int)item.itemUnitId;
                            int count = ilc.getBranchAmount(itemUnitId, branchId,1);
                            item.itemCount = count;
                            
                        }
                        item.details = itemO.details;
                        item.image = itemO.image;
                    }
                    return TokenManager.GenerateToken(itemsList);
                   
                }
            }
        }

        [HttpPost]
        [Route("GetKitchenItemsWithUnits")]
        public string GetKitchenItemsWithUnits(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                ItemsLocationsController ilc = new ItemsLocationsController();
                int branchId = 0;
                int categoryId = 0;
                List<string> typeLst = new List<string>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "categoryId")
                    {
                        categoryId = int.Parse(c.Value);
                    }
                }

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var searchPredicate = PredicateBuilder.New<items>();
                    searchPredicate.And(x => true);
                    if (categoryId != 0)
                        searchPredicate = searchPredicate.And(x => x.categoryId == categoryId);
                    var itemsList = (from I in entity.items.Where(searchPredicate)
                                     join u in entity.itemsUnits on I.itemId equals u.itemId
                                     join l in entity.itemsLocations.Where(x => x.locations.branchId == branchId && x.locations.isKitchen == 1 && x.quantity > 0) on u.itemUnitId equals l.itemUnitId 
                                     select new ItemModel()
                                     {
                                         itemId = I.itemId,
                                         name = I.name,
                                         code = I.code,
                                         type = I.type,
                                         isActive = I.isActive,
                                         updateDate = I.updateDate,
                                        unitName = u.units.name,
                                        itemCount = (int)l.quantity,
                                        endDate = l.endDate,
                                        details = I.details,
                                     }).Where(x => x.isActive == 1).ToList();
                    return TokenManager.GenerateToken(itemsList);                 
                }
            }
        }

            
       
        [HttpPost]
        [Route("GetItemByID")]
        public string GetItemByID(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int itemId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        itemId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {

                    var item = entity.items
                   .Where(I => I.itemId == itemId)
                   .Select(I => new
                   {
                       I.itemId,
                       I.name,
                       I.code,
                       I.categoryId,
                       I.max,
                       I.maxUnitId,
                       I.minUnitId,
                       I.min,
                       I.parentId,

                       I.image,
                       I.type,
                       I.details,
                       I.taxes,
                       I.createDate,
                       I.updateDate,
                       I.createUserId,
                       I.updateUserId,

                       I.avgPurchasePrice,
                      I.notes,
                     I.categoryString,
                   })
                   .FirstOrDefault();
                    return TokenManager.GenerateToken(item);
                }
            }
        }
          
        private int getItemUnitAmount(int itemUnitId, int branchId, int isKitchen = 0)
        {
            int amount = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemInLocs = (from b in entity.branches
                                  where b.branchId == branchId
                                  join s in entity.sections.Where(x => x.isKitchen == isKitchen) on b.branchId equals s.branchId
                                  join l in entity.locations on s.sectionId equals l.sectionId
                                  join il in entity.itemsLocations on l.locationId equals il.locationId
                                  where il.itemUnitId == itemUnitId && il.quantity > 0
                                  select new
                                  {
                                      il.itemsLocId,
                                      il.quantity,
                                      il.itemUnitId,
                                      il.locationId,
                                      s.sectionId,

                                  }).ToList();
                for (int i = 0; i < itemInLocs.Count; i++)
                {
                    amount += (int)itemInLocs[i].quantity;
                }

                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.isActive == 1).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (upperUnit != null && itemUnitId == upperUnit.itemUnitId)
                    return amount;
                if (upperUnit != null)
                    amount += (int)upperUnit.unitValue * getItemUnitAmount(upperUnit.itemUnitId, branchId, isKitchen);

                return amount;
            }
        }
     
        public IEnumerable<categories> Recursive(List<categories> categoriesList, int toplevelid)
        {
            List<categories> inner = new List<categories>();

            foreach (var t in categoriesList)
            {
                categoriesId.Add(t.categoryId);
                inner.Add(t);
                inner = inner.Union(Recursive(categoriesList, t.categoryId)).ToList();
            }

            return inner;
        }
        // add or update item
        [HttpPost]
        [Route("Save")]
        public string Save(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string itemObject = "";
                items itemObj = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        itemObject = c.Value.Replace("\\", string.Empty);
                        itemObject = itemObject.Trim('"');
                        itemObj = JsonConvert.DeserializeObject<items>(itemObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (itemObj != null)
                {
                    message = saveItem(itemObj);
                }
                return TokenManager.GenerateToken(message);
            }
        }

        [HttpPost]
        [Route("saveItemsCosting")]
        public string saveItemsCosting(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string itemObject = "";
                List<ItemModel> itemObj = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        itemObject = c.Value.Replace("\\", string.Empty);
                        itemObject = itemObject.Trim('"');
                        itemObj = JsonConvert.DeserializeObject<List<ItemModel>>(itemObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (itemObj != null)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            foreach (ItemModel item in itemObj)
                            {
                                var it = entity.items.Find(item.itemId);
                                it.avgPurchasePrice = item.avgPurchasePrice;

                                int itemUnitId = (int)item.itemUnitId;
                                var itemUnit = entity.itemsUnits.Find(itemUnitId);
                                itemUnit.price = item.price;
                                itemUnit.priceWithService = item.priceWithService;

                                entity.SaveChanges();
                            }
                            message = "1";
                        }
                    }
                    catch
                    {
                        message = "0";
                    }
                }
                return TokenManager.GenerateToken(message);
            }
        }
        private string saveItem(items itemObj)
        {
            string message = "";
            if (itemObj.updateUserId == 0 || itemObj.updateUserId == null)
            {
                Nullable<int> id = null;
                itemObj.updateUserId = id;
            }
            if (itemObj.createUserId == 0 || itemObj.createUserId == null)
            {
                Nullable<int> id = null;
                itemObj.createUserId = id;
            }
            if (itemObj.categoryId == 0 || itemObj.categoryId == null)
            {
                Nullable<int> id = null;
                itemObj.categoryId = id;
            }
            if (itemObj.minUnitId == 0 || itemObj.minUnitId == null)
            {
                Nullable<int> id = null;
                itemObj.minUnitId = id;
            }
            if (itemObj.maxUnitId == 0 || itemObj.maxUnitId == null)
            {
                Nullable<int> id = null;
                itemObj.maxUnitId = id;
            }
            try
            {
                items itemModel;
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var ItemEntity = entity.Set<items>();
                    if (itemObj.itemId == 0)
                    {
                        ProgramInfo programInfo = new ProgramInfo();
                        int itemMaxCount = programInfo.getItemCount();
                        int itemsCount = entity.items.Count();
                        if (itemsCount >= itemMaxCount)
                        {
                            message = "-1";
                        }
                        else
                        {
                            itemObj.createDate = DateTime.Now;
                            itemObj.updateDate = DateTime.Now;
                            itemObj.updateUserId = itemObj.createUserId;

                            itemModel = ItemEntity.Add(itemObj);
                            entity.SaveChanges();
                            message = itemObj.itemId.ToString();

                        }
                    }
                    else
                    {
                        itemModel = entity.items.Where(p => p.itemId == itemObj.itemId).First();
                        itemModel.code = itemObj.code;
                        itemModel.categoryId = itemObj.categoryId;
                        itemModel.parentId = itemObj.parentId;
                        itemModel.details = itemObj.details;
                        itemModel.image = itemObj.image;
                        itemModel.max = itemObj.max;
                        itemModel.maxUnitId = itemObj.maxUnitId;
                        itemModel.min = itemObj.min;
                        itemModel.minUnitId = itemObj.minUnitId;
                        itemModel.name = itemObj.name;
                        itemModel.tagId = itemObj.tagId;
                        itemModel.taxes = itemObj.taxes;
                        itemModel.type = itemObj.type;
                        itemModel.updateDate = DateTime.Now;
                        itemModel.updateUserId = itemObj.updateUserId;
                        itemModel.isActive = itemObj.isActive;
                        itemModel.avgPurchasePrice = itemObj.avgPurchasePrice;
                        itemModel.notes = itemObj.notes;
                        itemModel.categoryString = itemObj.categoryString;
                        entity.SaveChanges();
                        message = itemModel.itemId.ToString();
                    }
                }
            }
            //catch (DbEntityValidationException dbEx)
            //{
            //    foreach (var validationErrors in dbEx.EntityValidationErrors)
            //    {
            //        foreach (var validationError in validationErrors.ValidationErrors)
            //        {
            //            message ="Property: {0} Error: {1}"+ validationError.PropertyName+ validationError.ErrorMessage;
            //        }
            //    }
            //}
            catch
            {
                message = "0";
            }
            return message;
        }

        [HttpPost]
        [Route("SaveSaleItem")]
        public string SaveSaleItem(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string itemObject = "";
                items itemObj = null;
                itemsUnits itemUnitObj = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        itemObject = c.Value.Replace("\\", string.Empty);
                        itemObject = itemObject.Trim('"');
                        itemObj = JsonConvert.DeserializeObject<items>(itemObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "itemUnit")
                    {
                        itemObject = c.Value.Replace("\\", string.Empty);
                        itemObject = itemObject.Trim('"');
                        itemUnitObj = JsonConvert.DeserializeObject<itemsUnits>(itemObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                }
                if (itemObj != null)
                {
                    message = saveItem(itemObj);
                    int itemId = int.Parse(message);
                    int itemUnitId = 0;
                    if (int.Parse(message) > 0)
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                           itemUnitId = entity.itemsUnits.Where(x => x.itemId == itemId).Select(x => x.itemUnitId).FirstOrDefault();
                        }
                        ItemsUnitsController ic = new ItemsUnitsController();
                        itemUnitObj.itemId = int.Parse(message);
                        itemUnitObj.itemUnitId = itemUnitId;
                        ic.saveItemUnit(itemUnitObj);
                    }
                }
                return TokenManager.GenerateToken(message);
            }
        }

        [HttpPost]
        [Route("UpdateImage")]
        public string UpdateImage(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string imageName = "";
                int itemId =0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        itemId = int.Parse(c.Value);
                    }
                    else if (c.Type == "imageName")
                    {
                        imageName = c.Value;
                    }
                }
                try
                {
                    items item;
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemEntity = entity.Set<items>();
                        item = entity.items.Where(p => p.itemId == itemId).First();
                        item.image = imageName;
                        entity.SaveChanges();
                    }
                    message = item.itemId.ToString();
                    return TokenManager.GenerateToken(message);
                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }
            }
        }

        [HttpPost]
        [Route("Delete")]
        public string Delete(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int itemId = 0;
                int userId = 0;
                Boolean final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        itemId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "final")
                    {
                        final = bool.Parse(c.Value);
                    }
                }
                if (final)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var tmpItem = entity.items.Where(I => I.itemId == itemId).First();

                            var iuitems = entity.itemsUnits.Where(x => x.itemId == tmpItem.itemId).ToList();
                            // remove from itemunit table
                            entity.itemsUnits.RemoveRange(iuitems);
                            entity.SaveChanges();

                            entity.items.Remove(tmpItem);
                            message = entity.SaveChanges().ToString();

                        }
                        return TokenManager.GenerateToken(message);
                    }
                    catch
                    {
                        return TokenManager.GenerateToken("0");
                    }
                }
                else
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var tmpItem = entity.items.Where(I => I.itemId == itemId).First();
                            tmpItem.isActive = 0;
                            tmpItem.updateDate = DateTime.Now;
                            tmpItem.updateUserId = userId;

                            message = entity.SaveChanges().ToString();
                            return TokenManager.GenerateToken(message);
                        }

                    }
                    catch
                    {
                        return TokenManager.GenerateToken("0");
                    }

                }
            }
        }
       
        [Route("PostItemImage")]
        public IHttpActionResult PostItemImage()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                foreach (string file in httpRequest.Files)
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                    var postedFile = httpRequest.Files[file];
                    string imageName = postedFile.FileName;
                    string imageWithNoExt = Path.GetFileNameWithoutExtension(postedFile.FileName);

                    if (postedFile != null && postedFile.ContentLength > 0)
                    {
                        int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB

                        IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png", ".bmp", ".jpeg", ".tiff", ".jfif" };
                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();

                        if (!AllowedFileExtensions.Contains(extension))
                        {

                            var message = string.Format("Please Upload image of type .jpg,.gif,.png, .jfif, .bmp , .jpeg ,.tiff");
                            return Ok(message);
                        }
                        else if (postedFile.ContentLength > MaxContentLength)
                        {
                            var message = string.Format("Please Upload a file upto 1 mb.");

                            return Ok(message);
                        }
                        else
                        {
                            //  check if image exist
                            var pathCheck = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\item"), imageWithNoExt);
                            var files = Directory.GetFiles(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\item"), imageWithNoExt + ".*");
                            if (files.Length > 0)
                            {
                                File.Delete(files[0]);
                            }

                            //Userimage myfolder name where i want to save my image
                            var filePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\item"), imageName);
                            postedFile.SaveAs(filePath);

                        }
                    }
                    var message1 = string.Format("Image Updated Successfully.");
                    return Ok(message1);
                }
                var res = string.Format("Please Upload a image.");

                return Ok(res);
            }
            catch (Exception ex)
            {
                var res = string.Format("some Message");

                return Ok(res);
            }
        }

        [HttpGet]
        [Route("GetImage")]
        public HttpResponseMessage GetImage(string imageName)
        {
            if (String.IsNullOrEmpty(imageName))
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            string localFilePath;

            localFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\item"), imageName);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = imageName;

            return response;
        }


        // get all items where defaultSale is 1 and set isNew=1 if new item  and set isOffer=1 if Has Active Offer 
        int newdays = -15;

    }
}