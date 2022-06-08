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
    [RoutePrefix("api/itransIUServices")]
    public class itransIUServicesController : ApiController
    {
        // GET api/<controller> get all itransIUServices
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
                    var  List = entity.itransIUServices

                   .Select(S => new itransIUServicesModel
                   {
                       itransIUServiceId = S.itransIUServiceId,
                       itemsTransId = S.itemsTransId,
                       subServiceId = S.subServiceId,
                       itemUnitServiceId = S.itemUnitServiceId,
                       offerId = S.offerId,
                       offerType = S.offerType,
                       offerValue = S.offerValue,
                       notes = S.notes,
                       createDate = S.createDate,
                       updateDate = S.updateDate,
                       createUserId = S.createUserId,
                       updateUserId = S.updateUserId,

                   })
                   .ToList();
          
                    // can delet or not
                    if (List.Count > 0)
                    {
                        //foreach (itransIUServicesModel item in List)
                        //{
                        //    canDelete = false;
                        //    if (item.isActive == 1)
                        //    {
                        //        int itransIUServiceId = (int)item.itransIUServiceId;
                        //        var IU = entity.itransIUServices.Where(x => x.itransIUServiceId == itransIUServiceId).Select(x => new { x.itransIUServiceId }).FirstOrDefault();
                        //        var Sub = entity.subServices.Where(x => x.itransIUServiceId == itransIUServiceId).Select(x => new { x.subServiceId }).FirstOrDefault();

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
                    var item = entity.itransIUServices
                   .Where(S => S.itransIUServiceId == Id)
                   .Select(S => new
                   {
                       S.itransIUServiceId,
                       S.itemsTransId,
                       S.subServiceId,
                       S.itemUnitServiceId,
                       S.offerId,
                       S.offerType,
                       S.offerValue,
                       S.notes,
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
                string  Obj = "";
                itransIUServices newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        Obj = c.Value.Replace("\\", string.Empty);
                        Obj = Obj.Trim('"');
                        newObject = JsonConvert.DeserializeObject<itransIUServices>(Obj, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
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
                    if (newObject.itemsTransId == 0 || newObject.itemsTransId == null)
                    {
                        Nullable<int> id = null;
                        newObject.itemsTransId = id;
                    }
                    if (newObject.subServiceId == 0 || newObject.subServiceId == null)
                    {
                        Nullable<int> id = null;
                        newObject.subServiceId = id;
                    }
                    if (newObject.itemUnitServiceId == 0 || newObject.itemUnitServiceId == null)
                    {
                        Nullable<int> id = null;
                        newObject.itemUnitServiceId = id;
                    }
                    if (newObject.offerId == 0 || newObject.offerId == null)
                    {
                        Nullable<int> id = null;
                        newObject.offerId = id;
                    }



                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        itransIUServices tmpObject = new itransIUServices();
                        var  Entityobj = entity.Set<itransIUServices>();
                        if (newObject.itransIUServiceId == 0)
                        {

                            newObject.createDate = DateTime.Now;
                            newObject.updateDate = newObject.createDate;
                            newObject.updateUserId = newObject.createUserId;

                            tmpObject = Entityobj.Add(newObject);
                            entity.SaveChanges();
                            message = tmpObject .itransIUServiceId.ToString();
                            return TokenManager.GenerateToken(message);
                        }
                        else
                        {

                            tmpObject = entity.itransIUServices.Where(p => p.itransIUServiceId == newObject.itransIUServiceId).FirstOrDefault();
                            tmpObject.itransIUServiceId = newObject.itransIUServiceId;
                            tmpObject.itemsTransId = newObject.itemsTransId;
                            tmpObject.subServiceId = newObject.subServiceId;
                            tmpObject.itemUnitServiceId = newObject.itemUnitServiceId;
                            tmpObject.offerId = newObject.offerId;
                            tmpObject.offerType = newObject.offerType;
                            tmpObject.offerValue = newObject.offerValue;
                            tmpObject.notes = newObject.notes;
                         //   tmpObject.createDate = newObject.createDate;
                            tmpObject.updateDate = DateTime.Now;
                          //  tmpObject.createUserId = newObject.createUserId;
                            tmpObject.updateUserId = newObject.updateUserId;

                            entity.SaveChanges();
                            message = tmpObject.itransIUServiceId.ToString();
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
                            itransIUServices Obj = entity.itransIUServices.Find(Id);

                            entity.itransIUServices.Remove(Obj);
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
                //            itransIUServices coupObj = entity.itransIUServices.Find(Id);

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

        public int Save(itransIUServices newObject)
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
                    if (newObject.itemsTransId == 0 || newObject.itemsTransId == null)
                    {
                        Nullable<int> id = null;
                        newObject.itemsTransId = id;
                    }
                    if (newObject.subServiceId == 0 || newObject.subServiceId == null)
                    {
                        Nullable<int> id = null;
                        newObject.subServiceId = id;
                    }
                if (newObject.itemUnitServiceId == 0 || newObject.itemUnitServiceId == null)
                {
                    Nullable<int> id = null;
                    newObject.itemUnitServiceId = id;
                }
                if (newObject.offerId == 0 || newObject.offerId == null)
                {
                    Nullable<int> id = null;
                    newObject.offerId = id;
                }
                using (incposdbEntities entity = new incposdbEntities())
                    {
                        itransIUServices tmpObject = new itransIUServices();
                        var Entityobj = entity.Set<itransIUServices>();
                        if (newObject.itransIUServiceId == 0)
                        {

                            newObject.createDate = DateTime.Now;
                            newObject.updateDate = newObject.createDate;
                            newObject.updateUserId = newObject.createUserId;

                            tmpObject = Entityobj.Add(newObject);
                            entity.SaveChanges();
                            message = tmpObject.itransIUServiceId;
                            return message;
                        }
                        else
                        {

                            tmpObject = entity.itransIUServices.Where(p => p.itransIUServiceId == newObject.itransIUServiceId).FirstOrDefault();
                        tmpObject.itransIUServiceId = newObject.itransIUServiceId;
                        tmpObject.itemsTransId = newObject.itemsTransId;
                        tmpObject.subServiceId = newObject.subServiceId;
                        tmpObject.itemUnitServiceId = newObject.itemUnitServiceId;
                        tmpObject.offerId = newObject.offerId;
                        tmpObject.offerType = newObject.offerType;
                        tmpObject.offerValue = newObject.offerValue;
                        tmpObject.notes = newObject.notes;
                      //  tmpObject.createDate = newObject.createDate;
                        tmpObject.updateDate = DateTime.Now;
                       // tmpObject.createUserId = newObject.createUserId;
                        tmpObject.updateUserId = newObject.updateUserId;

                        entity.SaveChanges();
                            message = tmpObject.itransIUServiceId;
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

    }
}