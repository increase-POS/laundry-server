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
    [RoutePrefix("api/agentsSub")]
    public class agentsSubController : ApiController
    {
        // GET api/<controller> get all agentsSub
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
                    var List = entity.agentsSub

                   .Select(S => new agentsSubModel
                   {
                       agentSubId = S.agentSubId,
                       agentId = S.agentId,
                       subId = S.subId,
                       isLimited = S.isLimited,
                       endDate = S.endDate,
                       isActive = S.isActive,
                       notes = S.notes,
                       createDate = S.createDate,
                       updateDate = S.updateDate,
                       createUserId = S.createUserId,
                       updateUserId = S.updateUserId,



                   })
                   .ToList();
             
                    // can delet or not
                    //if (List.Count > 0)
                    //{
                    //    foreach (agentsSubModel item in List)
                    //    {
                    //        canDelete = false;
                    //        if (item.isActive == 1)
                    //        {
                    //            int agentSubId = (int)item.agentSubId;
                    //            var ag = entity.agentsSub.Where(x => x.agentSubId == agentSubId).Select(x => new { x.agentSubId }).FirstOrDefault();
                               
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
                    var item = entity.agentsSub
                   .Where(S => S.agentSubId == Id)
                   .Select(S => new
                   {
                       S.agentSubId,
                       S.agentId,
                       S.subId,
                       S.isLimited,
                       S.endDate,
                       S.isActive,
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
                string Obj = "";
                agentsSub newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        Obj = c.Value.Replace("\\", string.Empty);
                        Obj = Obj.Trim('"');
                        newObject = JsonConvert.DeserializeObject<agentsSub>(Obj, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
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
                        agentsSub tmpObject = new agentsSub();
                        var Entityobj = entity.Set<agentsSub>();
                        if (newObject.agentSubId == 0)
                        {

                            newObject.createDate = DateTime.Now;
                            newObject.updateDate = newObject.createDate;
                            newObject.updateUserId = newObject.createUserId;

                            tmpObject = Entityobj.Add(newObject);
                            entity.SaveChanges();
                            message = tmpObject.agentSubId.ToString();
                         
                          
                           

                            return TokenManager.GenerateToken(message);
                        }
                        else
                        {

                            tmpObject = entity.agentsSub.Where(p => p.agentSubId == newObject.agentSubId).FirstOrDefault();
                            tmpObject.updateDate = DateTime.Now;
                            tmpObject.agentSubId = newObject.agentSubId;
                            tmpObject.agentId = newObject.agentId;
                            tmpObject.subId = newObject.subId;
                            tmpObject.isLimited = newObject.isLimited;
                            tmpObject.endDate = newObject.endDate;
                            tmpObject.isActive = newObject.isActive;
                            tmpObject.notes = newObject.notes;
                          //  tmpObject.createDate = newObject.createDate;
                          
                          //  tmpObject.createUserId = newObject.createUserId;
                            tmpObject.updateUserId = newObject.updateUserId;

                            entity.SaveChanges();
                            message = tmpObject.agentSubId.ToString();
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
                            agentsSub Obj = entity.agentsSub.Find(Id);

                            entity.agentsSub.Remove(Obj);
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
                //            agentsSub  Obj = entity.agentsSub.Find(Id);

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