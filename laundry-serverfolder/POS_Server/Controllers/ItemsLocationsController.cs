using Newtonsoft.Json;
using POS_Server.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using POS_Server.Models.VM;
using System.Security.Claims;
using System.Web;

using Newtonsoft.Json.Converters;
using LinqKit;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/ItemsLocations")]
    public class ItemsLocationsController : ApiController
    {
        ItemsUnitsController itemsUnitsController = new ItemsUnitsController();
        GroupObjectController group = new GroupObjectController();
        NotificationController notificationController = new NotificationController();
        notificationUserController notUserController = new notificationUserController();

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
                int branchId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var docImageList = (from b in entity.itemsLocations
                                            where b.quantity > 0 && b.invoiceId == null
                                            join u in entity.itemsUnits on b.itemUnitId equals u.itemUnitId
                                            join i in entity.items on u.itemId equals i.itemId
                                            join l in entity.locations on b.locationId equals l.locationId
                                            join s in entity.sections on l.sectionId equals s.sectionId
                                            where s.branchId == branchId && s.isFreeZone != 1 && s.isKitchen != 1

                                            select new ItemLocationModel
                                            {
                                                createDate = b.createDate,
                                                createUserId = b.createUserId,
                                                endDate = b.endDate,
                                                itemsLocId = b.itemsLocId,
                                                itemUnitId = b.itemUnitId,
                                                locationId = b.locationId,
                                                notes = b.notes,
                                                quantity = b.quantity,
                                                startDate = b.startDate,

                                                updateDate = b.updateDate,
                                                updateUserId = b.updateUserId,
                                                itemName = i.name,
                                                location = l.x + l.y + l.z,
                                                section = s.name,
                                                sectionId = s.sectionId,
                                                itemType = i.type,
                                                unitName = u.units.name,
                                                invoiceId = b.invoiceId,
                                            }).ToList().OrderBy(x => x.location).ToList();


                        return TokenManager.GenerateToken(docImageList);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        [HttpPost]
        [Route("GetItemsHasQuantity")]
        public string GetItemsHasQuantity(string token)
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

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemsList = (from b in entity.itemsLocations
                                            where b.quantity > 0 && b.invoiceId == null 
                                            join u in entity.itemsUnits on b.itemUnitId equals u.itemUnitId
                                            join I in entity.items on u.itemId equals I.itemId
                                            join l in entity.locations on b.locationId equals l.locationId
                                            join s in entity.sections on l.sectionId equals s.sectionId
                                            where s.branchId == branchId && s.isKitchen != 1

                                            select new ItemModel()
                                            {
                                                itemId = I.itemId,
                                                name = I.name,
                                                type = I.type,
                                                isActive = I.isActive,
                                                avgPurchasePrice = I.avgPurchasePrice,
                                            }).Where(x => x.isActive == 1).Distinct().ToList();


                        return TokenManager.GenerateToken(itemsList);
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
                int branchId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var docImageList = (from b in entity.itemsLocations
                                            where b.quantity > 0 && b.invoiceId == null
                                            join u in entity.itemsUnits on b.itemUnitId equals u.itemUnitId
                                            join i in entity.items on u.itemId equals i.itemId
                                            join l in entity.locations on b.locationId equals l.locationId
                                            join s in entity.sections on l.sectionId equals s.sectionId
                                            where s.branchId == branchId && s.isKitchen != 1

                                            select new ItemLocationModel
                                            {
                                                createDate = b.createDate,
                                                createUserId = b.createUserId,
                                                endDate = b.endDate,
                                                itemsLocId = b.itemsLocId,
                                                itemUnitId = b.itemUnitId,
                                                locationId = b.locationId,
                                                notes = b.notes,
                                                quantity = b.quantity,
                                                startDate = b.startDate,

                                                updateDate = b.updateDate,
                                                updateUserId = b.updateUserId,
                                                itemName = i.name,
                                                location = l.x + l.y + l.z,
                                                section = s.name,
                                                sectionId = s.sectionId,
                                                itemType = i.type,
                                                unitName = u.units.name,
                                                invoiceId = b.invoiceId,
                                            }).ToList().OrderBy(x => x.location).ToList();

                        return TokenManager.GenerateToken(docImageList);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
             
        [HttpPost]
        [Route("GetFreeZoneItems")]
        public string GetFreeZoneItems(string token)
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
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var docImageList = (from b in entity.itemsLocations
                                            where b.quantity > 0 && b.invoiceId == null
                                            join u in entity.itemsUnits on b.itemUnitId equals u.itemUnitId
                                            join i in entity.items on u.itemId equals i.itemId
                                            join l in entity.locations on b.locationId equals l.locationId
                                            join s in entity.sections on l.sectionId equals s.sectionId
                                            where s.branchId == branchId && s.isFreeZone == 1 && s.isKitchen != 1

                                            select new ItemLocationModel
                                            {
                                                createDate = b.createDate,
                                                createUserId = b.createUserId,
                                                endDate = b.endDate,
                                                itemsLocId = b.itemsLocId,
                                                itemUnitId = b.itemUnitId,
                                                locationId = b.locationId,
                                                notes = b.notes,
                                                quantity = b.quantity,
                                                startDate = b.startDate,

                                                updateDate = b.updateDate,
                                                updateUserId = b.updateUserId,
                                                itemName = i.name,
                                                sectionId = s.sectionId,
                                                isFreeZone = s.isFreeZone,
                                                itemType = i.type,
                                                location = l.x + l.y + l.z,
                                                section = s.name,
                                                unitName = u.units.name,
                                            })
                                        .ToList();


                        return TokenManager.GenerateToken(docImageList);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }          
        }      
       
        [HttpPost]
        [Route("receiptInvoice")]
        public string receiptInvoice(string token)
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
                int branchId = 0;
                int userId = 0;
                string objectName = "";
                string notificationObj = "";

                List<itemsTransfer> newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "branchId")
                        branchId = int.Parse(c.Value);
                    else if (c.Type == "userId")
                        userId = int.Parse(c.Value);
                    else if (c.Type == "objectName")
                        objectName = c.Value;
                    else if (c.Type == "notificationObj")
                        notificationObj = c.Value;
                }

                if (newObject != null)
                {
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                        {
                            var freeZoneLocation = (from s in entity.sections.Where(x => x.branchId == branchId && x.isFreeZone == 1 && x.isKitchen != 1)
                                                    join l in entity.locations on s.sectionId equals l.sectionId
                                                    select l.locationId).SingleOrDefault();
                            foreach (itemsTransfer item in newObject)
                            {
                                var itemId = entity.itemsUnits.Where(x => x.itemUnitId == item.itemUnitId).Select(x => x.itemId).Single();
                                var itemV = entity.items.Find(itemId);
                           
                                if (item.invoiceId == 0 || item.invoiceId == null)
                                    increaseItemQuantity(item.itemUnitId.Value, freeZoneLocation, (int)item.quantity, userId);
                                else//for order
                                    increaseLockedItem(item.itemUnitId.Value, freeZoneLocation, (int)item.quantity, (int)item.invoiceId, userId);
                                #region should upgrade
                                //if(item.offerId != 0 && item.offerId != null)
                                //{
                                //    int offerId = (int)item.offerId;
                                //    int itemUnitId = (int)item.itemUnitId;
                                //    var offer = entity.itemsOffers.Where(x => x.iuId == itemUnitId && x.offerId == offerId).FirstOrDefault();
                                //    offer.used -= (int)item.quantity;
                                //    entity.SaveChanges();
                                //}
                                #endregion
                                bool isExcedded = isExceddMaxQuantity((int)item.itemUnitId, branchId, userId);
                                if (isExcedded == true) //add notification
                                {
                                    notificationController.addNotifications(objectName, notificationObj, branchId, itemV.name);
                                }
                            }

                        }
                        return TokenManager.GenerateToken("1");
            }
                catch
            {
                message = "0";
                return TokenManager.GenerateToken(message);
            }
        }
                else
                {
                    return TokenManager.GenerateToken("0");
                }
        }
    }

        public bool isExceddMaxQuantity(int itemUnitId, int branchId, int userId)
        {
            bool isExcedded = false;
            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemId = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => x.itemId).Single();
                    var item = entity.items.Find(itemId);
                    int maxUnitId = (int)item.maxUnitId;
                    int maxQuantity = (int)item.max;
                    if (maxQuantity == 0)
                        return false;
                    var maxUnit = entity.itemsUnits.Where(x => x.itemId == itemId && x.unitId == maxUnitId).FirstOrDefault();
                    if (maxUnit == null)
                        isExcedded = false;
                    else
                    {
                        int itemUnitQuantity = getItemAmount(maxUnit.itemUnitId, branchId);
                        if (itemUnitQuantity >= maxQuantity)
                        {
                            isExcedded = true;
                        }
                        if (isExcedded == false)
                        {
                            int smallestItemUnit = entity.itemsUnits.Where(x => x.itemId == itemId && x.subUnitId == x.unitId).Select(x => x.itemUnitId).Single();
                            int smallUnitQuantity = getLevelItemUnitAmount(smallestItemUnit, maxUnit.itemUnitId, branchId);
                            int unitValue = itemsUnitsController.getLargeUnitConversionQuan(smallestItemUnit, maxUnit.itemUnitId);
                            int quantity = 0;
                            if (unitValue != 0)
                                quantity = smallUnitQuantity / unitValue;

                            quantity += itemUnitQuantity;
                            if (quantity >= maxQuantity)
                            {
                                isExcedded = true;
                            }
                        }

                    }
                }
            }
            catch
            {
            }
            return isExcedded;
        }
            
        [HttpPost]
        [Route("receiptOrder")]
        public string receiptOrder(string token)
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
                string orderList = "";

                int toBranch = 0;
                int userId = 0;
                string objectName = "";
                string notificationObj = "";

                List<itemsLocations> newObject = new List<itemsLocations>();
                List<itemsTransfer> items = new List<itemsTransfer>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<itemsLocations>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "orderList")
                    {
                        orderList = c.Value.Replace("\\", string.Empty);
                        orderList = orderList.Trim('"');
                        items = JsonConvert.DeserializeObject<List<itemsTransfer>>(orderList, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "toBranch")
                    {
                        toBranch = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "objectName")
                    {
                        objectName = c.Value;
                    }
                    else if (c.Type == "notificationObj")
                    {
                        notificationObj = c.Value;
                    }

                }

                if (newObject != null)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var freeZoneLocation = (from s in entity.sections.Where(x => x.branchId == toBranch && x.isFreeZone == 1 && x.isKitchen != 1)
                                                    join l in entity.locations on s.sectionId equals l.sectionId
                                                    select l.locationId).SingleOrDefault();
                            foreach (itemsLocations item in newObject)
                            {
                                itemsLocations itemL = new itemsLocations();

                                itemL = entity.itemsLocations.Find(item.itemsLocId);
                                itemL.quantity -= item.quantity;
                                itemL.updateDate = DateTime.Now;
                                itemL.updateUserId = userId;
                                entity.SaveChanges();

                                var itemId = entity.itemsUnits.Where(x => x.itemUnitId == item.itemUnitId).Select(x => x.itemId).Single();

                                var itemV = entity.items.Find(itemId);
                                int quantity = (int)item.quantity;
                                foreach (itemsTransfer it in items)
                                {
                                    if (it.itemUnitId == item.itemUnitId && it.invoiceId != 0 && it.invoiceId != null)//for order
                                    {
                                        int itemQuantity = 0;
                                        if (quantity >= item.quantity)
                                        {
                                            itemQuantity = (int)item.quantity;
                                            quantity -= (int)item.quantity;
                                            item.quantity = quantity;
                                            it.quantity = 0;
                                        }
                                        else
                                        {
                                            itemQuantity = quantity;
                                            quantity = 0;
                                            it.quantity -= quantity;
                                        }
                                        increaseLockedItem(item.itemUnitId.Value, freeZoneLocation, itemQuantity, (int)it.invoiceId, userId);
                                    }
                                }
                                if (quantity != 0)
                                    increaseItemQuantity(item.itemUnitId.Value, freeZoneLocation, quantity, userId);

                                bool isExcedded = isExceddMaxQuantity((int)item.itemUnitId, toBranch, userId);
                                if (isExcedded == true) //add notification
                                {
                                    notificationController.addNotifications(objectName, notificationObj, toBranch, itemV.name);
                                }
                            }
                            return TokenManager.GenerateToken("1");
                        }
                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("0");
                }
            }          
        }

        [HttpPost]
        [Route("transferToKitchen")]
        public string transferToKitchen(string token)
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
                string orderList = "";

                int branchId = 0;
                int userId = 0;

                List<itemsLocations> newObject = new List<itemsLocations>();
                List<itemsTransfer> items = new List<itemsTransfer>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<itemsLocations>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "orderList")
                    {
                        orderList = c.Value.Replace("\\", string.Empty);
                        orderList = orderList.Trim('"');
                        items = JsonConvert.DeserializeObject<List<itemsTransfer>>(orderList, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }

                if (newObject != null)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var kitchenLocation = (from s in entity.sections.Where(x => x.branchId == branchId &&  x.isKitchen == 1)
                                                    join l in entity.locations on s.sectionId equals l.sectionId
                                                    select l.locationId).SingleOrDefault();
                            foreach (itemsLocations item in newObject)
                            {
                                itemsLocations itemL = new itemsLocations();

                                itemL = entity.itemsLocations.Find(item.itemsLocId);
                                itemL.quantity -= item.quantity;
                                itemL.updateDate = DateTime.Now;
                                itemL.updateUserId = userId;
                                entity.SaveChanges();

                                var itemId = entity.itemsUnits.Where(x => x.itemUnitId == item.itemUnitId).Select(x => x.itemId).Single();

                                var itemV = entity.items.Find(itemId);
                                int quantity = (int)item.quantity;
                               
                                if (quantity != 0)
                                    increaseItemQuantity(item.itemUnitId.Value, kitchenLocation, quantity, userId);
                            }
                            return TokenManager.GenerateToken("1");
                        }
                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        [HttpPost]
        [Route("transferAmountbetweenUnits")]
        public string transferAmountbetweenUnits(string token)
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
                int locationId = 0;
                int itemLocId = 0;
                int toItemUnitId = 0;
                int fromQuantity = 0;
                int toQuantity = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "locationId")
                    {
                        locationId = int.Parse(c.Value);

                    }
                    else if (c.Type == "itemLocId")
                    {
                        itemLocId = int.Parse(c.Value);

                    }
                    else if (c.Type == "toItemUnitId")
                    {
                        toItemUnitId = int.Parse(c.Value);

                    }
                    else if (c.Type == "fromQuantity")
                    {
                        fromQuantity = int.Parse(c.Value);

                    }
                    else if (c.Type == "toQuantity")
                    {
                        toQuantity = int.Parse(c.Value);

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        decreaseItemLocationQuantity(itemLocId, fromQuantity, userId, "", "");
                        increaseItemQuantity(toItemUnitId, locationId, toQuantity, userId);
                    }
                    //  return Ok(1);
                    return TokenManager.GenerateToken("1");
                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }

            }
        }
        private void increaseItemQuantity(int itemUnitId, int locationId, int quantity, int userId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemUnit = (from il in entity.itemsLocations
                                where il.itemUnitId == itemUnitId && il.locationId == locationId && il.invoiceId == null
                                select new { il.itemsLocId }
                                ).FirstOrDefault();
                itemsLocations itemL = new itemsLocations();
                if (itemUnit == null)//add item in new location
                {
                    itemL.itemUnitId = itemUnitId;
                    itemL.locationId = locationId;
                    itemL.quantity = quantity;
                    itemL.createDate = DateTime.Now;
                    itemL.updateDate = DateTime.Now;
                    itemL.createUserId = userId;
                    itemL.updateUserId = userId;

                    entity.itemsLocations.Add(itemL);
                }
                else
                {
                    itemL = entity.itemsLocations.Find(itemUnit.itemsLocId);
                    itemL.quantity += quantity;
                    itemL.updateDate = DateTime.Now;
                    itemL.updateUserId = userId;
                }
                entity.SaveChanges();
            }
        }


        [HttpPost]
        [Route("trasnferItem")]
        public string trasnferItem(string token)
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
                int itemLocId = 0;

                ItemLocationModel newObject = new ItemLocationModel();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemLocId")
                    {
                        itemLocId = int.Parse(c.Value);

                    }

                    else if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<ItemLocationModel>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                }

                if (newObject != null)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var oldItemL = entity.itemsLocations.Find(itemLocId);
                            int userId = (int)newObject.updateUserId;
                            long newQuantity = (long)oldItemL.quantity - (long)newObject.quantity;
                            oldItemL.quantity = (long)newQuantity;
                            oldItemL.updateDate = DateTime.Now;
                            oldItemL.updateUserId = userId;


                            var newtemLocation = (from il in entity.itemsLocations
                                                  where il.itemUnitId == newObject.itemUnitId && il.locationId == newObject.locationId
                                                  && il.startDate == newObject.startDate && il.endDate == newObject.endDate && il.invoiceId == newObject.invoiceId && il.locations.isKitchen != 1
                                                  select new { il.itemsLocId }
                                           ).FirstOrDefault();

                            itemsLocations newItemL;
                            if (newtemLocation == null)//add item in new location
                            {
                                newItemL = new itemsLocations();
                                newItemL.createDate = DateTime.Now;
                                newItemL.createUserId = (int)newObject.createUserId;
                                if (newObject.endDate != null)
                                    newItemL.endDate = newObject.endDate;
                                if (newObject.startDate != null)
                                    newItemL.startDate = newObject.startDate;
                                newItemL.updateDate = DateTime.Now;
                                newItemL.updateUserId = (int)newObject.createUserId;
                                newItemL.itemUnitId = (int)newObject.itemUnitId;
                                newItemL.locationId = (int)newObject.locationId;
                                newItemL.notes = newObject.notes;
                                newItemL.quantity = (long)newObject.quantity;
                                newItemL.invoiceId = newObject.invoiceId;
                                entity.itemsLocations.Add(newItemL);
                            }
                            else
                            {
                                newItemL = new itemsLocations();
                                newItemL = entity.itemsLocations.Find(newtemLocation.itemsLocId);
                                newQuantity = (long)newItemL.quantity + (long)newObject.quantity;
                                newItemL.quantity = (long)newQuantity;
                                newItemL.updateDate = DateTime.Now;
                                newItemL.updateUserId = (int)newObject.updateUserId;

                            }
                            message = entity.SaveChanges().ToString();
                            return TokenManager.GenerateToken(message);
                        }
                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("0");
                }

            }

            
        }      

        public int updateItemQuantity(int itemUnitId, int branchId, int requiredAmount, int userId, int isKitchen = 0)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            using (incposdbEntities entity = new incposdbEntities())
            {
                var searchPredicate = PredicateBuilder.New<sections>();
                searchPredicate = searchPredicate.And(x => x.isKitchen == isKitchen);
                var itemInLocs = (from b in entity.branches
                                  where b.branchId == branchId
                                  join s in entity.sections.Where(searchPredicate) on b.branchId equals s.branchId
                                  join l in entity.locations on s.sectionId equals l.sectionId
                                  join il in entity.itemsLocations on l.locationId equals il.locationId
                                  where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null 
                                  select new
                                  {
                                      il.itemsLocId,
                                      il.quantity,
                                      il.itemUnitId,
                                      il.locationId,
                                      il.updateDate,
                                      s.sectionId,
                                  }).ToList().OrderBy(x => x.updateDate).ToList();
                for (int i = 0; i < itemInLocs.Count; i++)
                {
                    int availableAmount = (int)itemInLocs[i].quantity;
                    var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);
                    itemL.updateDate = DateTime.Now;
                    if (availableAmount >= requiredAmount)
                    {
                        itemL.quantity = availableAmount - requiredAmount;
                        requiredAmount = 0;
                        entity.SaveChanges();
                    }
                    else if (availableAmount > 0)
                    {
                        itemL.quantity = 0;
                        requiredAmount = requiredAmount - availableAmount;
                        entity.SaveChanges();
                    }

                    if (requiredAmount == 0)
                        return (3);
                }
                if (requiredAmount != 0)
                {
                    dic = checkUpperUnit(itemUnitId, branchId, requiredAmount, userId,isKitchen);

                    var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                    var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId && x.isActive == 1).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();


                    if (dic["remainQuantity"] > 0)
                    {
                        var item = (from il in entity.itemsLocations
                                    where il.itemUnitId == itemUnitId && il.invoiceId == null 
                                    join l in entity.locations on il.locationId equals l.locationId
                                    join s in entity.sections.Where(searchPredicate) on l.sectionId equals s.sectionId
                                    where s.branchId == branchId
                                    select new
                                    {
                                        il.itemsLocId,
                                    }).FirstOrDefault();
                        if (item != null)
                        {
                            var itemloc = entity.itemsLocations.Find(item.itemsLocId);
                            itemloc.quantity = dic["remainQuantity"];
                            entity.SaveChanges();
                        }
                        else
                        {
                            var locations = entity.locations.Where(x => x.branchId == branchId && x.isActive == 1 && x.isKitchen == isKitchen).Select(x => new { x.locationId }).OrderBy(x => x.locationId).ToList();
                            // if (locations.Count > 0)
                            // {
                            int locationId = dic["locationId"];
                            if (locationId == 0 && locations.Count > 1)
                                locationId = locations[0].locationId; // free zoon
                            itemsLocations itemL = new itemsLocations();
                            itemL.itemUnitId = itemUnitId;
                            itemL.locationId = locationId;
                            itemL.quantity = dic["remainQuantity"];
                            itemL.createDate = DateTime.Now;
                            itemL.updateDate = DateTime.Now;
                            itemL.createUserId = userId;
                            itemL.updateUserId = userId;

                            entity.itemsLocations.Add(itemL);
                            entity.SaveChanges();
                        }
                    }
                    if (dic["requiredQuantity"] > 0)
                    {
                        checkLowerUnit(itemUnitId, branchId, dic["requiredQuantity"], userId, isKitchen);
                    }

                }
            }
            return (2);

        }

        private Dictionary<string, int> checkUpperUnit(int itemUnitId, int branchId, int requiredAmount, int userId, int isKitchen = 0)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            dic.Add("remainQuantity", 0);
            dic.Add("locationId", 0);
            dic.Add("requiredQuantity", 0);
            dic.Add("isConsumed", 0);
            int remainQuantity = 0;
            int firstRequir = requiredAmount;
            decimal newQuant = 0;
            using (incposdbEntities entity = new incposdbEntities())
            {
                var searchPredicate = PredicateBuilder.New<sections>();
                searchPredicate = searchPredicate.And(x => x.isKitchen == isKitchen);

                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId && x.isActive == 1).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (upperUnit != null)
                {
                    decimal unitValue = (decimal)upperUnit.unitValue;
                    int breakNum = (int)Math.Ceiling(requiredAmount / unitValue);
                    newQuant = (decimal)(breakNum * upperUnit.unitValue);
                    var itemInLocs = (from b in entity.branches
                                      where b.branchId == branchId
                                      join s in entity.sections.Where(searchPredicate) on b.branchId equals s.branchId
                                      join l in entity.locations on s.sectionId equals l.sectionId
                                      join il in entity.itemsLocations on l.locationId equals il.locationId
                                      where il.itemUnitId == upperUnit.itemUnitId && il.quantity > 0 && il.invoiceId == null
                                      select new
                                      {
                                          il.itemsLocId,
                                          il.quantity,
                                          il.itemUnitId,
                                          il.locationId,
                                          il.updateDate,
                                          s.sectionId,
                                      }).ToList().OrderBy(x => x.updateDate).ToList();

                    for (int i = 0; i < itemInLocs.Count; i++)
                    {
                        dic["isConsumed"] = 1;
                        var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);
                        var smallUnitLocId = entity.itemsLocations.Where(x => x.itemUnitId == itemUnitId && x.invoiceId == null && x.locations.isKitchen == isKitchen).
                            Select(x => x.itemsLocId).FirstOrDefault();

                        if (breakNum <= itemInLocs[i].quantity)
                        {
                            itemL.quantity = itemInLocs[i].quantity - breakNum;
                            entity.SaveChanges();
                            remainQuantity = (int)newQuant - firstRequir;
                            requiredAmount = 0;
                            dic["remainQuantity"] = remainQuantity;
                            dic["locationId"] = (int)itemInLocs[i].locationId;
                            dic["requiredQuantity"] = 0;

                            return dic;
                        }
                        else
                        {
                            itemL.quantity = 0;
                            breakNum = (int)(breakNum - itemInLocs[i].quantity);
                            requiredAmount = requiredAmount - ((int)itemInLocs[i].quantity * (int)upperUnit.unitValue);
                            entity.SaveChanges();
                        }
                        if (breakNum == 0)
                            break;
                    }
                    if (breakNum != 0)
                    {
                        dic = new Dictionary<string, int>();
                        dic = checkUpperUnit(upperUnit.itemUnitId, branchId, breakNum, userId,isKitchen);
                        var item = (from s in entity.sections
                                    where s.branchId == branchId
                                    join l in entity.locations on s.sectionId equals l.sectionId
                                    join il in entity.itemsLocations on l.locationId equals il.locationId
                                    where il.itemUnitId == upperUnit.itemUnitId && il.invoiceId == null && il.locations.isKitchen == isKitchen
                                    select new
                                    {
                                        il.itemsLocId,
                                    }).FirstOrDefault();
                        if (item != null)
                        {
                            var itemloc = entity.itemsLocations.Find(item.itemsLocId);
                            itemloc.quantity = dic["remainQuantity"];
                            entity.SaveChanges();
                        }
                        else
                        {
                            var locations = entity.locations.Where(x => x.branchId == branchId && x.isActive == 1 && x.isKitchen == isKitchen).Select(x => new { x.locationId }).OrderBy(x => x.locationId).ToList();

                            int locationId = dic["locationId"];
                            if (locationId == 0 && locations.Count > 1)
                                locationId = locations[0].locationId; // free zoon

                            itemsLocations itemL = new itemsLocations();
                            //itemL.itemUnitId = itemUnitId;
                            itemL.itemUnitId = upperUnit.itemUnitId;
                            itemL.locationId = locationId;
                            itemL.quantity = dic["remainQuantity"];
                            itemL.createDate = DateTime.Now;
                            itemL.updateDate = DateTime.Now;
                            itemL.createUserId = userId;
                            itemL.updateUserId = userId;

                            entity.itemsLocations.Add(itemL);
                            entity.SaveChanges();

                        }

                        ///////////////////
                        if (dic["isConsumed"] == 0)
                        {
                            dic["requiredQuantity"] = requiredAmount;
                            dic["remainQuantity"] = 0;
                        }
                        else
                        {
                            dic["remainQuantity"] = (int)newQuant - firstRequir;
                            dic["requiredQuantity"] = breakNum * (int)upperUnit.unitValue;
                        }
                        return dic;
                    }
                }
                else
                {
                    dic["remainQuantity"] = 0;
                    dic["requiredQuantity"] = requiredAmount;
                    dic["locationId"] = 0;

                    return dic;
                }
            }
            return dic;
        }
        private Dictionary<string, int> checkLowerUnit(int itemUnitId, int branchId, int requiredAmount, int userId, int isKitchen = 0)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            int remainQuantity = 0;
            int firstRequir = requiredAmount;
            decimal newQuant = 0;
            using (incposdbEntities entity = new incposdbEntities())
            {
                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId, x.subUnitId, x.unitValue }).FirstOrDefault();
                var lowerUnit = entity.itemsUnits.Where(x => x.unitId == unit.subUnitId && x.itemId == unit.itemId && x.isActive == 1).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (lowerUnit != null)
                {
                    decimal unitValue = (decimal)unit.unitValue;
                    int breakNum = (int)requiredAmount * (int)unitValue;
                    newQuant = (decimal)Math.Ceiling(breakNum / (decimal)lowerUnit.unitValue);
                    var itemInLocs = (from b in entity.branches
                                      where b.branchId == branchId
                                      join s in entity.sections.Where(x => x.isKitchen == isKitchen) on b.branchId equals s.branchId
                                      join l in entity.locations on s.sectionId equals l.sectionId
                                      join il in entity.itemsLocations on l.locationId equals il.locationId
                                      where il.itemUnitId == lowerUnit.itemUnitId && il.quantity > 0 && il.invoiceId == null 
                                      select new
                                      {
                                          il.itemsLocId,
                                          il.quantity,
                                          il.itemUnitId,
                                          il.locationId,
                                          il.updateDate,
                                          s.sectionId,
                                      }).ToList().OrderBy(x => x.updateDate).ToList();

                    for (int i = 0; i < itemInLocs.Count; i++)
                    {

                        var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);
                        var smallUnitLocId = entity.itemsLocations.Where(x => x.itemUnitId == itemUnitId && x.invoiceId == null && x.locations.isKitchen == isKitchen).
                            Select(x => x.itemsLocId).FirstOrDefault();

                        if (breakNum <= itemInLocs[i].quantity)
                        {
                            itemL.quantity = itemInLocs[i].quantity - breakNum;
                            entity.SaveChanges();
                            remainQuantity = (int)newQuant - firstRequir;
                            requiredAmount = 0;
                            // return remainQuantity;
                            dic.Add("remainQuantity", remainQuantity);
                            dic.Add("locationId", (int)itemInLocs[i].locationId);
                            return dic;
                        }
                        else
                        {
                            itemL.quantity = 0;
                            breakNum = (int)(breakNum - itemInLocs[i].quantity);
                            requiredAmount = requiredAmount - ((int)itemInLocs[i].quantity / (int)unit.unitValue);
                            entity.SaveChanges();
                        }
                        if (breakNum == 0)
                            break;
                    }
                    if (itemUnitId == lowerUnit.itemUnitId)
                        return dic;
                    if (breakNum != 0)
                    {
                        dic = new Dictionary<string, int>();
                        dic = checkLowerUnit(lowerUnit.itemUnitId, branchId, breakNum, userId,isKitchen);
                       
                        dic["remainQuantity"] = (int)newQuant - firstRequir;
                        dic["requiredQuantity"] = breakNum;
                        return dic;
                    }
                }
            }
            return dic;
        }

        [HttpPost]
        [Route("getAmountInBranch")]
        public string getAmountInBranch(string token)
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
                int itemUnitId = 0;
                int branchId = 0;
                int isKitchen = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "isKitchen")
                    {
                        isKitchen = int.Parse(c.Value);
                    }
                }
                try
                {
                    int amount = 0;
                    amount += getItemUnitAmount(itemUnitId, branchId,isKitchen); // from bigger unit
                    amount += getSmallItemUnitAmount(itemUnitId, branchId,isKitchen);
                    return TokenManager.GenerateToken(amount.ToString());
                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }
            }           
        }

        public int getBranchAmount(int itemUnitId, int branchId,int isKitchen = 0)
        {

            int amount = 0;
            amount += getItemUnitAmount(itemUnitId, branchId,isKitchen); // from bigger unit
                amount += getSmallItemUnitAmount(itemUnitId, branchId, isKitchen);
            
            return amount;
        }

        private int getItemUnitAmount(int itemUnitId, int branchId, int isKitchen = 0)
        {
            int amount = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemInLocs = (from b in entity.branches
                                  where b.branchId == branchId
                                  join s in entity.sections on b.branchId equals s.branchId
                                  join l in entity.locations on s.sectionId equals l.sectionId
                                  join il in entity.itemsLocations on l.locationId equals il.locationId
                                  where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null && il.locations.isKitchen == isKitchen
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
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId && x.isActive == 1).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if ((upperUnit != null && itemUnitId == upperUnit.itemUnitId))
                    return amount;
                if (upperUnit != null)
                    amount += (int)upperUnit.unitValue * getItemUnitAmount(upperUnit.itemUnitId, branchId,isKitchen);

                return amount;
            }
        }
        private int getSmallItemUnitAmount(int itemUnitId, int branchId, int isKitchen = 0)
        {
            int amount = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.subUnitId, x.unitId, x.unitValue, x.itemId }).FirstOrDefault();

                var smallUnit = entity.itemsUnits.Where(x => x.unitId == unit.subUnitId && x.itemId == unit.itemId && x.isActive == 1).Select(x => new { x.itemUnitId }).FirstOrDefault();
                if (smallUnit == null || smallUnit.itemUnitId == itemUnitId)
                {
                    return 0;
                }
                else
                {
                    var itemInLocs = (from b in entity.branches
                                      where b.branchId == branchId
                                      join s in entity.sections on b.branchId equals s.branchId
                                      join l in entity.locations on s.sectionId equals l.sectionId
                                      join il in entity.itemsLocations on l.locationId equals il.locationId
                                      where il.itemUnitId == smallUnit.itemUnitId && il.quantity > 0 && il.invoiceId == null && il.locations.isKitchen == isKitchen
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
                    if (unit.unitValue != 0)
                        amount = amount / (int)unit.unitValue;
                    else
                        amount += getSmallItemUnitAmount(smallUnit.itemUnitId, branchId) / (int)unit.unitValue;

                    return amount;
                }
            }
        }
        private int getItemAmount(int itemUnitId, int branchId)
        {
            int amount = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemInLocs = (from b in entity.branches
                                  where b.branchId == branchId
                                  join s in entity.sections on b.branchId equals s.branchId
                                  join l in entity.locations on s.sectionId equals l.sectionId
                                  join il in entity.itemsLocations on l.locationId equals il.locationId
                                  where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null && il.locations.isKitchen != 1
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
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if ((upperUnit != null && itemUnitId == upperUnit.itemUnitId) || upperUnit == null)
                    return amount;
                if (upperUnit != null)
                    amount += (int)upperUnit.unitValue * getItemUnitAmount(upperUnit.itemUnitId, branchId);

                return amount;
            }
        }
        private int getLevelItemUnitAmount(int itemUnitId, int topLevelUnit, int branchId)
        {
            int amount = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemInLocs = (from b in entity.branches
                                  where b.branchId == branchId
                                  join s in entity.sections on b.branchId equals s.branchId
                                  join l in entity.locations on s.sectionId equals l.sectionId
                                  join il in entity.itemsLocations on l.locationId equals il.locationId
                                  where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null && il.locations.isKitchen != 1
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
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if ((upperUnit != null && itemUnitId == upperUnit.itemUnitId) || upperUnit == null)
                    return amount;
                if (upperUnit != null && upperUnit.itemUnitId != topLevelUnit)
                    amount += (int)upperUnit.unitValue * getLevelItemUnitAmount(upperUnit.itemUnitId, topLevelUnit, branchId);

                return amount;
            }
        }


        [HttpPost]
        [Route("getUnitAmount")]
        public string getUnitAmount(string token)
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

                int itemUnitId = 0;
                int branchId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);

                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);

                    }

                }

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var amount = (from b in entity.branches
                                      where b.branchId == branchId
                                      join s in entity.sections on b.branchId equals s.branchId
                                      join l in entity.locations on s.sectionId equals l.sectionId
                                      join il in entity.itemsLocations on l.locationId equals il.locationId
                                      where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null && il.locations.isKitchen != 1
                                      select new
                                      {
                                          il.itemsLocId,
                                          il.quantity,
                                          il.itemUnitId,
                                          il.locationId,
                                          s.sectionId,
                                      }).ToList().Sum(x => x.quantity);
                        return TokenManager.GenerateToken(amount.ToString());
                    }

                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }

            }          
        }
                      
        [HttpPost]
        [Route("returnSpendingOrder")]
        public string returnSpendingOrder(string token)
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
                int branchId = 0;
                int userId = 0;
                List<itemsTransfer> newObject = new List<itemsTransfer>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }

                }

                if (newObject != null)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var freeZoneLocation = (from s in entity.sections.Where(x => x.branchId == branchId && x.isFreeZone == 1 && x.isKitchen != 1)
                                                    join l in entity.locations on s.sectionId equals l.sectionId
                                                    select l.locationId).SingleOrDefault();
                            foreach (itemsTransfer item in newObject)
                            {
                                var itemL = entity.itemsLocations.Where(x => x.itemUnitId == item.itemUnitId && x.locations.isKitchen == 1).FirstOrDefault();
                                if (item.quantity > 0)
                                {
                                    itemL.quantity -= item.quantity;
                                    itemL.updateDate = DateTime.Now;
                                    itemL.updateUserId = userId;
                                    entity.SaveChanges();

                                    var itemId = entity.itemsUnits.Where(x => x.itemUnitId == item.itemUnitId).Select(x => x.itemId).Single();

                                    var itemV = entity.items.Find(itemId);
                                    int quantity = (int)item.quantity;

                                    if (quantity != 0)
                                        increaseItemQuantity(item.itemUnitId.Value, freeZoneLocation, quantity, userId);
                                }
                            }
                            return TokenManager.GenerateToken("1");
                        }
                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
      
        [HttpPost]
        [Route("decreaseItemLocationQuantity")]
        public string decreaseItemLocationQuantity(string token)
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
                int itemLocId = 0;
                int quantity = 0;
                int userId = 0;

                string objectName = "";
                string notificationObj = "";

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemLocId")
                    {
                        itemLocId = int.Parse(c.Value);

                    }
                    else if (c.Type == "quantity")
                    {
                        quantity = int.Parse(c.Value);

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }
                    else if (c.Type == "objectName")
                    {
                        objectName = c.Value;

                    }
                    else if (c.Type == "notificationObj")
                    {
                        notificationObj = c.Value;

                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        itemsLocations itemL = new itemsLocations();

                        itemL = entity.itemsLocations.Find(itemLocId);
                        itemL.quantity -= quantity;
                        itemL.updateDate = DateTime.Now;
                        itemL.updateUserId = userId;
                        entity.SaveChanges();
                        if (objectName != "")
                        {
                            var branchId = (from l in entity.itemsLocations
                                            where l.itemsLocId == itemLocId
                                            select l.locations.branchId).Single();
                            bool isExcedded = isExceddMinQuantity((int)itemL.itemUnitId, (int)branchId, userId);
                            if (isExcedded == true) //add notification
                            {
                                var itemId = entity.itemsUnits.Where(x => x.itemUnitId == itemL.itemUnitId).Select(x => x.itemId).Single();
                                var itemV = entity.items.Find(itemId);
                                notificationController.addNotifications(objectName, notificationObj, (int)branchId, itemV.name);
                            }
                        }
                        return TokenManager.GenerateToken("1");
                    }
                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }

            }
        }

        public void decreaseItemLocationQuantity(int itemLocId, int quantity, int userId, string objectName, string notificationObj)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                itemsLocations itemL = new itemsLocations();

                itemL = entity.itemsLocations.Find(itemLocId);
                itemL.quantity -= quantity;
                itemL.updateDate = DateTime.Now;
                itemL.updateUserId = userId;
                entity.SaveChanges();
                if (objectName != "")
                {
                    var branchId = (from l in entity.itemsLocations
                                    where l.itemsLocId == itemLocId
                                    select l.locations.branchId).Single();
                    bool isExcedded = isExceddMinQuantity((int)itemL.itemUnitId, (int)branchId, userId);
                    if (isExcedded == true) //add notification
                    {
                        var itemId = entity.itemsUnits.Where(x => x.itemUnitId == itemL.itemUnitId).Select(x => x.itemId).Single();
                        var itemV = entity.items.Find(itemId);
                        notificationController.addNotifications(objectName, notificationObj, (int)branchId, itemV.name);
                    }
                }
            }
        }

        public bool isExceddMinQuantity(int itemUnitId, int branchId, int userId)
        {
            bool isExcedded = false;
            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemId = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => x.itemId).Single();
                    var item = entity.items.Find(itemId);
                    int minUnitId = (int)item.minUnitId;
                    int minQuantity = (int)item.min;
                    if (minQuantity == 0)
                        return false;
                    var minUnit = entity.itemsUnits.Where(x => x.itemId == itemId && x.unitId == minUnitId).FirstOrDefault();
                    if (minUnit == null)
                        isExcedded = false;
                    else
                    {
                        int itemUnitQuantity = getItemAmount(minUnit.itemUnitId, branchId);
                        if (itemUnitQuantity <= minQuantity)
                        {
                            isExcedded = true;
                        }
                        if (isExcedded == false)
                        {
                            int smallestItemUnit = entity.itemsUnits.Where(x => x.itemId == itemId && x.subUnitId == x.unitId).Select(x => x.itemUnitId).Single();
                            int smallUnitQuantity = getLevelItemUnitAmount(smallestItemUnit, minUnit.itemUnitId, branchId);
                            int unitValue = itemsUnitsController.getLargeUnitConversionQuan(smallestItemUnit, minUnit.itemUnitId);
                            int quantity = 0;
                            if (unitValue != 0)
                                quantity = smallUnitQuantity / unitValue;

                            quantity += itemUnitQuantity;
                            if (quantity <= minQuantity)
                                isExcedded = true;
                        }
                    }
                }
            }
            catch
            {
            }
            return isExcedded;
        }

       
        [HttpPost]
        [Route("decreaseAmountsInKitchen")]
        public string decreaseAmountsInKitchen(string token)
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
                int branchId = 0;
                int userId = 0;
                List<itemsTransfer> newObject = new List<itemsTransfer>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);

                    }                  
                }

                if (newObject != null)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            foreach (itemsTransfer item in newObject)
                            {
                                updateItemQuantity(item.itemUnitId.Value, branchId, (int)item.quantity, userId,1);

                            }
                        }
                        //  return true;

                        return TokenManager.GenerateToken("1");
                    }

                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }

                }
                else
                {
                    return TokenManager.GenerateToken("0");
                }


            }
        }
             
       
        private Dictionary<string, int> lockLowerUnit(int itemUnitId, int branchId, int requiredAmount, int userId)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            int remainQuantity = 0;
            int firstRequir = requiredAmount;
            int lockedQuantity = 0;
            decimal newQuant = 0;
            dic.Add("lockedQuantity", 0);
            dic.Add("remainQuantity", 0);
            dic.Add("locationId", 0);

            using (incposdbEntities entity = new incposdbEntities())
            {
                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId, x.subUnitId, x.unitValue }).FirstOrDefault();
                var lowerUnit = entity.itemsUnits.Where(x => x.unitId == unit.subUnitId && x.itemId == unit.itemId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (lowerUnit != null && lowerUnit.itemUnitId != itemUnitId)
                {
                    decimal unitValue = (decimal)unit.unitValue;
                    int breakNum = (int)requiredAmount * (int)unitValue;
                    newQuant = (decimal)Math.Ceiling(breakNum / (decimal)unit.unitValue);
                    var itemInLocs = (from b in entity.branches
                                      where b.branchId == branchId
                                      join s in entity.sections on b.branchId equals s.branchId
                                      join l in entity.locations on s.sectionId equals l.sectionId
                                      join il in entity.itemsLocations on l.locationId equals il.locationId
                                      where il.itemUnitId == lowerUnit.itemUnitId && il.quantity > 0 && il.invoiceId == null && il.locations.isKitchen != 1
                                      select new
                                      {
                                          il.itemsLocId,
                                          il.quantity,
                                          il.itemUnitId,
                                          il.locationId,
                                          il.updateDate,
                                          s.sectionId,
                                      }).ToList().OrderBy(x => x.updateDate).ToList();

                    for (int i = 0; i < itemInLocs.Count; i++)
                    {

                        var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);

                        if (breakNum <= (int)itemInLocs[i].quantity)
                        {
                            itemL.quantity = itemInLocs[i].quantity - breakNum;
                            entity.SaveChanges();
                            remainQuantity = (int)newQuant - firstRequir;
                            requiredAmount = 0;
                            lockedQuantity = breakNum;

                            dic["remainQuantity"] = remainQuantity;
                            dic["locationId"] = (int)itemInLocs[i].locationId;
                            dic["lockedQuantity"] += lockedQuantity / (int)unit.unitValue;
                            return dic;
                        }
                        else
                        {
                            itemL.quantity = 0;
                            breakNum = (int)(breakNum - itemInLocs[i].quantity);
                            requiredAmount = requiredAmount - ((int)itemInLocs[i].quantity / (int)unit.unitValue);
                            lockedQuantity += (int)itemInLocs[i].quantity / (int)unit.unitValue;
                            entity.SaveChanges();
                            dic["lockedQuantity"] += lockedQuantity;
                        }
                        if (breakNum == 0)
                            break;
                    }
                    if (itemUnitId == lowerUnit.itemUnitId)
                        return dic;
                    if (breakNum != 0)
                    {
                        dic = new Dictionary<string, int>();
                        dic = lockLowerUnit(lowerUnit.itemUnitId, branchId, breakNum, userId);

                        dic["remainQuantity"] = (int)newQuant - firstRequir;
                        dic["requiredQuantity"] = breakNum;
                        dic["lockedQuantity"] += ((int)newQuant - firstRequir) / (int)unit.unitValue;
                        return dic;
                    }
                }
            }
            return dic;
        }
        private Dictionary<string, int> lockUpperUnit(int itemUnitId, int branchId, int requiredAmount, int userId)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            dic.Add("remainQuantity", 0);
            dic.Add("locationId", 0);
            dic.Add("requiredQuantity", 0);
            dic.Add("lockedQuantity", 0);
            dic.Add("isConsumed", 0);

            int remainQuantity = 0;
            int firstRequir = requiredAmount;
            decimal newQuant = 0;
            int lockedAmount = 0;
            int isConsumed = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId, x.unitValue }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (upperUnit != null && upperUnit.itemUnitId != itemUnitId)
                {
                    decimal unitValue = (decimal)upperUnit.unitValue;
                    int breakNum = (int)Math.Ceiling(requiredAmount / unitValue);
                    newQuant = (decimal)(breakNum * upperUnit.unitValue);
                    var itemInLocs = (from b in entity.branches
                                      where b.branchId == branchId
                                      join s in entity.sections on b.branchId equals s.branchId
                                      join l in entity.locations on s.sectionId equals l.sectionId
                                      join il in entity.itemsLocations on l.locationId equals il.locationId
                                      where il.itemUnitId == upperUnit.itemUnitId && il.quantity > 0 && il.invoiceId == null && il.locations.isKitchen != 1
                                      select new
                                      {
                                          il.itemsLocId,
                                          il.quantity,
                                          il.itemUnitId,
                                          il.locationId,
                                          il.updateDate,
                                          s.sectionId,
                                      }).ToList().OrderBy(x => x.updateDate).ToList();

                    for (int i = 0; i < itemInLocs.Count; i++)
                    {
                        dic["isConsumed"] = 1;
                        isConsumed = 1;
                        var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);

                        if (breakNum <= itemInLocs[i].quantity)
                        {
                            itemL.quantity = itemInLocs[i].quantity - breakNum;
                            entity.SaveChanges();
                            remainQuantity = (int)newQuant - firstRequir;

                            lockedAmount = firstRequir;
                            requiredAmount = 0;
                            // return remainQuantity;
                            dic["remainQuantity"] = remainQuantity;
                            dic["locationId"] = (int)itemInLocs[i].locationId;
                            dic["requiredQuantity"] = 0;
                            dic["lockedQuantity"] += lockedAmount;

                            return dic;
                        }
                        else
                        {
                            itemL.quantity = 0;
                            breakNum = (int)(breakNum - itemInLocs[i].quantity);
                            lockedAmount += (int)itemInLocs[i].quantity;
                            requiredAmount = requiredAmount - ((int)itemInLocs[i].quantity * (int)upperUnit.unitValue);
                            entity.SaveChanges();
                            dic["locationId"] = (int)itemInLocs[i].locationId;
                            dic["requiredQuantity"] = requiredAmount;
                        }
                        if (breakNum == 0)
                            break;
                    }
                    if (breakNum != 0)
                    {
                        dic = new Dictionary<string, int>();
                        dic = lockUpperUnit(upperUnit.itemUnitId, branchId, breakNum, userId);


                        int locationId = dic["locationId"];
                        if (locationId == 0)
                        {
                            var locations = entity.locations.Where(x => x.branchId == branchId && x.isActive == 1).Select(x => new { x.locationId }).OrderBy(x => x.locationId).ToList();

                            if (locationId == 0 && locations.Count >= 1)
                                locationId = locations[0].locationId; // free zoon
                        }
                        var item = (from s in entity.sections
                                    where s.branchId == branchId
                                    join l in entity.locations on s.sectionId equals l.sectionId
                                    join il in entity.itemsLocations on l.locationId equals il.locationId
                                    where il.itemUnitId == upperUnit.itemUnitId && il.invoiceId == null && il.locations.isKitchen != 1
                                    && il.locationId == locationId
                                    select new
                                    {
                                        il.itemsLocId,
                                    }).FirstOrDefault();
                        if (item != null)
                        {
                            var itemloc = entity.itemsLocations.Find(item.itemsLocId);
                            itemloc.quantity += dic["remainQuantity"];
                            entity.SaveChanges();
                        }
                        else
                        {

                            itemsLocations itemL = new itemsLocations();
                            itemL.itemUnitId = upperUnit.itemUnitId;
                            itemL.locationId = locationId;
                            itemL.quantity = dic["remainQuantity"];
                            itemL.createDate = DateTime.Now;
                            itemL.updateDate = DateTime.Now;
                            itemL.createUserId = userId;
                            itemL.updateUserId = userId;

                            entity.itemsLocations.Add(itemL);
                            entity.SaveChanges();

                        }

                        dic["locationId"] = locationId;
                        if (dic["lockedQuantity"] > 0)
                        {
                            isConsumed = 1;

                            lockedAmount += dic["lockedQuantity"] * (int)upperUnit.unitValue;
                            dic["lockedQuantity"] = lockedAmount;
                        }
                        if (isConsumed == 0)
                        {
                            dic["requiredQuantity"] = requiredAmount;
                            dic["remainQuantity"] = 0;
                        }
                        else
                        {
                            dic["remainQuantity"] = (int)newQuant - firstRequir;
                            dic["requiredQuantity"] = dic["requiredQuantity"] * (int)upperUnit.unitValue;
                        }
                        return dic;
                    }
                }
                else
                {
                    dic["remainQuantity"] = 0;
                    dic["requiredQuantity"] = requiredAmount;
                    dic["locationId"] = 0;
                    dic["lockedQuantity"] = 0;
                    return dic;
                }
            }
            return dic;
        }
        private void increaseLockedItem(int itemUnitId, int locationId, int quantity, int invoiceId, int userId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemUnit = (from il in entity.itemsLocations
                                where il.itemUnitId == itemUnitId && il.locationId == locationId && il.invoiceId == invoiceId 
                                select new { il.itemsLocId }
                                ).FirstOrDefault();
                itemsLocations itemL = new itemsLocations();
                if (itemUnit == null)//add item in new location
                {
                    itemL.itemUnitId = itemUnitId;
                    itemL.locationId = locationId;
                    itemL.quantity = quantity;
                    itemL.createDate = DateTime.Now;
                    itemL.updateDate = DateTime.Now;
                    itemL.createUserId = userId;
                    itemL.updateUserId = userId;
                    itemL.invoiceId = invoiceId;

                    entity.itemsLocations.Add(itemL);
                }
                else
                {
                    itemL = entity.itemsLocations.Find(itemUnit.itemsLocId);
                    itemL.quantity += quantity;
                    itemL.updateDate = DateTime.Now;
                    itemL.updateUserId = userId;
                }
                entity.SaveChanges();
            }
        }

        [HttpPost]
        [Route("unitsConversion")]
        public string unitsConversion(string token)
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
                int branchId = 0;
                int fromItemUnit = 0;
                int toItemUnit = 0;
                int fromQuantity = 0;
                int toQuantity = 0;
                int userId = 0;

                itemsUnits newObject = new itemsUnits();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<itemsUnits>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);

                    }
                    else if (c.Type == "fromItemUnit")
                    {
                        fromItemUnit = int.Parse(c.Value);

                    }
                    else if (c.Type == "toItemUnit")
                    {
                        toItemUnit = int.Parse(c.Value);

                    }
                    else if (c.Type == "fromQuantity")
                    {
                        fromQuantity = int.Parse(c.Value);

                    }
                    else if (c.Type == "toQuantity")
                    {
                        toQuantity = int.Parse(c.Value);

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }
                }

                if (newObject != null)
                {
                    try
                    {
                        #region covert from unit (fromItemUnit) is bigger than the last (toItemUnit)
                        if (newObject.itemUnitId != 0)// covert from unit (fromItemUnit) is bigger than the last (toItemUnit)
                        {
                            using (incposdbEntities entity = new incposdbEntities())
                            {
                                var itemInLocs = (from b in entity.branches
                                                  where b.branchId == branchId
                                                  join s in entity.sections on b.branchId equals s.branchId
                                                  join l in entity.locations on s.sectionId equals l.sectionId
                                                  join il in entity.itemsLocations on l.locationId equals il.locationId
                                                  where il.itemUnitId == fromItemUnit && il.quantity > 0 && il.invoiceId == null && il.locations.isKitchen != 1
                                                  select new
                                                  {
                                                      il.itemsLocId,
                                                      il.quantity,
                                                      il.itemUnitId,
                                                      il.locationId,
                                                      s.sectionId,
                                                  }).ToList();
                                int unitValue = getUnitValue(fromItemUnit, toItemUnit);

                                for (int i = 0; i < itemInLocs.Count; i++)
                                {
                                    int toQuant = 0;
                                    int availableAmount = (int)itemInLocs[i].quantity;
                                    var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);
                                    itemL.updateDate = DateTime.Now;
                                    if (availableAmount >= fromQuantity)
                                    {
                                        itemL.quantity = availableAmount - fromQuantity;
                                        toQuant = fromQuantity * unitValue;
                                        fromQuantity = 0;
                                        entity.SaveChanges();
                                    }
                                    else if (availableAmount > 0)
                                    {
                                        itemL.quantity = 0;
                                        fromQuantity = fromQuantity - availableAmount;
                                        toQuant = availableAmount * unitValue;
                                        entity.SaveChanges();
                                    }

                                    increaseItemQuantity(toItemUnit, (int)itemInLocs[i].locationId, toQuant, userId);

                                    if (fromQuantity == 0)
                                        //  return true;
                                        return TokenManager.GenerateToken("1");
                                }
                            }
                            // return true;
                            return TokenManager.GenerateToken("1");
                        }
                        #endregion
                        #region from small to large
                        else
                        {
                            using (incposdbEntities entity = new incposdbEntities())
                            {
                                var itemInLocs = (from b in entity.branches
                                                  where b.branchId == branchId
                                                  join s in entity.sections on b.branchId equals s.branchId
                                                  join l in entity.locations on s.sectionId equals l.sectionId
                                                  join il in entity.itemsLocations on l.locationId equals il.locationId
                                                  where il.itemUnitId == fromItemUnit && il.quantity > 0 && il.invoiceId == null && il.locations.isKitchen != 1
                                                  select new
                                                  {
                                                      il.itemsLocId,
                                                      il.quantity,
                                                      il.itemUnitId,
                                                      il.locationId,
                                                      s.sectionId,
                                                  }).ToList();

                                int unitValue = getUnitValue(toItemUnit, fromItemUnit);
                                int i = 0;
                                for (i = 0; i < itemInLocs.Count; i++)
                                {
                                    int availableAmount = (int)itemInLocs[i].quantity;
                                    var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);
                                    itemL.updateDate = DateTime.Now;
                                    if (availableAmount >= fromQuantity)
                                    {
                                        itemL.quantity = availableAmount - fromQuantity;
                                        fromQuantity = 0;
                                        entity.SaveChanges();
                                    }
                                    else if (availableAmount > 0)
                                    {
                                        itemL.quantity = 0;
                                        fromQuantity = fromQuantity - availableAmount;
                                        entity.SaveChanges();
                                    }



                                    if (fromQuantity == 0)
                                        //  return true;
                                        return TokenManager.GenerateToken("1");
                                }
                                increaseItemQuantity(toItemUnit, (int)itemInLocs[i].locationId, toQuantity, userId);
                                //  return true;
                                return TokenManager.GenerateToken("1");
                            }
                            #endregion
                        }


                    }

                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }

                }
                else
                {
                    return TokenManager.GenerateToken("0");
                }


            }
        }

        private int getUnitValue(int itemunitId, int smallestItemUnitId)
        {
            int unitValue = 0;
            using (incposdbEntities entity = new incposdbEntities())
            {
                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemunitId).Select(x => new { x.subUnitId, x.unitId, x.unitValue, x.itemId }).FirstOrDefault();
                int smallUnitId = entity.itemsUnits.Where(x => x.unitId == unit.subUnitId && x.itemId == unit.itemId).Select(x => x.itemUnitId).Single();
                unitValue = (int)unit.unitValue;
                if (itemunitId == smallestItemUnitId)
                    return unitValue;
                else
                {
                    unitValue = unitValue * getUnitValue(smallUnitId, smallestItemUnitId);
                }
            }
            return unitValue;
        }


        [HttpPost]
        [Route("getSpecificItemLocation")]
        public string getSpecificItemLocation(string token)
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
                int branchId = 0;

                string newObject = "";
                List<int> ids = new List<int>();
                List<string> strIds = new List<string>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemUnitsIds")
                    {
                        newObject = c.Value;
                        strIds = newObject.Split(',').ToList();
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }

                if (strIds != null)
                {
                    try
                    {
                        for (int i = 0; i < strIds.Count; i++)
                        {
                            if (!strIds[i].Equals(""))
                                ids.Add(int.Parse(strIds[i]));
                        }

                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var locList = (from b in entity.itemsLocations
                                           where b.quantity > 0 && b.invoiceId == null && b.locations.isKitchen != 1 && ids.Contains((int)b.itemUnitId)
                                           join u in entity.itemsUnits on b.itemUnitId equals u.itemUnitId
                                           join i in entity.items on u.itemId equals i.itemId
                                           join l in entity.locations on b.locationId equals l.locationId
                                           join s in entity.sections on l.sectionId equals s.sectionId
                                           where s.branchId == branchId

                                           select new ItemLocationModel
                                           {
                                               createDate = b.createDate,
                                               createUserId = b.createUserId,
                                               endDate = b.endDate,
                                               itemsLocId = b.itemsLocId,
                                               itemUnitId = b.itemUnitId,
                                               locationId = b.locationId,
                                               notes = b.notes,
                                               quantity = b.quantity,
                                               startDate = b.startDate,

                                               updateDate = b.updateDate,
                                               updateUserId = b.updateUserId,
                                               itemName = i.name,
                                               unitName = u.units.name,
                                               sectionId = s.sectionId,
                                               isFreeZone = s.isFreeZone,
                                               itemType = i.type,
                                               location = l.x + l.y + l.z,
                                           }).OrderBy(a => a.endDate)
                                            .ToList();

                            return TokenManager.GenerateToken(locList);
                        }
                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("0");
                }
            }

        }


        [HttpPost]
        [Route("getShortageItems")]
        public string getShortageItems(string token)
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
                int branchId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);

                    }

                }
                try
                {

                    InvoicesController c = new InvoicesController();
                    var orders = c.getUnhandeledOrdersList("or", 0, branchId);

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        List<ItemTransferModel> requiredTransfers = new List<ItemTransferModel>();
                        foreach (InvoiceModel invoice in orders)
                        {
                            var itemsTransfer = entity.itemsTransfer.Where(x => x.invoiceId == invoice.invoiceId).ToList();
                            foreach (itemsTransfer tr in itemsTransfer)
                            {
                                var lockedQuantity = entity.itemsLocations
                                    .Where(x => x.invoiceId == invoice.invoiceId && x.itemUnitId == tr.itemUnitId)
                                    .Select(x => x.quantity).Sum();
                                var availableAmount = getBranchAmount((int)tr.itemUnitId, branchId);
                                var item = (from i in entity.items
                                            join u in entity.itemsUnits on i.itemId equals u.itemId
                                            where u.itemUnitId == tr.itemUnitId
                                            select new ItemModel()
                                            {
                                                itemId = i.itemId,
                                                name = i.name,
                                                unitName = u.units.name,
                                            }).FirstOrDefault();
                                if (lockedQuantity == null)
                                    lockedQuantity = 0;
                                if ((lockedQuantity + availableAmount) < tr.quantity) // there is a shortage in order amount
                                {
                                    long requiredQuantity = (long)tr.quantity - ((long)lockedQuantity + (long)availableAmount);
                                    ItemTransferModel transfer = new ItemTransferModel()
                                    {
                                        invNumber = invoice.invNumber,
                                        invoiceId = invoice.invoiceId,
                                        price = 0,
                                        quantity = requiredQuantity,
                                        itemUnitId = tr.itemUnitId,
                                        itemId = item.itemId,
                                        itemName = item.name,
                                        unitName = item.unitName,
                                    };
                                    requiredTransfers.Add(transfer);
                                }

                            }
                        }
                        return TokenManager.GenerateToken(requiredTransfers);
                    }
                }

                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }

            }



        }
      
    }
}