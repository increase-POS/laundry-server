﻿using Newtonsoft.Json;
using POS_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using POS_Server.Models.VM;
using System.Security.Claims;
using System.Web;
using Newtonsoft.Json.Converters;
using System.Data.Entity.SqlServer;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/ItemsUnits")]
    public class ItemsUnitsController : ApiController
    {
        List<int> itemUnitsIds = new List<int>();
        private Classes.Calculate Calc = new Classes.Calculate();

        [HttpPost]
        [Route("Get")]
        public string Get(string token)
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
                        itemId = int.Parse(c.Value);
                }
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemUnitsList = (from IU in entity.itemsUnits
                                             where (IU.itemId == itemId && IU.isActive == 1)
                                             join u in entity.units on IU.unitId equals u.unitId into lj
                                             from v in lj.DefaultIfEmpty()
                                             join u1 in entity.units on IU.subUnitId equals u1.unitId into tj
                                             from v1 in tj.DefaultIfEmpty()
                                             select new ItemUnitModel()
                                             {
                                                 itemUnitId = IU.itemUnitId,
                                                 unitId = IU.unitId,
                                                 mainUnit = v.name,
                                                 createDate = IU.createDate,
                                                 createUserId = IU.createUserId,
                                                 defaultPurchase = IU.defaultPurchase,
                                                 defaultSale = IU.defaultSale,
                                                 price = IU.price,
                                                 priceWithService = IU.priceWithService,
                                                 subUnitId = IU.subUnitId,
                                                 smallUnit = v1.name,
                                                 unitValue = IU.unitValue,
                                                 barcode = IU.barcode,
                                                 updateDate = IU.updateDate,
                                                 updateUserId = IU.updateUserId,
                                                 storageCostId = IU.storageCostId,

                                             }).ToList();
                        return TokenManager.GenerateToken(itemUnitsList);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        [HttpPost]
        [Route("GetIU")]
        public string GetIU(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemUnitsList = (from IU in entity.itemsUnits
                                             join u in entity.units on IU.unitId equals u.unitId into lj
                                             from v in lj.DefaultIfEmpty()
                                             join I in entity.items.Where(x => x.isActive == 1) on IU.itemId equals I.itemId
                                             join u1 in entity.units on IU.subUnitId equals u1.unitId into tj
                                             from v1 in tj.DefaultIfEmpty()
                                             select new ItemUnitModel()
                                             {
                                                 itemUnitId = IU.itemUnitId,
                                                 unitId = IU.unitId,
                                                 unitName = v.name,
                                                 itemId = IU.itemId,
                                                 itemName = I.name,
                                                 unitValue = IU.unitValue,
                                                 createDate = IU.createDate,
                                                 createUserId = IU.createUserId,
                                                 defaultPurchase = IU.defaultPurchase,
                                                 defaultSale = IU.defaultSale,
                                                 price = IU.price,
                                                 priceWithService = IU.priceWithService,
                                                 subUnitId = IU.subUnitId,
                                                 barcode = IU.barcode,
                                                 updateDate = IU.updateDate,
                                                 updateUserId = IU.updateUserId,
                                                 storageCostId = IU.storageCostId,
                                                 purchasePrice = IU.purchasePrice,
                                                 isActive = IU.isActive,
                                                 type = I.type,
                                                 categoryId = I.categoryId,
                                                 smallUnit = v1.name,
                                                 countSmallUnit = IU.unitValue + " " + v1.name,
                                             }).OrderBy(x => x.itemName).ToList();
                        return TokenManager.GenerateToken(itemUnitsList);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }


        [HttpPost]
        [Route("GetById")]
        public string GetById(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int itemUnitId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);
                    }
                }
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemUnitsList = (from IU in entity.itemsUnits
                                             where (IU.itemUnitId == itemUnitId)
                                             join u in entity.units on IU.unitId equals u.unitId into lj
                                             from v in lj.DefaultIfEmpty()
                                             join u1 in entity.units on IU.subUnitId equals u1.unitId into tj
                                             from v1 in tj.DefaultIfEmpty()
                                             select new ItemUnitModel()
                                             {
                                                 itemUnitId = IU.itemUnitId,
                                                 unitId = IU.unitId,
                                                 itemId = IU.itemId,
                                                 createDate = IU.createDate,
                                                 createUserId = IU.createUserId,
                                                 defaultPurchase = IU.defaultPurchase,
                                                 defaultSale = IU.defaultSale,
                                                 price = IU.price,
                                                 priceWithService = IU.priceWithService,
                                                 subUnitId = IU.subUnitId,
                                                 unitValue = IU.unitValue,
                                                 barcode = IU.barcode,
                                                 updateDate = IU.updateDate,
                                                 updateUserId = IU.updateUserId,
                                                 storageCostId = IU.storageCostId,
                                                 isActive = IU.isActive,
                                             }).FirstOrDefault();
                        return TokenManager.GenerateToken(itemUnitsList);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }    
        }

        [HttpPost]
        [Route("GetAll")]
        public string GetAll(string token)
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

                bool canDelete = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                        itemId = int.Parse(c.Value);
                }

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemUnitsList = (from IU in entity.itemsUnits
                                         join u in entity.units on IU.unitId equals u.unitId into lj
                                         from v in lj.DefaultIfEmpty()
                                         join I in entity.items.Where(x => x.isActive == 1) on IU.itemId equals I.itemId
                                         join u1 in entity.units on IU.subUnitId equals u1.unitId into tj
                                         from v1 in tj.DefaultIfEmpty()
                                         select new ItemUnitModel()
                                         {
                                             itemUnitId = IU.itemUnitId,
                                             unitId = IU.unitId,
                                             mainUnit = v.name,
                                             createDate = IU.createDate,
                                             createUserId = IU.createUserId,
                                             defaultPurchase = IU.defaultPurchase,
                                             defaultSale = IU.defaultSale,
                                             price = IU.price,
                                             priceWithService = IU.priceWithService,
                                             subUnitId = IU.subUnitId,
                                             smallUnit = v1.name,
                                             countSmallUnit = IU.unitValue+ " " + v1.name,
                                             unitValue = IU.unitValue,
                                             barcode = IU.barcode,
                                             updateDate = IU.updateDate,
                                             updateUserId = IU.updateUserId,
                                             storageCostId = IU.storageCostId,
                                             isActive = IU.isActive,
                                         })
                                         .ToList();
                    foreach (ItemUnitModel um in itemUnitsList)
                    {
                        canDelete = false;
                        if (um.isActive == 1)
                        {
                            var purItem = entity.itemsTransfer.Where(x => x.itemUnitId == um.itemUnitId).Select(b => new { b.itemsTransId, b.itemUnitId }).FirstOrDefault();
                            var packages = entity.packages.Where(x => x.childIUId == um.itemUnitId || x.packageId == um.itemUnitId).Select(x => new { x.packageId, x.parentIUId }).FirstOrDefault();
                            if (purItem == null && packages == null)
                                canDelete = true;
                        }
                        um.canDelete = canDelete;
                    }

                    return TokenManager.GenerateToken(itemUnitsList);

                }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }               
            }
        }
        // add or update item unit
        [HttpPost]
        [Route("Save")]
        public string Save(string token)
        {
            string message = "";
            token = TokenManager.readToken(HttpContext.Current.Request); 
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string Object = "";
                itemsUnits newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<itemsUnits>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {
                   message = saveItemUnit(newObject);
                }
                else
                {
                    message = "0";
                }
            }
            return TokenManager.GenerateToken(message);
        }
        public string saveItemUnit( itemsUnits newObject)
        {
            string message = "";
            if (newObject.updateUserId == 0 || newObject.updateUserId == null)
            {
                Nullable<int> id = null;
                newObject.updateUserId = id;
            }
            if (newObject.createUserId == 0 || newObject.createUserId == null)
            {
                Nullable<int> id = null;
                newObject.createUserId = id;
            }
            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemUnitEntity = entity.Set<itemsUnits>();
                    if (newObject.itemUnitId == 0)
                    {
                        var iu = entity.itemsUnits.Where(x => x.itemId == newObject.itemId).FirstOrDefault();
                        if (iu == null)
                        {
                            newObject.defaultPurchase = 1;
                        }
                        else
                        {
                            if (newObject.defaultPurchase == 1)
                            {
                                var defItemUnit = entity.itemsUnits.Where(p => p.itemId == newObject.itemId && p.defaultPurchase == 1).FirstOrDefault();
                                if (defItemUnit != null)
                                {
                                    defItemUnit.defaultPurchase = 0;
                                    entity.SaveChanges();
                                }
                            }
                        }
                        newObject.createDate = DateTime.Now;
                        newObject.updateDate = DateTime.Now;
                        newObject.updateUserId = newObject.createUserId;
                        newObject.isActive = 1;

                        itemUnitEntity.Add(newObject);
                        message = entity.SaveChanges().ToString();
                   int res= SaveIUSbyitemUnitId(newObject);
                        if (res==0)
                        {
                            message = "0";
                        }
                    }
                    else
                    {
                        //update
                        // set the other default sale or purchase to 0 if the new object.default is 1
                        int itemUnitId = newObject.itemUnitId;
                        var tmpItemUnit = entity.itemsUnits.Find(itemUnitId);

                        if (newObject.defaultPurchase == 1)
                        {
                            var defItemUnit = entity.itemsUnits.Where(p => p.itemId == tmpItemUnit.itemId && p.defaultPurchase == 1).FirstOrDefault();
                            if (defItemUnit != null)
                            {
                                defItemUnit.defaultPurchase = 0;
                                entity.SaveChanges();
                            }
                        }
                        tmpItemUnit.barcode = newObject.barcode;
                        tmpItemUnit.price = newObject.price;
                        tmpItemUnit.priceWithService = newObject.priceWithService;
                        tmpItemUnit.subUnitId = newObject.subUnitId;
                        tmpItemUnit.unitId = newObject.unitId;
                        tmpItemUnit.unitValue = newObject.unitValue;
                        tmpItemUnit.defaultPurchase = newObject.defaultPurchase;
                        tmpItemUnit.updateDate = DateTime.Now;
                        tmpItemUnit.updateUserId = newObject.updateUserId;
                        tmpItemUnit.storageCostId = newObject.storageCostId;
                        tmpItemUnit.isActive = newObject.isActive;
                        message = entity.SaveChanges().ToString();
                    }
                
                }
            }
            catch
            {
                message = "0";
            }
            return message;
        }

        [HttpPost]
        [Route("Delete")]
        public string Delete(string token)
        {
            string message = "";
            token = TokenManager.readToken(HttpContext.Current.Request); 
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int ItemUnitId = 0;
                int userId = 0;
                bool final = false;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "ItemUnitId")
                    {
                        ItemUnitId = int.Parse(c.Value);
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
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        if (final)
                        {
                            itemsUnits itemUnit = entity.itemsUnits.Find(ItemUnitId);

                            entity.itemsUnits.Remove(itemUnit);
                            message = entity.SaveChanges().ToString();
                            return TokenManager.GenerateToken(message);
                        }
                        else
                        {
                            itemsUnits unitDelete = entity.itemsUnits.Find(ItemUnitId);
                            unitDelete.isActive = 0;
                            unitDelete.updateDate = DateTime.Now;
                            unitDelete.updateUserId = userId;

                            message = entity.SaveChanges().ToString();
                            return TokenManager.GenerateToken(message);
                        }
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }      

        [HttpPost]
        [Route("GetallItemsUnits")]
        public string GetallItemsUnits(string token)
        {
            //  public string Getall(string token)
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemUnitsList = (from IU in entity.itemsUnits

                                             join u in entity.units on IU.unitId equals u.unitId where u.isActive==1

                                             join i in entity.items on IU.itemId equals i.itemId
                                             orderby i.name
                                             select new ItemUnitModel()
                                             {
                                                 itemUnitId = IU.itemUnitId,
                                                 unitId = IU.unitId,
                                                 itemId = IU.itemId,
                                                 mainUnit = u.name,
                                                 createDate = IU.createDate,
                                                 createUserId = IU.createUserId,
                                                 defaultPurchase = IU.defaultPurchase,
                                                 defaultSale = IU.defaultSale,
                                                 price = IU.price,
                                                 priceWithService = IU.priceWithService,
                                                 subUnitId = IU.subUnitId,
                                                 unitValue = IU.unitValue,
                                                 barcode = IU.barcode,
                                                 updateDate = IU.updateDate,
                                                 updateUserId = IU.updateUserId,
                                                 itemName = i.name,
                                                 itemCode = i.code,
                                                 unitName = u.name,
                                                 storageCostId = IU.storageCostId,
                                                 isActive=IU.isActive,

                                             }).ToList();
                        return TokenManager.GenerateToken(itemUnitsList);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }


        [HttpPost]
        [Route("getSmallItemUnits")]
        public string getSmallItemUnits(string token)
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
                int itemUnitId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        itemId = int.Parse(c.Value);
                    }
                    else if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        // get all sub item units 
                        List<itemsUnits> unitsList = entity.itemsUnits
                         .ToList().Where(x => x.itemId == itemId)
                          .Select(p => new itemsUnits
                          {
                              itemUnitId = p.itemUnitId,
                              unitId = p.unitId,
                              subUnitId = p.subUnitId,
                          })
                         .ToList();

                        var unitId = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => x.unitId).Single();
                        itemUnitsIds = new List<int>();
                        itemUnitsIds.Add(itemUnitId);

                        var result = Recursive(unitsList, (int)unitId);

                        var units = (from iu in entity.itemsUnits.Where(x => x.itemId == itemId && x.isActive == 1)
                                     join u in entity.units on iu.unitId equals u.unitId
                                     select new ItemUnitModel()
                                     {
                                         unitId = iu.unitId,
                                         itemUnitId = iu.itemUnitId,
                                         subUnitId = iu.subUnitId,
                                         mainUnit = u.name,

                                     }).Where(p => !itemUnitsIds.Contains((int)p.itemUnitId)).ToList();

                        return TokenManager.GenerateToken(units);
                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        public IEnumerable<itemsUnits> Recursive(List<itemsUnits> unitsList, int smallLevelid)
        {
            List<itemsUnits> inner = new List<itemsUnits>();

            foreach (var t in unitsList.Where(item => item.subUnitId == smallLevelid && item.unitId != smallLevelid))
            {

                itemUnitsIds.Add(t.itemUnitId);
                inner.Add(t);

                if (t.unitId.Value == smallLevelid)
                    return inner;
                inner = inner.Union(Recursive(unitsList, t.unitId.Value)).ToList();
            }

            return inner;
        }

        [HttpPost]
        [Route("largeToSmallUnitQuan")]
        public string largeToSmallUnitQuan(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int fromItemUnit = 0;
                int toItemUnit = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "fromItemUnit")
                    {
                        fromItemUnit = int.Parse(c.Value);
                    }
                    else if (c.Type == "toItemUnit")
                    {
                        toItemUnit = int.Parse(c.Value);

                    }
                }
                try
                {
                    int amount = 0;
                    amount += getUnitConversionQuan(fromItemUnit, toItemUnit);
                    return TokenManager.GenerateToken(amount.ToString());
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
        private int getUnitConversionQuan(int fromItemUnit, int toItemUnit)
        {
            int amount = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var unit = entity.itemsUnits.Where(x => x.itemUnitId == toItemUnit).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId && x.isActive == 1).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();
                if (upperUnit != null)
                    amount = (int)upperUnit.unitValue;
                if (fromItemUnit == upperUnit.itemUnitId)
                    return amount;
                if (upperUnit != null)
                    amount += (int)upperUnit.unitValue * getUnitConversionQuan(fromItemUnit, upperUnit.itemUnitId);

                return amount;
            }
        }


        [HttpPost]
        [Route("smallToLargeUnitQuan")]
        public string smallToLargeUnitQuan(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int fromItemUnit = 0;
                int toItemUnit = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "fromItemUnit")
                    {
                        fromItemUnit = int.Parse(c.Value);
                    }
                    else if (c.Type == "toItemUnit")
                    {
                        toItemUnit = int.Parse(c.Value);

                    }
                }
                try
                {
                    int amount = 0;
                    amount = getLargeUnitConversionQuan(fromItemUnit, toItemUnit);
                    return TokenManager.GenerateToken(amount.ToString());
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        public int getLargeUnitConversionQuan(int fromItemUnit, int toItemUnit)
        {
            int amount = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var unit = entity.itemsUnits.Where(x => x.itemUnitId == toItemUnit).Select(x => new { x.unitId, x.itemId, x.subUnitId, x.unitValue }).FirstOrDefault();
                var smallUnit = entity.itemsUnits.Where(x => x.unitId == unit.subUnitId && x.itemId == unit.itemId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (toItemUnit == smallUnit.itemUnitId)
                {
                    amount = 1;
                    return amount;
                }
                if (smallUnit != null)
                    amount += (int)unit.unitValue * getLargeUnitConversionQuan(fromItemUnit, smallUnit.itemUnitId);

                return amount;
            }
        }

        public int SaveIUSbyitemUnitId(itemsUnits tmpObject)
        {
            int res = 0;
            using (incposdbEntities entity = new incposdbEntities())
            {
                //save itemsUnits
                var SList= entity.services.Select(X => new ServicesModel { serviceId= X.serviceId, cost = X.cost }).ToList();
                foreach (ServicesModel S in SList)
                {
                    ItemsUnitsServices newIUS = new ItemsUnitsServices();
                    newIUS.createUserId = tmpObject.createUserId;
                    newIUS.updateUserId = tmpObject.createUserId;
                    newIUS.serviceId = S.serviceId;
                    newIUS.itemUnitId = tmpObject.itemUnitId;
                    newIUS.cost = S.cost;
                    ItemsUnitsServicesController iuscntrlr = new ItemsUnitsServicesController();
                    res = iuscntrlr.Save(newIUS);
                    if (res == 0)
                    {
                        return 0;
                    }

                }
                // 
            }
            return res;
        }
    }
}