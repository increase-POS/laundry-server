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
    [RoutePrefix("api/subServices")]
    public class subServicesController : ApiController
    {
        // GET api/<controller> get all subServices
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
                    var List = entity.subServices

                   .Select(S => new subServicesModel
                   {
                       subServiceId = S.subServiceId,
                       agentSubId = S.agentSubId,
                       subId = S.subId,
                       serviceId = S.serviceId,
                       serviceType = S.serviceType,
                       count = S.count,
                       remain = S.remain,
                       createDate = S.createDate,
                       updateDate = S.updateDate,
                       createUserId = S.createUserId,
                       updateUserId = S.updateUserId,
                   })
                   .ToList();
             
                    // can delet or not
                    //if (List.Count > 0)
                    //{
                    //    foreach (subServicesModel item in List)
                    //    {
                    //        canDelete = false;
                    //        if (item.isActive == 1)
                    //        {
                    //            int subServiceId = (int)item.subServiceId;
                    //            var ag = entity.subServices.Where(x => x.subServiceId == subServiceId).Select(x => new { x.subServiceId }).FirstOrDefault();
                               
                    //            if ((ag is null  ))
                    //                canDelete = true;
                    //        }
                    //        item.canDelete = canDelete;
                    //    }
                    //}


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
                    var item = entity.subServices
                   .Where(S => S.subServiceId == Id)
                   .Select(S => new
                   {
                       S.subServiceId,
                       S.agentSubId,
                       S.subId,
                       S.serviceId,
                       S.serviceType,
                       S.count,
                       S.remain,
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
                subServices newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        Obj = c.Value.Replace("\\", string.Empty);
                        Obj = Obj.Trim('"');
                        newObject = JsonConvert.DeserializeObject<subServices>(Obj, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
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
                    
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        subServices tmpObject = new subServices();
                        var Entityobj = entity.Set<subServices>();
                        if (newObject.subServiceId == 0)
                        {

                            newObject.createDate = DateTime.Now;
                            newObject.updateDate = newObject.createDate;
                            newObject.updateUserId = newObject.createUserId;

                            tmpObject = Entityobj.Add(newObject);
                            entity.SaveChanges();
                            message = tmpObject.subServiceId.ToString();
                         
                          
                           

                            return TokenManager.GenerateToken(message);
                        }
                        else
                        {

                            tmpObject = entity.subServices.Where(p => p.subServiceId == newObject.subServiceId).FirstOrDefault();
                            tmpObject.updateDate = DateTime.Now;
                            tmpObject.subServiceId = newObject.subServiceId;
                            tmpObject.agentSubId = newObject.agentSubId;
                            tmpObject.subId = newObject.subId;
                            tmpObject.serviceId = newObject.serviceId;
                            tmpObject.serviceType = newObject.serviceType;
                            tmpObject.count = newObject.count;
                            tmpObject.remain = newObject.remain;
                        //    tmpObject.createDate = newObject.createDate;
                          //  tmpObject.updateDate = newObject.updateDate;
                          //  tmpObject.createUserId = newObject.createUserId;
                            tmpObject.updateUserId = newObject.updateUserId;

                            //  tmpObject.createDate = newObject.createDate;

                            //  tmpObject.createUserId = newObject.createUserId;
                            tmpObject.updateUserId = newObject.updateUserId;

                            entity.SaveChanges();
                            message = tmpObject.subServiceId.ToString();
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
                            subServices Obj = entity.subServices.Find(Id);

                            entity.subServices.Remove(Obj);
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
                //            subServices  Obj = entity.subServices.Find(Id);

                //            Obj.isActive = 0;
                //             Obj.updateUserId = userId;
                //             Obj.updateDate = DateTime.Now;
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
       



    }
}