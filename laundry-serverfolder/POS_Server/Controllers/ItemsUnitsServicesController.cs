using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS_Server.Models;
using POS_Server.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/ItemsUnitsServices")]
    public class ItemsUnitsServicesController : ApiController
    {
        // GET api/<controller> get all ItemsUnitsServices
        [HttpPost]
        [Route("Get")]
        public string Get(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            bool canDelete = false;
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var  List = entity.ItemsUnitsServices

                   .Select(S => new ItemsUnitsServicesModel
                   {
                       itemUnitServiceId = S.itemUnitServiceId,
                       normalPrice = S.normalPrice,
                       instantPrice = S.instantPrice,
                       createDate = S.createDate,
                       updateDate = S.updateDate,
                       createUserId = S.createUserId,
                       updateUserId = S.updateUserId,
                       serviceId = S.serviceId,
                       itemUnitId = S.itemUnitId,
                       cost = S.cost,


                   })
                   .ToList();
                    /*
         public int itemUnitServiceId { get; set; }
        public decimal normalPrice { get; set; }
        public decimal instantPrice { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<int> itemUnitServiceId { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public decimal cost { get; set; }
                   */
                    // can delet or not
                    if (List.Count > 0)
                    {
                        //foreach (ItemsUnitsServicesModel item in List)
                        //{
                        //    canDelete = false;
                        //    if (item.isActive == 1)
                        //    {
                        //        int itemUnitServiceId = (int)item.itemUnitServiceId;
                        //        var IU = entity.ItemsUnitsServices.Where(x => x.itemUnitServiceId == itemUnitServiceId).Select(x => new { x.itemUnitServiceId }).FirstOrDefault();
                        //        var Sub = entity.subServices.Where(x => x.itemUnitServiceId == itemUnitServiceId).Select(x => new { x.subServiceId }).FirstOrDefault();

                        //        if ((IU is null && Sub is null))
                        //            canDelete = true;
                        //    }
                        //    item.canDelete = canDelete;
                        //}
                    }


                    return TokenManager.GenerateToken( List);
                }
            }
        }


        [HttpPost]
        [Route("GetIUServicesByServiceId")]
        public string GetIUServicesByServiceId(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
           
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                int serviceId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        serviceId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var List = entity.ItemsUnitsServices

                   .Select(S => new ItemsUnitsServicesModel
                   {
                       itemUnitServiceId = S.itemUnitServiceId,
                       normalPrice = S.normalPrice,
                       instantPrice = S.instantPrice,
                       //createDate = S.createDate,
                       //updateDate = S.updateDate,
                       //createUserId = S.createUserId,
                       //updateUserId = S.updateUserId,
                       serviceId = S.serviceId,
                       itemUnitId = S.itemUnitId,
                       cost = S.cost,
                       itemId = S.itemsUnits.items.itemId,
                       itemName = S.itemsUnits.items.name,
                       unitId = S.itemsUnits.units.unitId,
                       unitName = S.itemsUnits.units.name,
                       ServiceName=S.services.name,


                   })
                   .ToList().Where(X=>X.unitName== "saleUnit" && X.serviceId==serviceId).ToList();
        


                    return TokenManager.GenerateToken(List);
                }
            }
        }

        // GET api/<controller>  Get Coupon By ID 
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
                int Id = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        Id = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var item = entity.ItemsUnitsServices
                   .Where(S => S.itemUnitServiceId == Id)
                   .Select(S => new
                   {
                       S.itemUnitServiceId,
                       S.normalPrice,
                       S.instantPrice,
                       S.createDate,
                       S.updateDate,
                       S.createUserId,
                       S.updateUserId,
                       S.serviceId,
                       S.itemUnitId,
                       S.cost,


                   })
                   .FirstOrDefault();

                    return TokenManager.GenerateToken(item);
                }
            }
        }
       
        
    
        // add or update coupon 
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
                string  Obj = "";
                ItemsUnitsServices newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        Obj = c.Value.Replace("\\", string.Empty);
                        Obj = Obj.Trim('"');
                        newObject = JsonConvert.DeserializeObject<ItemsUnitsServices>(Obj, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }

                try
                {
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
                    if (newObject.serviceId == 0 || newObject.serviceId == null)
                    {
                        Nullable<int> id = null;
                        newObject.serviceId = id;
                    }
                    if (newObject.itemUnitId == 0 || newObject.itemUnitId == null)
                    {
                        Nullable<int> id = null;
                        newObject.itemUnitId = id;
                    }

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        ItemsUnitsServices tmpObject = new ItemsUnitsServices();
                        var  Entityobj = entity.Set<ItemsUnitsServices>();
                        if (newObject.itemUnitServiceId == 0)
                        {

                            newObject.createDate = DateTime.Now;
                            newObject.updateDate = newObject.createDate;
                            newObject.updateUserId = newObject.createUserId;

                            tmpObject = Entityobj.Add(newObject);
                            entity.SaveChanges();
                            message = tmpObject .itemUnitServiceId.ToString();
                            return TokenManager.GenerateToken(message);
                        }
                        else
                        {

                            tmpObject = entity.ItemsUnitsServices.Where(p => p.itemUnitServiceId == newObject.itemUnitServiceId).FirstOrDefault();
                            tmpObject.itemUnitServiceId = newObject.itemUnitServiceId;
                            tmpObject.normalPrice = newObject.normalPrice;
                            tmpObject.instantPrice = newObject.instantPrice;
                          //  tmpObject.createDate = newObject.createDate;
                            tmpObject.updateDate = DateTime.Now; ;
                           // tmpObject.createUserId = newObject.createUserId;
                            tmpObject.updateUserId = newObject.updateUserId;
                            tmpObject.serviceId = newObject.serviceId;
                            tmpObject.itemUnitId = newObject.itemUnitId;
                            tmpObject.cost = newObject.cost;


                            entity.SaveChanges();
                            message = tmpObject.itemUnitServiceId.ToString();
                            return TokenManager.GenerateToken(message);
                        }
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
                int Id = 0;
                int userId = 0;
                bool final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        Id = int.Parse(c.Value);
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
                //if (final)
                //{
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            ItemsUnitsServices Obj = entity.ItemsUnitsServices.Find(Id);

                            entity.ItemsUnitsServices.Remove(Obj);
                            message = entity.SaveChanges().ToString();
                        return TokenManager.GenerateToken(message);
                    }
                       
                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }
                //}
                //else
                //{
                //    try
                //    {
                //        using (incposdbEntities entity = new incposdbEntities())
                //        {
                //            ItemsUnitsServices coupObj = entity.ItemsUnitsServices.Find(Id);

                //            coupObj.isActive = 0;
                //            coupObj.updateUserId = userId;
                //            coupObj.updateDate = DateTime.Now;
                //            message = entity.SaveChanges().ToString();
                //            return TokenManager.GenerateToken(message);
                //        }
                //    }
                //    catch
                //    {
                //        message = "0";
                //        return TokenManager.GenerateToken(message);
                //    }
                //}
            }
        }

        public int Save(ItemsUnitsServices newObject)
        {

            int message = 0;

                try
                {
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
                    if (newObject.serviceId == 0 || newObject.serviceId == null)
                    {
                        Nullable<int> id = null;
                        newObject.serviceId = id;
                    }
                    if (newObject.itemUnitId == 0 || newObject.itemUnitId == null)
                    {
                        Nullable<int> id = null;
                        newObject.itemUnitId = id;
                    }

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        ItemsUnitsServices tmpObject = new ItemsUnitsServices();
                        var Entityobj = entity.Set<ItemsUnitsServices>();
                        if (newObject.itemUnitServiceId == 0)
                        {

                            newObject.createDate = DateTime.Now;
                            newObject.updateDate = newObject.createDate;
                            newObject.updateUserId = newObject.createUserId;

                            tmpObject = Entityobj.Add(newObject);
                            entity.SaveChanges();
                            message = tmpObject.itemUnitServiceId;
                            return message;
                        }
                        else
                        {

                            tmpObject = entity.ItemsUnitsServices.Where(p => p.itemUnitServiceId == newObject.itemUnitServiceId).FirstOrDefault();
                            tmpObject.itemUnitServiceId = newObject.itemUnitServiceId;
                            tmpObject.normalPrice = newObject.normalPrice;
                            tmpObject.instantPrice = newObject.instantPrice;
                           // tmpObject.createDate = newObject.createDate;
                            tmpObject.updateDate = DateTime.Now;
                         //   tmpObject.createUserId = newObject.createUserId;
                            tmpObject.updateUserId = newObject.updateUserId;
                            tmpObject.serviceId = newObject.serviceId;
                            tmpObject.itemUnitId = newObject.itemUnitId;
                            tmpObject.cost = newObject.cost;


                            entity.SaveChanges();
                            message = tmpObject.itemUnitServiceId;
                            return message;
                        }
                    }
                }

                catch
                {
                    message = 0;
                    return message;
                }
             
        }

        [HttpPost]
        [Route("UpdateIUServiceList")]
        public string UpdateIUServiceList(string token)
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
                string strObject = "";
                List<ItemsUnitsServices> newListObj = null;
                int serviceId = 0;
                int updateUserId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "newList")
                    {
                        strObject = c.Value.Replace("\\", string.Empty);
                        strObject = strObject.Trim('"');
                        newListObj = JsonConvert.DeserializeObject<List<ItemsUnitsServices>>(strObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "serviceId")
                    {
                        serviceId = int.Parse(c.Value);
                    }
                    else
                  if (c.Type == "updateUserId")
                    {
                        updateUserId = int.Parse(c.Value);
                    }
                }

                List<ItemsUnitsServices> items = null;

                try
                {
                    int res = 0;

                    using (incposdbEntities entity = new incposdbEntities())
                {
                        items = entity.ItemsUnitsServices.Where(x => x.serviceId == serviceId).ToList();
                        if (items != null)
                        {
                            foreach (ItemsUnitsServices newrow in newListObj)
                            {
                                ItemsUnitsServices tempobj = new ItemsUnitsServices();
                                
                                if (newrow.itemUnitServiceId > 0)
                                {
                                    tempobj = items.Where(X => X.itemUnitServiceId == newrow.itemUnitServiceId).FirstOrDefault();

                                    tempobj.cost=newrow.cost;
                                    tempobj.normalPrice = newrow.normalPrice;
                                    tempobj.instantPrice = newrow.instantPrice;

                                    tempobj.updateUserId = updateUserId;
                                   // newrow.serviceId = serviceId;
                                    res = Save(tempobj);
                                    if (res == 0)
                                    {
                                        return TokenManager.GenerateToken("0");
                                    }
                                }
                            }

                           

                        }
 return TokenManager.GenerateToken("1");

                    }
                             }
                catch (Exception ex)
                {
                    message = "-2";
                    return TokenManager.GenerateToken(message);
                }

            }

        }

        [HttpPost]
        [Route("UpdateCostByServiceId")]
        public string UpdateCostByServiceId(string token)
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
                int serviceId = 0;
                decimal cost = 0;
                int updateUserId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {                 
                     if (c.Type == "serviceId")
                    {
                        serviceId = int.Parse(c.Value);
                    }
                    else if (c.Type == "cost")
                    {
                        cost = decimal.Parse(c.Value);
                    }
                    else if (c.Type == "updateUserId")
                    {
                        updateUserId = int.Parse(c.Value);
                    }
                }
                try
                {
                    message = UpdateValue(serviceId, cost, "cost", updateUserId);
                    if (message!="0" && message != "-2")
                    {

                        return TokenManager.GenerateToken("1");
                    }
                    else
                    {
                        return TokenManager.GenerateToken("-2");
                    }

                }
                catch (Exception ex)
                {
                    message = "-2";
                    return TokenManager.GenerateToken(message);
                }

            }

        }

        /// //////////////////////
        [HttpPost]
        [Route("UpdateNormalByServiceId")]
        public string UpdateNormalByServiceId(string token)
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
                int serviceId = 0;
                decimal normal = 0;
                int updateUserId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "serviceId")
                    {
                        serviceId = int.Parse(c.Value);
                    }
                    else if (c.Type == "normal")
                    {
                        normal = decimal.Parse(c.Value);
                    }
                    else if (c.Type == "updateUserId")
                    {
                        updateUserId = int.Parse(c.Value);
                    }
                }
                try
                {
                    message = UpdateValue(serviceId, normal, "normal", updateUserId);
                    if (message != "0" && message != "-2")
                    {

                        return TokenManager.GenerateToken("1");
                    }
                    else
                    {
                        return TokenManager.GenerateToken("-2");
                    }

                }
                catch (Exception ex)
                {
                    message = "-2";
                    return TokenManager.GenerateToken(message);
                }

            }

        }

        /// //////////////////////
        [HttpPost]
        [Route("UpdateInstantByServiceId")]
        public string UpdateInstantByServiceId(string token)
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
                int serviceId = 0;
                decimal instant = 0;
                int updateUserId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "serviceId")
                    {
                        serviceId = int.Parse(c.Value);
                    }
                    else if (c.Type == "instant")
                    {
                        instant = decimal.Parse(c.Value);
                    }
                    else if (c.Type == "updateUserId")
                    {
                        updateUserId = int.Parse(c.Value);
                    }
                }
                try
                {
                    message = UpdateValue(serviceId, instant, "instant", updateUserId);
                    if (message != "0" && message != "-2")
                    {

                        return TokenManager.GenerateToken("1");
                    }
                    else
                    {
                        return TokenManager.GenerateToken("-2");
                    }

                }
                catch (Exception ex)
                {
                    message = "-2";
                    return TokenManager.GenerateToken(message);
                }

            }

        }

        public string UpdateValue(int serviceId, decimal value,string type, int updateUserId)
        {
            //  cost normal  instant
 
                List<ItemsUnitsServices> items = null;
                try
                {
                    int res = 0;

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        items = entity.ItemsUnitsServices.Where(x => x.serviceId == serviceId).ToList();
                        if (items != null)
                        {
                        if (type == "cost")
                        {
                            foreach (ItemsUnitsServices row in items)
                            {

                                if (row.itemUnitServiceId > 0)
                                {
                                    row.updateUserId = updateUserId;
                                    row.cost = value;
                                    //  row.serviceId = serviceId;
                                    res = Save(row);
                                    if (res == 0)
                                    {
                                        return "0";
                                    }
                                }
                            }
                        } else if (type == "normal")
                        {
                            foreach (ItemsUnitsServices row in items)
                            {
                                if (row.itemUnitServiceId > 0)
                                {
                                    row.updateUserId = updateUserId;

                                    row.normalPrice = value;
                                    //  row.serviceId = serviceId;
                                    res = Save(row);
                                    if (res == 0)
                                    {
                                        return  "0";
                                    }
                                }
                            }
                        } else if(type == "instant")
                        {
                            foreach (ItemsUnitsServices row in items)
                            {

                                if (row.itemUnitServiceId > 0)
                                {
                                    row.updateUserId = updateUserId;

                                    row.instantPrice = value;
                                    //  row.serviceId = serviceId;
                                    res = Save(row);
                                    if (res == 0)
                                    {
                                        return  "0";
                                    }
                                }
                            }
                        }
                        else
                        {
                            return "0";
                        }
                        }
                        return TokenManager.GenerateToken("1");
                    }
                }
                catch (Exception ex)
                {
                    return  "-2" ;
                }
        }
    }
}