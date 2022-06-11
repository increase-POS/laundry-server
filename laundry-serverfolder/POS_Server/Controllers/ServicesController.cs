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
    [RoutePrefix("api/services")]
    public class ServicesController : ApiController
    {
        // GET api/<controller> get all services
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
                    var List = entity.services

                   .Select(S => new ServicesModel
                   {
                       serviceId = S.serviceId,
                       name = S.name,
                       notes = S.notes,
                       isActive = S.isActive,
                       price = S.price,
                       cost = S.cost,
                       categoryId = S.categoryId,
                       createDate = S.createDate,
                       updateDate = S.updateDate,
                       createUserId = S.createUserId,
                       updateUserId = S.updateUserId,

                   })
                   .ToList();
                    /*
                        public int serviceId { get; set; }
        public string name { get; set; }
        public string notes { get; set; }
        public byte isActive { get; set; }
        public decimal price { get; set; }
        public decimal cost { get; set; }
        public Nullable<int> categoryId { get; set; }
        public System.DateTime createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
     
                   */
                    // can delet or not
                    if (List.Count > 0)
                    {
                        foreach (ServicesModel item in List)
                        {
                            canDelete = false;
                            if (item.isActive == 1)
                            {
                                int serviceId = (int)item.serviceId;
                                var IU = entity.ItemsUnitsServices.Where(x => x.serviceId == serviceId).Select(x => new { x.itemUnitServiceId }).FirstOrDefault();
                                var Sub = entity.subServices.Where(x => x.serviceId == serviceId).Select(x => new { x.subServiceId }).FirstOrDefault();

                                if ((IU is null && Sub is null))
                                    canDelete = true;
                            }
                            item.canDelete = canDelete;
                        }
                    }


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
                    var item = entity.services
                   .Where(S => S.serviceId == Id)
                   .Select(S => new
                   {
                       S.serviceId,
                       S.name,
                       S.notes,
                       S.isActive,
                       S.price,
                       S.cost,
                       S.categoryId,
                       S.createDate,
                       S.updateDate,
                       S.createUserId,
                       S.updateUserId,

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
                string Obj = "";
                services newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        Obj = c.Value.Replace("\\", string.Empty);
                        Obj = Obj.Trim('"');
                        newObject = JsonConvert.DeserializeObject<services>(Obj, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
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
                    if (newObject.categoryId == 0 || newObject.categoryId == null)
                    {
                        Nullable<int> id = null;
                        newObject.categoryId = id;
                    }
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        services tmpObject = new services();
                        var Entityobj = entity.Set<services>();
                        if (newObject.serviceId == 0)
                        {

                            newObject.createDate = DateTime.Now;
                            newObject.updateDate = newObject.createDate;
                            newObject.updateUserId = newObject.createUserId;

                            tmpObject = Entityobj.Add(newObject);
                            entity.SaveChanges();
                            message = tmpObject.serviceId.ToString();
                            int res=  SaveIUSbyServiceId(tmpObject);
                            if (res == 0)
                            {
                                return TokenManager.GenerateToken("0");
                            }
                           

                            return TokenManager.GenerateToken(message);
                        }
                        else
                        {

                            tmpObject = entity.services.Where(p => p.serviceId == newObject.serviceId).FirstOrDefault();
                            tmpObject.serviceId = newObject.serviceId;
                            tmpObject.name = newObject.name;
                            tmpObject.notes = newObject.notes;
                            tmpObject.isActive = newObject.isActive;
                            tmpObject.price = newObject.price;
                            tmpObject.cost = newObject.cost;
                            tmpObject.categoryId = newObject.categoryId;
                          //  tmpObject.createDate = newObject.createDate;
                            tmpObject.updateDate = DateTime.Now;
                          //  tmpObject.createUserId = newObject.createUserId;
                            tmpObject.updateUserId = newObject.updateUserId;

                            entity.SaveChanges();
                            message = tmpObject.serviceId.ToString();
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
                Boolean final = false;
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
                if (final)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            services Obj = entity.services.Find(Id);

                            entity.services.Remove(Obj);
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
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            services  Obj = entity.services.Find(Id);

                             Obj.isActive = 0;
                             Obj.updateUserId = userId;
                             Obj.updateDate = DateTime.Now;
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
            }
        }
        public int SaveIUSbyServiceId(services tmpObject)
        {
            int res = 0;
            using (incposdbEntities entity = new incposdbEntities())
            {
                //save itemsUnits
                var IUAll = entity.itemsUnits.Select(X => new ItemsUnitsServicesModel { itemUnitId = X.itemUnitId, unitName = X.units.name, categoryId = X.items.categoryId }).ToList();
                var IU = IUAll.Where(X => X.unitName == "saleUnit" && X.categoryId== tmpObject.categoryId).ToList();
                //saleUnit
                foreach (ItemsUnitsServicesModel itemUnitId in IU)
                {
                    ItemsUnitsServices newIUS = new ItemsUnitsServices();
                    newIUS.createUserId = tmpObject.createUserId;
                    newIUS.updateUserId = tmpObject.createUserId;
                    newIUS.serviceId = tmpObject.serviceId;
                    newIUS.itemUnitId = itemUnitId.itemUnitId;
                    newIUS.cost = 0;
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