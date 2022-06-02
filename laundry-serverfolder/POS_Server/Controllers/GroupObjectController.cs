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
    [RoutePrefix("api/GroupObject")]
    public class GroupObjectController : ApiController
    {
        // GET api/<controller> get all Group
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


                    try
                    {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var List = (from c in entity.groupObject
                                    join o in entity.objects on c.objectId equals o.objectId
                                    join p in entity.objects on o.parentObjectId equals p.objectId
                                    select new
                                    {
                                        id = c.id,
                                        groupId = c.groupId,
                                        objectId = c.objectId,
                                        notes = c.notes,
                                        addOb = c.addOb,
                                        updateOb = c.updateOb,
                                        deleteOb = c.deleteOb,
                                        showOb = c.showOb,
                                        createDate = c.createDate,
                                        updateDate = c.updateDate,
                                        createUserId = c.createUserId,
                                        updateUserId = c.updateUserId,
                                        canDelete = true,
                                        reportOb = c.reportOb,
                                        levelOb = c.levelOb,
                                        parentObjectId = o.parentObjectId,
                                        objectName = o.name,
                                        parentObjectName = p.name,
                                          o.objectType,
                                        
                                    }).ToList();



                        return TokenManager.GenerateToken(List);
                    }
                }
                catch
                    {
                        return TokenManager.GenerateToken("0");
                    }
                }


                //            var re = Request;
                //            var headers = re.Headers;
                //            string token = "";

                //            if (headers.Contains("APIKey"))
                //            {
                //                token = headers.GetValues("APIKey").First();
                //            }
                //            Validation validation = new Validation();
                //            bool valid = validation.CheckApiKey(token);

                //            if (valid) // APIKey is valid
                //            {
                //                using (incposdbEntities entity = new incposdbEntities())
                //                {
                //                    var List = (from c in entity.groupObject
                //                                join o in entity.objects on c.objectId equals o.objectId
                //                                join p in entity.objects on o.parentObjectId equals p.objectId
                //                                select new
                //                                {
                //                                    id = c.id,
                //                                    groupId = c.groupId,
                //                                    objectId = c.objectId,
                //                                    notes = c.notes,
                //                                    addOb = c.addOb,
                //                                    updateOb = c.updateOb,
                //                                    deleteOb = c.deleteOb,
                //                                    showOb = c.showOb,

                //                                    desc = c.objects.note,

                //                                    createDate = c.createDate,
                //                                    updateDate = c.updateDate,
                //                                    createUserId = c.createUserId,
                //                                    updateUserId = c.updateUserId,
                //                                    canDelete = true,
                //                                    reportOb = c.reportOb,
                //                                    levelOb = c.levelOb,
                //                                    parentObjectId = o.parentObjectId,
                //                                    objectName = o.name,
                //                                    parentObjectName=p.name,

                //                                })
                //                               .ToList();

                //                    /*
                //                     * 

                //id
                //groupId
                //objectId
                //notes
                //addOb
                //updateOb
                //deleteOb
                //showOb
                //createDate
                //updateDate
                //createUserId
                //updateUserId


                //                     * */

                //                    if (List == null)
                //                        return NotFound();
                //                    else
                //                        return Ok(List);
                //                }
                //            }
                //            //else
                //                return NotFound();
            }
       

        // add or update 
        [HttpPost]
        [Route("Save")]
        public String Save(string token)
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
                groupObject newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<groupObject>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {


                 //   bondes tmpObject = null;


                    try
                    {

                        if (newObject.groupId == 0 || newObject.groupId == null)
                        {
                            Nullable<int> id = null;
                            newObject.groupId = id;
                        }

                        if (newObject.objectId == 0 || newObject.objectId == null)
                        {
                            Nullable<int> id = null;
                            newObject.objectId = id;
                        }


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
                            var sEntity = entity.Set<groupObject>();
                            if (newObject.id == 0)
                            {
                                newObject.createDate = DateTime.Now;
                                newObject.updateDate = DateTime.Now;
                                newObject.updateUserId = newObject.createUserId;



                                sEntity.Add(newObject);
                                message = newObject.groupId.ToString();
                                entity.SaveChanges();
                            }
                            else
                            {

                                var tmps = entity.groupObject.Where(p => p.id == newObject.id).FirstOrDefault();
                                tmps.id = newObject.id;
                                tmps.groupId = newObject.groupId;
                                tmps.objectId = newObject.objectId;
                                tmps.notes = newObject.notes;
                                tmps.addOb = newObject.addOb;
                                tmps.updateOb = newObject.updateOb;
                                tmps.deleteOb = newObject.deleteOb;
                                tmps.showOb = newObject.showOb;
                                //tmps.isActive = newObject.isActive;
                                tmps.reportOb = newObject.reportOb;
                                tmps.levelOb = newObject.levelOb;

                                tmps.createDate = newObject.createDate;
                                tmps.updateDate = DateTime.Now;// server current date
                                tmps.updateUserId = newObject.updateUserId;
                                entity.SaveChanges();
                                message = tmps.id.ToString();
                            }


                        }
                        //return message;

                        return TokenManager.GenerateToken(message);

                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }


                }

            return TokenManager.GenerateToken(message);

            }


            //var re = Request;
            //var headers = re.Headers;
            //string token = "";
            //string message ="";
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);
            
            //if (valid)
            //{
            //    newObject = newObject.Replace("\\", string.Empty);
            //    newObject = newObject.Trim('"');
            //   groupObject Object = JsonConvert.DeserializeObject<groupObject>(newObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
            //    try
            //    {

            //        if (Object.groupId == 0 || Object.groupId == null)
            //        {
            //            Nullable<int> id = null;
            //            Object.groupId = id;
            //        }

            //        if (Object.objectId == 0 || Object.objectId == null)
            //        {
            //            Nullable<int> id = null;
            //            Object.objectId = id;
            //        }


            //        if (Object.updateUserId == 0 || Object.updateUserId == null)
            //        {
            //            Nullable<int> id = null;
            //            Object.updateUserId = id;
            //        }
            //        if (Object.createUserId == 0 || Object.createUserId == null)
            //        {
            //            Nullable<int> id = null;
            //            Object.createUserId = id;
            //        }
            //        using (incposdbEntities entity = new incposdbEntities())
            //        {
            //            var sEntity = entity.Set<groupObject>();
            //            if (Object.id == 0)
            //            {
            //                Object.createDate = DateTime.Now;
            //                Object.updateDate = DateTime.Now;
            //                Object.updateUserId = Object.createUserId;
                    


            //                sEntity.Add(Object);
            //                 message = Object.groupId.ToString();
            //                entity.SaveChanges();
            //            }
            //            else
            //            {

            //                var tmps = entity.groupObject.Where(p => p.id == Object.id).FirstOrDefault();
            //                tmps.id = Object.id;
            //                tmps.groupId = Object.groupId;
            //                tmps.objectId = Object.objectId;
            //                tmps.notes = Object.notes;
            //                tmps.addOb = Object.addOb;
            //                tmps.updateOb = Object.updateOb;
            //                tmps.deleteOb = Object.deleteOb;
            //                tmps.showOb = Object.showOb;
            //                //tmps.isActive = Object.isActive;
            //                tmps.reportOb = Object.reportOb;
            //                tmps.levelOb = Object.levelOb;
                            
            //                tmps.createDate=Object.createDate;
            //                tmps.updateDate = DateTime.Now;// server current date
            //                tmps.updateUserId = Object.updateUserId;
            //                entity.SaveChanges();
            //                message = tmps.id.ToString();
            //            }
                       
                       
            //        }
            //        return message; ;
            //    }

            //    catch
            //    {
            //        return "-1";
            //    }
            //}
            //else
            //    return "-1";
        }
    
        //
        //per
        [HttpPost]
        [Route("GetUserpermission")]
        public string GetUserpermission(string token)
        {

          token = TokenManager.readToken(HttpContext.Current.Request); 
 var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
                else
                {
                    int userId = 0;


                    IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                    foreach (Claim c in claims)
                    {
                        if (c.Type == "userId")
                        {
                        userId = int.Parse(c.Value);
                        }


                    }

                    // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                    try
                    {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var list = (from GO in entity.groupObject
                                    join U in entity.users on GO.groupId equals U.groupId
                                    join G in entity.groups on GO.groupId equals G.groupId
                                    join O in entity.objects on GO.objectId equals O.objectId
                                    join POO in entity.objects on O.parentObjectId equals POO.objectId into JP

                                    from PO in JP.DefaultIfEmpty()
                                    where (U.userId == userId && G.isActive == 1 )
                                    select new
                                    {
                                        //group object
                                        GO.id,
                                        GO.groupId,
                                        GO.objectId,
                                        GO.addOb,
                                        GO.updateOb,
                                        GO.deleteOb,
                                        GO.showOb,
                                        GO.reportOb,
                                        GO.levelOb,
                                        //group 
                                        GroupName = G.name,
                                        //object
                                        ObjectName = O.name,
                                        O.parentObjectId,
                                        O.objectType,
                                        parentObjectName = PO.name,

                                    }).ToList();


                        return TokenManager.GenerateToken(list);
                    }
                    }
                catch
                    {
                        return TokenManager.GenerateToken("0");
                    }

                }

            //    var re = Request;
            //var headers = re.Headers;
            //string token = "";

            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}

            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid)
            //{
            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        var list = (from GO in entity.groupObject
            //                    join U in entity.users on GO.groupId equals U.groupId
            //                    join G in entity.groups on GO.groupId equals G.groupId
            //                    join O in entity.objects on GO.objectId equals O.objectId
            //                    join POO in entity.objects on O.parentObjectId equals POO.objectId into JP

            //                    from PO in JP.DefaultIfEmpty()
            //                    where U.userId == userId
            //                    select new
            //                    {
            //                        //group object
            //                        GO.id,
            //                        GO.groupId,
            //                        GO.objectId,
            //                        GO.addOb,
            //                        GO.updateOb,
            //                        GO.deleteOb,
            //                        GO.showOb,
            //                        GO.reportOb,
            //                        GO.levelOb,
            //                        //group 
            //                        GroupName = G.name,
            //                        //object
            //                        ObjectName = O.name,
            //                        O.parentObjectId,
            //                        O.objectType,
            //                        parentObjectName = PO.name,

            //                    }).ToList();


            //        if (list == null)
            //            return NotFound();
            //        else
            //            return Ok(list);

            //    }
            //}
            //else
            //    return NotFound();


        }

        //

    }
}