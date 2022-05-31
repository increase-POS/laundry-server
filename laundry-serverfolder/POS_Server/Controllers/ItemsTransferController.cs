using Newtonsoft.Json;
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

namespace POS_Server.Controllers
{
    [RoutePrefix("api/ItemsTransfer")]
    public class ItemsTransferController : ApiController
    {
        [HttpPost]
        [Route("Get")]
        public string Get(string token)
        {
            //int invoiceId

            // public string GetUsersByGroupId(string token)
          token = TokenManager.readToken(HttpContext.Current.Request);var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int invoiceId = 0;


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        invoiceId = int.Parse(c.Value);
                    }


                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var transferList = (from t in entity.itemsTransfer.Where(x => x.invoiceId == invoiceId)
                                            join u in entity.itemsUnits on t.itemUnitId equals u.itemUnitId
                                            join i in entity.items on u.itemId equals i.itemId
                                            join un in entity.units on u.unitId equals un.unitId
                                            join inv in entity.invoices on t.invoiceId equals inv.invoiceId
                                            select new ItemTransferModel()
                                            {
                                                itemsTransId = t.itemsTransId,
                                                itemId = i.itemId,
                                                itemName = i.name,
                                                quantity = t.quantity,
                                                invoiceId = entity.invoiceOrder.Where(x => x.itemsTransferId == t.itemsTransId).Select(x => x.orderId).FirstOrDefault(),
                                                invNumber = inv.invNumber,
                                             
                                                createUserId = t.createUserId,
                                                updateUserId = t.updateUserId,
                                                notes = t.notes,
                                                createDate = t.createDate,
                                                updateDate = t.updateDate,
                                                itemUnitId = u.itemUnitId,
                                                price = t.price,
                                                unitName = un.name,
                                                unitId = un.unitId,
                                                barcode = u.barcode,
                                                itemSerial = t.itemSerial,
                                                itemType = i.type,
                                                offerId = t.offerId,
                                                forAgents = t.forAgents,
                                            })
                                            .ToList();

                        return TokenManager.GenerateToken(transferList);
                    }

                    }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        // add or update item transfer
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
                int invoiceId = 0;
                string Object = "";
                List<itemsTransfer> newObject = new List<itemsTransfer>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "invoiceId")
                    {
                        invoiceId = int.Parse(c.Value);
                    }
                }
                if (newObject != null)
                {
                    try
                    {
                  string res = saveInvoiceItems(newObject,invoiceId);
                        if (res == "0")
                            message = "0";
                        else
                            message = "1";
                        return TokenManager.GenerateToken(message);
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
        public string saveInvoiceItems(List<itemsTransfer> newObject, int invoiceId)
        {
            string message = "";
            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    List<invoiceOrder> iol = entity.invoiceOrder.Where(x => x.invoiceId == invoiceId).ToList();
                    entity.invoiceOrder.RemoveRange(iol);
                    entity.SaveChanges();

                    List<itemsTransfer> items = entity.itemsTransfer.Where(x => x.invoiceId == invoiceId).ToList();
                    entity.itemsTransfer.RemoveRange(items);
                    entity.SaveChanges();

                    var invoice = entity.invoices.Find(invoiceId);
                    for (int i = 0; i < newObject.Count; i++)
                    {
                        itemsTransfer t;
                        if (newObject[i].createUserId == 0 || newObject[i].createUserId == null)
                        {
                            Nullable<int> id = null;
                            newObject[i].createUserId = id;
                        }
                        if (newObject[i].offerId == 0)
                        {
                            Nullable<int> id = null;
                            newObject[i].offerId = id;
                        }
                        if (newObject[i].itemSerial == null)
                            newObject[i].itemSerial = "";

                        var transferEntity = entity.Set<itemsTransfer>();
                        int orderId = 0;
                        try { orderId = (int)newObject[i].invoiceId; } catch { }
                        
                        newObject[i].invoiceId = invoiceId;
                        newObject[i].createDate = DateTime.Now;
                        newObject[i].updateDate = DateTime.Now;
                        newObject[i].updateUserId = newObject[i].createUserId;

                        t = entity.itemsTransfer.Add(newObject[i]);
                        entity.SaveChanges();

                        if (orderId != 0)
                        {
                            invoiceOrder invoiceOrder = new invoiceOrder()
                            {
                                invoiceId = invoiceId,
                                orderId = orderId,
                                quantity = (int)newObject[i].quantity,
                                itemsTransferId = t.itemsTransId,
                            };
                            entity.invoiceOrder.Add(invoiceOrder);
                        }
                        if (newObject[i].offerId != null && invoice.invType == "s")
                        {
                            int offerId = (int)newObject[i].offerId;
                            int itemUnitId = (int)newObject[i].itemUnitId;
                            var offer = entity.itemsOffers.Where(x => x.iuId == itemUnitId && x.offerId == offerId).FirstOrDefault();

                            offer.used += (int)newObject[i].quantity;
                        }
                    }
                    entity.SaveChanges();
                    message = "1";
                }
            }
            catch { message = "0"; }
            return message;
        }
        [HttpPost]
        [Route("getOrderItems")]
        public string getOrderItems(string token)
        {
            string message = "";
            ItemsLocationsController ilc = new ItemsLocationsController();
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int branchId = 0;
                int invoiceId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                        branchId = int.Parse(c.Value);
                    else if (c.Type == "invoiceId")
                        invoiceId = int.Parse(c.Value);
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        List<ItemTransferModel> requiredTransfers = new List<ItemTransferModel>();
                        var itemsTransfer = entity.itemsTransfer.Where(x => x.invoiceId == invoiceId).ToList();
                        foreach (itemsTransfer tr in itemsTransfer)
                        {
                            var lockedQuantity = entity.itemsLocations
                                .Where(x => x.invoiceId == invoiceId && x.itemUnitId == tr.itemUnitId)
                                .Select(x => x.quantity).Sum();
                            var availableAmount = ilc.getBranchAmount((int)tr.itemUnitId, branchId);
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

                            long requiredQuantity = (long)tr.quantity - ((long)lockedQuantity + (long)availableAmount);
                            ItemTransferModel transfer = new ItemTransferModel()
                            {
                                invoiceId = invoiceId,
                                price = 0,
                                quantity = tr.quantity,
                                lockedQuantity = lockedQuantity,
                                newLocked = lockedQuantity,
                                availableQuantity = availableAmount,
                                itemUnitId = tr.itemUnitId,
                                itemId = item.itemId,
                                itemName = item.name,
                                unitName = item.unitName,
                            };
                            requiredTransfers.Add(transfer);

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