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
    [RoutePrefix("api/Group")]
    public class GroupController : ApiController
    {
        // GET api/<controller> get all Group

        [HttpPost]
        [Route("Get")]
        public string Get(string token)
        {
            //  public string Get(string token)




          token = TokenManager.readToken(HttpContext.Current.Request); 
 var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                bool canDelete = false;

                //int mainBranchId = 0;
                //int userId = 0;

                //IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                //foreach (Claim c in claims)
                //{
                //    if (c.Type == "mainBranchId")
                //    {
                //        mainBranchId = int.Parse(c.Value);
                //    }
                //    else if (c.Type == "userId")
                //    {
                //        userId = int.Parse(c.Value);
                //    }

                //}


                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var List = entity.groups

                       .Select(c => new GroupModel
                       {
                           groupId = c.groupId,
                           name = c.name,
                           notes = c.notes,
                           createDate = c.createDate,
                           updateDate = c.updateDate,
                           createUserId = c.createUserId,
                           updateUserId = c.updateUserId,
                           isActive = c.isActive,

                       })
                       .ToList();
                        if (List.Count > 0)
                        {
                            for (int i = 0; i < List.Count; i++)
                            {
                                canDelete = false;
                                if (List[i].isActive == 1)
                                {
                                    int groupId = (int)List[i].groupId;
                                    // var operationsL = entity.groupObject.Where(x => x.groupId == groupId).Select(b => new { b.id }).FirstOrDefault();
                                    var operationsu = entity.users.Where(x => x.groupId == groupId).Select(b => new { b.groupId }).FirstOrDefault();
                                    if (operationsu is null)
                                        canDelete = true;
                                }
                                List[i].canDelete = canDelete;
                            }
                        }
                 

                      
                        return TokenManager.GenerateToken(List);
                    }
             
                   

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }


            //var re = Request;
            //var headers = re.Headers;
            //string token = "";
            //bool canDelete = false;
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid) // APIKey is valid
            //{
            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        var List = entity.groups

            //       .Select(c => new GroupModel {
            //           groupId=  c.groupId,
            //           name= c.name,
            //           notes = c.notes,
            //           createDate = c.createDate,
            //           updateDate = c.updateDate,
            //           createUserId = c.createUserId,
            //           updateUserId = c.updateUserId,
            //           isActive = c.isActive,

            //       })
            //       .ToList();
            //        if (List.Count > 0)
            //        {
            //            for (int i = 0; i < List.Count; i++)
            //            {
            //                canDelete = false;
            //                if (List[i].isActive == 1)
            //                {
            //                    int groupId = (int)List[i].groupId;
            //                   // var operationsL = entity.groupObject.Where(x => x.groupId == groupId).Select(b => new { b.id }).FirstOrDefault();
            //                    var operationsu = entity.users.Where(x => x.groupId == groupId).Select(b => new { b.groupId }).FirstOrDefault();
            //                    if (operationsu is null)
            //                        canDelete = true;
            //                }
            //                List[i].canDelete = canDelete;
            //            }
            //        }
            //        /*
            //         * 



            //         * */

            //        if (List == null)
            //            return NotFound();
            //        else
            //            return Ok(List);
            //    }
            //}
            ////else
            //    return NotFound();
        }



        // GET api/<controller>  Get medal By ID 
        //[HttpPost]
        //[Route("GetByID")]
        //public IHttpActionResult GetByID()
        //{
        //    var re = Request;
        //    var headers = re.Headers;
        //    string token = "";
        //    int cId = 0;
        //    if (headers.Contains("APIKey"))
        //    {
        //        token = headers.GetValues("APIKey").First();
        //    }
        //    if (headers.Contains("Id"))
        //    {
        //        cId = Convert.ToInt32(headers.GetValues("Id").First());
        //    }
        //    Validation validation = new Validation();
        //    bool valid = validation.CheckApiKey(token);

        //    if (valid)
        //    {
        //        using (incposdbEntities entity = new incposdbEntities())
        //        {
        //            var list = entity.groups
        //           .Where(c => c.groupId == cId)
        //           .Select(c => new {
        //               c.groupId,
        //               c.name,
        //               c.notes,
        //               c.createDate,
        //               c.updateDate,
        //               c.createUserId,
        //               c.updateUserId,

        //             c.isActive,
        //           })
        //           .FirstOrDefault();

        //            if (list == null)
        //                return NotFound();
        //            else
        //                return Ok(list);
        //        }
        //    }
        //    else
        //        return NotFound();
        //}


        // GET api/<controller>  ارجاع قائمة المستخدمين التابعين للمجموعة
        [HttpPost]
        [Route("GetUsersByGroupId")]
        public string GetUsersByGroupId(string token)
        {

          token = TokenManager.readToken(HttpContext.Current.Request);var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int groupId = 0;


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "groupId")
                    {
                        groupId = int.Parse(c.Value);
                    }


                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var list = entity.users
                       .Where(c => c.groupId == groupId)
                       .Select(c => new
                       {
                           c.userId,
                           c.groupId,
                           c.name,
                           c.notes,
                           c.createDate,
                           c.updateDate,
                           c.createUserId,
                           c.updateUserId,

                           c.isActive,
                           c.username,
                           c.password,
                           c.lastname,
                           c.job,
                           c.workHours,
                           c.phone,
                           c.mobile,
                           c.email,
                           c.address,
                           c.isOnline,
                        
                           c.image,
                           c.balance,
                           c.balanceType,

                       })
                                   .ToList();

                        return TokenManager.GenerateToken(list);
                       
                    }
              

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }
            //var re = Request;
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
            //        var list = entity.users
            //       .Where(c => c.groupId == groupId)
            //       .Select(c => new {
            //           c.userId,
            //           c.groupId,
            //           c.name,
            //           c.notes,
            //           c.createDate,
            //           c.updateDate,
            //           c.createUserId,
            //           c.updateUserId,

            //           c.isActive,
            //           c.username,
            //           c.password,
            //           c.lastname,
            //           c.job,
            //           c.workHours,
            //           c.phone,
            //           c.mobile,
            //           c.email,
            //           c.address,
            //           c.isOnline,
            //           c.role,
            //           c.image,
            //           c.balance,
            //           c.balanceType,

            //       })
            //       .ToList();

            //        if (list == null)
            //            return NotFound();
            //        else
            //            return Ok(list);
            //    }
            //}
            //else
            //    return NotFound();
        }
        public string addObjects(int groupId)
        {

            string message = "";
            List<objects> ListObjects = new List<objects>();
            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    List<objects> Listtmp = entity.objects.ToList();

                    ListObjects= Listtmp.Select(c => new objects
                               {
                                   objectId = c.objectId,
                                   name = c.name,
                                 
                                   parentObjectId = c.parentObjectId,
                                   objectType = c.objectType,
                               })
                               .ToList();
                }
            }
            catch
            {
                message = "0";
                return message;
                //return ex.ToString();
            }
         groupObject   groupObjectitem = new groupObject();
            List<  groupObject > groupObjectsList = new List<groupObject>();
            foreach (var item in ListObjects)
            {
                groupObjectitem = new groupObject();
                groupObjectitem.groupId = groupId;
                groupObjectitem.objectId = item.objectId;
                if (item.objectType == "one" || item.objectType == "alert")
                {
                    groupObjectitem.showOb = 0;
                    groupObjectitem.addOb = 2;
                    groupObjectitem.updateOb = 2;
                    groupObjectitem.deleteOb = 2;
                    groupObjectitem.reportOb = 2;
                }
                else
                {
                    groupObjectitem.showOb = 0;
                    groupObjectitem.addOb = 0;
                    groupObjectitem.updateOb = 0;
                    groupObjectitem.deleteOb = 0;
                    groupObjectitem.reportOb = 0;
                }
                groupObjectitem.levelOb = 0;
                groupObjectitem.notes = "";
               // groupObjectitem.createUserId = MainWindow.userID;
               // groupObjectitem.updateUserId = MainWindow.userID;
                groupObjectitem.isActive = 1;
                groupObjectsList.Add(groupObjectitem);
            }
          
            using (incposdbEntities entity = new incposdbEntities())
            {
                if (groupObjectsList.Count > 0)
                {
                    for (int i = 0; i < groupObjectsList.Count; i++)
                    {
                        groupObjectsList[i].createDate = DateTime.Now;
                        groupObjectsList[i].updateDate = DateTime.Now;
                        entity.groupObject.Add(groupObjectsList[i]);
                        try
                        {
                            message = entity.SaveChanges().ToString();

                        }
                        catch
                        {

                            message = "0";
                            return message;
                            //return ex.ToString();
                        }
                    }
               
                }
           
            }
   return  message;
        }

        // add or update 
        [HttpPost]
        [Route("Save")]
        public String Save(string token)
        {

            //string Object
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
                groups newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<groups>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {

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
                                var sEntity = entity.Set<groups>();
                                if (newObject.groupId == 0 || newObject.groupId == null)
                                {

                                    newObject.createDate = DateTime.Now;
                                    newObject.updateDate = DateTime.Now;
                                    newObject.updateUserId = newObject.createUserId;



                                    entity.groups.Add(newObject);

                                entity.SaveChanges();
                                int gid = newObject.groupId;
                                    message = newObject.groupId.ToString();



                         string res =    addObjects(gid);
                               // return TokenManager.GenerateToken(res);

                                if (res == "0")
                                {
                                    return TokenManager.GenerateToken("0");
                                }
                                }
                                else
                                {

                                    var tmps = entity.groups.Where(p => p.groupId == newObject.groupId).FirstOrDefault();
                                    tmps.groupId = newObject.groupId;
                                    tmps.name = newObject.name;
                                    tmps.notes = newObject.notes;
                                    tmps.isActive = newObject.isActive;
                                    tmps.createDate = newObject.createDate;
                                    tmps.updateDate = DateTime.Now;// server current date
                                    tmps.updateUserId = newObject.updateUserId;
                                    entity.SaveChanges();
                                    message = tmps.groupId.ToString();
                                }


                            }
                           // return message; ;
                      


                        return TokenManager.GenerateToken(message);

                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                        //   return TokenManager.GenerateToken(ex.ToString());
                    }


                }

                return TokenManager.GenerateToken(message);

            }


            //            var re = Request;
            //            var headers = re.Headers;
            //            string token = "";
            //            string message ="";
            //            if (headers.Contains("APIKey"))
            //            {
            //                token = headers.GetValues("APIKey").First();
            //            }
            //            Validation validation = new Validation();
            //            bool valid = validation.CheckApiKey(token);

            //            if (valid)
            //            {
            //                newObject = newObject.Replace("\\", string.Empty);
            //                newObject = newObject.Trim('"');
            //               groups Object = JsonConvert.DeserializeObject<groups>(newObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
            //                try
            //                {

            //                    if (Object.updateUserId == 0 || Object.updateUserId == null)
            //                    {
            //                        Nullable<int> id = null;
            //                        Object.updateUserId = id;
            //                    }
            //                    if (Object.createUserId == 0 || Object.createUserId == null)
            //                    {
            //                        Nullable<int> id = null;
            //                        Object.createUserId = id;
            //                    }
            //                    using (incposdbEntities entity = new incposdbEntities())
            //                    {
            //                        var sEntity = entity.Set<groups>();
            //                        if (Object.groupId == 0 || Object.groupId== null)
            //                        {
            //                            Object.createDate = DateTime.Now;
            //                            Object.updateDate = DateTime.Now;
            //                            Object.updateUserId = Object.createUserId;



            //                            entity.groups.Add(Object);

            //                            entity.SaveChanges();
            //message = Object.groupId.ToString();
            //                        }
            //                        else
            //                        {

            //                            var tmps = entity.groups.Where(p => p.groupId == Object.groupId).FirstOrDefault();
            //                            tmps.groupId = Object.groupId;
            //                            tmps.name = Object.name;
            //                            tmps.notes = Object.notes;
            //                            tmps.isActive = Object.isActive;
            //                            tmps.createDate=Object.createDate;
            //                            tmps.updateDate = DateTime.Now;// server current date
            //                            tmps.updateUserId = Object.updateUserId;
            //                            entity.SaveChanges();
            //                            message = tmps.groupId.ToString();
            //                        }


            //                    }
            //                    return message; ;
            //                }

            //                catch
            //                {
            //                    return "-1";
            //                }
            //            }
            //            else
            //                return "-1";
        }

        [HttpPost]
        [Route("Delete")]
        public string Delete(string token)
        {


            // int groupId, int userId, Boolean final
            //int Id, int userId
            string message = "";



          token = TokenManager.readToken(HttpContext.Current.Request); 
 var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int groupId = 0;
                int userId = 0;
                bool final = false;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "groupId")
                    {
                        groupId = int.Parse(c.Value);
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

                            groups Deleterow = entity.groups.Find(groupId);

                            List<groupObject> delGrObject = entity.groupObject.Where(x => x.groupId == groupId).ToList();
                            entity.groupObject.RemoveRange(delGrObject);
                            entity.SaveChanges();
                            entity.groups.Remove(Deleterow);
                            message = entity.SaveChanges().ToString();

                            //  return Ok("OK");
                            return TokenManager.GenerateToken(message);
                        }
                    }
                    catch
                    {
                        return TokenManager.GenerateToken("0");
                    }
                }
                else
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {

                            groups Obj = entity.groups.Find(groupId);
                            Obj.isActive = 0;
                            Obj.updateUserId = userId;
                            Obj.updateDate = DateTime.Now;
                            message = entity.SaveChanges().ToString();
                            return TokenManager.GenerateToken(message);
                            //return Ok("Ok");
                        }
                    }
                    catch
                    {
                        return TokenManager.GenerateToken("0");
                    }

                }
            }
            //var re = Request;
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

            //    if (final)
            //    {
            //        try
            //        {
            //            using (incposdbEntities entity = new incposdbEntities())
            //            {

            //                groups Deleterow = entity.groups.Find(groupId);

            //                List<groupObject> delGrObject = entity.groupObject.Where(x => x.groupId == groupId).ToList();
            //                entity.groupObject.RemoveRange(delGrObject);
            //                entity.SaveChanges();
            //                entity.groups.Remove(Deleterow);
            //                entity.SaveChanges();
            //                return Ok("OK");
            //            }
            //        }
            //        catch
            //        {
            //            return NotFound();
            //        }
            //    }
            //    else
            //    {
            //        try
            //        {
            //            using (incposdbEntities entity = new incposdbEntities())
            //            {

            //                groups Obj = entity.groups.Find(groupId);
            //                Obj.isActive = 0;
            //                Obj.updateUserId = userId;
            //                Obj.updateDate = DateTime.Now;
            //                entity.SaveChanges();
            //                return Ok("Ok");
            //            }
            //        }
            //        catch
            //        {
            //            return NotFound();
            //        }
            //    }



            //}
            //else
            //    return NotFound();
        }

        [HttpPost]
        [Route("UpdateGroupIdInUsers")]
        public String UpdateGroupIdInUsers(string token)
        {
            //int groupId, string newList, int userId
          token = TokenManager.readToken(HttpContext.Current.Request); 
 var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string message = "";
                int groupId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
               
                    string newList = "";
                List<int> newListObj = new List<int>();
                  
                    foreach (Claim c in claims)
                    {
                        if (c.Type == "newList")
                        {
                        newList = c.Value.Replace("\\", string.Empty);
                        newList = newList.Trim('"');
                        newListObj = JsonConvert.DeserializeObject<List<int>>(newList, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                            
                        }
                        else if (c.Type == "groupId")
                        {
                        groupId = int.Parse(c.Value);
                        }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }

               

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {
                   

                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            // reset old list
                            List<users> oldList = entity.users.Where(x => x.groupId == groupId).ToList();
                            if (oldList.Count > 0)
                            {
                                for (int i = 0; i < oldList.Count; i++)
                                {
                                    oldList[i].groupId = null;
                                    oldList[i].updateUserId = userId;
                                    oldList[i].updateDate = DateTime.Now;


                                }
                                entity.SaveChanges();
                            }

                            //save new list
                            if (newListObj.Count > 0)
                            {
                                foreach (int rowId in newListObj)
                                {
                                    users userRow = entity.users.Find(rowId);
                                    userRow.updateUserId = userId;
                                    userRow.updateDate = DateTime.Now;
                                    userRow.groupId = groupId;
                                message= entity.SaveChanges().ToString();
                              
                            }
                            }
                            else
                            {
                            //message = "-1";
                            message = "0";
                           
                        }


                    }
                    //    return message; ;

                   return TokenManager.GenerateToken(message);
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }

            //var re = Request;
            //var headers = re.Headers;
            //string token = "";
            //string message = "";
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid)
            //{
            //    newList = newList.Replace("\\", string.Empty);
            //    newList = newList.Trim('"');

            //    List<int> newListObj = JsonConvert.DeserializeObject<List<int>>(newList, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
            //    try
            //    {


            //        using (incposdbEntities entity = new incposdbEntities())
            //        {
            //            // reset old list
            //            List<users> oldList = entity.users.Where(x => x.groupId == groupId).ToList();
            //            if (oldList.Count > 0)
            //            {
            //                for (int i = 0; i < oldList.Count; i++)
            //                {
            //                    oldList[i].groupId = null;
            //                    oldList[i].updateUserId = userId;
            //                    oldList[i].updateDate = DateTime.Now;


            //                }
            //                entity.SaveChanges();
            //            }

            //            //save new list
            //            if (newListObj.Count > 0)
            //            {
            //                foreach (int rowId in newListObj)
            //                {
            //                    users userRow = entity.users.Find(rowId);
            //                    userRow.updateUserId = userId;
            //                    userRow.updateDate = DateTime.Now;
            //                    userRow.groupId = groupId;
            //                    entity.SaveChanges();
            //                }
            //            }
            //            else
            //            {
            //                message = "-1";
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

    }
}