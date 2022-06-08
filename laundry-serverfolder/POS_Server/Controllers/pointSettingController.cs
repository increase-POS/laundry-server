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
    [RoutePrefix("api/pointSetting")]
    public class pointSettingController : ApiController
    {
        // GET api/<controller> get all pointSetting
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
                    var List = entity.pointSetting

                   .Select(S => new pointSettingModel
                   {
                       pointSettingId = S.pointSettingId,
                       points = S.points,
                       pointsAccum = S.pointsAccum,
                       createDate = S.createDate,
                       updateDate = S.updateDate,
                       createUserId = S.createUserId,
                       updateUserId = S.updateUserId,
                       notes = S.notes,
                       isActive = S.isActive,
                   })
                   .ToList();
             
                    // can delet or not
                    //if (List.Count > 0)
                    //{
                    //    foreach (pointSettingModel item in List)
                    //    {
                    //        canDelete = false;
                    //        if (item.isActive == 1)
                    //        {
                    //            int pointSettingId = (int)item.pointSettingId;
                    //            var ag = entity.agentsSub.Where(x => x.pointSettingId == pointSettingId).Select(x => new { x.agentSubId }).FirstOrDefault();
                               
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
                    var item = entity.pointSetting
                   .Where(S => S.pointSettingId == Id)
                   .Select(S => new
                   {
                       S.pointSettingId,
                       S.points,
                       S.pointsAccum,
                       S.createDate,
                       S.updateDate,
                       S.createUserId,
                       S.updateUserId,
                       S.notes,
                       S.isActive,



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
                pointSetting newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        Obj = c.Value.Replace("\\", string.Empty);
                        Obj = Obj.Trim('"');
                        newObject = JsonConvert.DeserializeObject<pointSetting>(Obj, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
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
                        pointSetting tmpObject = new pointSetting();
                        var Entityobj = entity.Set<pointSetting>();
                        if (newObject.pointSettingId == 0)
                        {

                            newObject.createDate = DateTime.Now;
                            newObject.updateDate = newObject.createDate;
                            newObject.updateUserId = newObject.createUserId;

                            tmpObject = Entityobj.Add(newObject);
                            entity.SaveChanges();
                            message = tmpObject.pointSettingId.ToString();
                         
                          
                           

                            return TokenManager.GenerateToken(message);
                        }
                        else
                        {

                            tmpObject = entity.pointSetting.Where(p => p.pointSettingId == newObject.pointSettingId).FirstOrDefault();
                            tmpObject.pointSettingId = newObject.pointSettingId;
                            tmpObject.points = newObject.points;
                            tmpObject.pointsAccum = newObject.pointsAccum;
                         //   tmpObject.createDate = newObject.createDate;
                            tmpObject.updateDate = DateTime.Now; ;
                        //    tmpObject.createUserId = newObject.createUserId;
                            tmpObject.updateUserId = newObject.updateUserId;
                            tmpObject.notes = newObject.notes;
                            tmpObject.isActive = newObject.isActive;


                            entity.SaveChanges();
                            message = tmpObject.pointSettingId.ToString();
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
                if (final)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            pointSetting Obj = entity.pointSetting.Find(Id);

                            entity.pointSetting.Remove(Obj);
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
                            pointSetting  Obj = entity.pointSetting.Find(Id);

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
       



    }
}