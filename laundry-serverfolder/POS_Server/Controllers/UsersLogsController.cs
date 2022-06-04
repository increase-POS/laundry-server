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

    [RoutePrefix("api/UsersLogs")]
    public class UsersLogsController : ApiController
    {
        
      
        // GET api/<controller> 
        [HttpPost]
        [Route("GetByID")]
        public string GetByID(string token)
        {     
            token = TokenManager.readToken(HttpContext.Current.Request); 
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int logId = 0;


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "logId") 
                    {
                        logId = int.Parse(c.Value);
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var item = entity.usersLogs
                       .Where(u => u.logId == logId)
                       .Select(S => new
                       {
                           S.logId,
                           S.sInDate,
                           S.sOutDate,
                           S.posId,
                           S.userId,
                       })
                       .FirstOrDefault();
                        return TokenManager.GenerateToken(item);
                       // return TokenManager.GenerateToken(item);
                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                 //    return TokenManager.GenerateToken("0");
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
            //        var row = entity.usersLogs
            //       .Where(u => u.logId == logId)
            //       .Select(S => new
            //       {

            //              S.logId,
            //              S.sInDate,
            //              S.sOutDate,
            //              S.posId,
            //              S.userId,


            //       })
            //       .FirstOrDefault();

            //        if (row == null)
            //            return NotFound();
            //        else
            //            return Ok(row);
            //    }
            //}
            //else
            //    return NotFound();
        }


      

        // add or update location
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
                string Object = "";
                usersLogs newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<usersLogs>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {


                    usersLogs tmpObject = null;


                    try
                    {
                        if (newObject.posId == 0 || newObject.posId == null)
                        {
                            Nullable<int> id = null;
                            newObject.posId = id;
                        }
                        if (newObject.userId == 0 || newObject.userId == null)
                        {
                            Nullable<int> id = null;
                            newObject.userId = id;
                        }
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var locationEntity = entity.Set<usersLogs>();
                            if (newObject.logId == 0 || newObject.logId == null)
                            {
                                // signIn

                                newObject.sInDate = DateTime.Now;


                                locationEntity.Add(newObject);
                                entity.SaveChanges();
                                message = newObject.logId.ToString();
                                //sign out old user

                                using (incposdbEntities entity2 = new incposdbEntities())
                                {
                                    List<usersLogs> ul = new List<usersLogs>();
                                    List<usersLogs> locationE = entity2.usersLogs.ToList();
                                    ul = locationE.Where(s => s.sOutDate == null &&
                                   ((DateTime.Now - (DateTime)s.sInDate).TotalHours >= 8)).ToList();
                                    if (ul != null)
                                    {
                                        foreach (usersLogs row in ul)
                                        {
                                            row.sOutDate = DateTime.Now;
                                            entity2.SaveChanges();
                                        }
                                    }
                                }

                            }



                            else
                            {//signOut
                                tmpObject = entity.usersLogs.Where(p => p.logId == newObject.logId).FirstOrDefault();



                                tmpObject.logId = newObject.logId;
                                //  tmpObject.sInDate=newObject.sInDate;
                                tmpObject.sOutDate = DateTime.Now;
                                //    tmpObject.posId=newObject.posId;
                                //  tmpObject.userId = newObject.userId;


                                entity.SaveChanges();

                                message = tmpObject.logId.ToString();
                            }
                            //  entity.SaveChanges();
                        }

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
            //string message = "";
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid)
            //{
            //    Object = Object.Replace("\\", string.Empty);
            //    Object = Object.Trim('"');
            //    usersLogs newObject = JsonConvert.DeserializeObject<usersLogs>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
            //    if (newObject.posId == 0 || newObject.posId == null)
            //    {
            //        Nullable<int> id = null;
            //        newObject.posId = id;
            //    }
            //    if (newObject.userId == 0 || newObject.userId == null)
            //    {
            //        Nullable<int> id = null;
            //        newObject.userId = id;
            //    }



            //    try
            //    {
            //        using (incposdbEntities entity = new incposdbEntities())
            //        {
            //            var locationEntity = entity.Set<usersLogs>();
            //            if (newObject.logId == 0 || newObject.logId == null)
            //            {
            //                // signIn

            //                newObject.sInDate = DateTime.Now;


            //                locationEntity.Add(newObject);
            //                entity.SaveChanges();
            //                message = newObject.logId.ToString();
            //                //sign out old user

            //                using (incposdbEntities entity2 = new incposdbEntities())
            //                {
            //                    List<usersLogs> ul = new List<usersLogs>();
            //                    List<usersLogs>  locationE = entity2.usersLogs.ToList();
            //                    ul = locationE.Where(s => s.sOutDate == null &&
            //                   ( (DateTime.Now-(DateTime)s.sInDate).TotalHours>=24)).ToList();
            //                    if (ul != null)
            //                    {
            //                        foreach(usersLogs row in ul)
            //                        {
            //                            row.sOutDate = DateTime.Now;
            //                            entity2.SaveChanges();
            //                        }
            //                    }
            //                }

            //                }



            //            else
            //            {//signOut
            //                var tmpObject = entity.usersLogs.Where(p => p.logId == newObject.logId).FirstOrDefault();



            //                tmpObject.logId = newObject.logId;
            //               //  tmpObject.sInDate=newObject.sInDate;
            //                 tmpObject.sOutDate= DateTime.Now;
            //             //    tmpObject.posId=newObject.posId;
            //              //  tmpObject.userId = newObject.userId;


            //                entity.SaveChanges();

            //                message = tmpObject.logId.ToString();
            //            }
            //          //  entity.SaveChanges();
            //        }
            //    }
            //    catch
            //    {
            //        message = "-1";
            //    }
            //}
            //return message;
        }
        public bool checkLogByID(int logId)
        {
            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var item = entity.usersLogs.Where(u => u.logId == logId).FirstOrDefault();
                    //check if user change server date
                    if (item.sInDate > DateTime.Now)
                        return true;
                    if (item.sOutDate != null)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
      

        //////////////////////
        ///

        public string Save(usersLogs newObject)
        {
            string message = "";


            if (newObject != null)
            {


                usersLogs tmpObject = null;


                try
                {
                    if (newObject.posId == 0 || newObject.posId == null)
                    {
                        Nullable<int> id = null;
                        newObject.posId = id;
                    }
                    if (newObject.userId == 0 || newObject.userId == null)
                    {
                        Nullable<int> id = null;
                        newObject.userId = id;
                    }
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var locationEntity = entity.Set<usersLogs>();
                        if (newObject.logId == 0 || newObject.logId == null)
                        {
                            // signIn
                            // sign out old
                            using (incposdbEntities entity2 = new incposdbEntities())
                            {
                                List<usersLogs> ul = new List<usersLogs>();
                                List<usersLogs> locationE = entity2.usersLogs.ToList();
                                ul = locationE.Where(s => s.sOutDate == null &&
                               ((DateTime.Now - (DateTime)s.sInDate).TotalHours >= 8) || (s.userId == newObject.userId && s.sOutDate == null)).ToList();
                                if (ul != null)
                                {
                                    foreach (usersLogs row in ul)
                                    {
                                        row.sOutDate = DateTime.Now;
                                        entity2.SaveChanges();
                                    }
                                }
                            }
                            newObject.sInDate = DateTime.Now;


                            locationEntity.Add(newObject);
                            entity.SaveChanges();
                            message = newObject.logId.ToString();
                            //sign out old user



                        }



                        else
                        {//signOut
                            tmpObject = entity.usersLogs.Where(p => p.logId == newObject.logId).FirstOrDefault();



                            tmpObject.logId = newObject.logId;
                            //  tmpObject.sInDate=newObject.sInDate;
                            tmpObject.sOutDate = DateTime.Now;
                            //    tmpObject.posId=newObject.posId;
                            //  tmpObject.userId = newObject.userId;


                            entity.SaveChanges();

                            message = tmpObject.logId.ToString();
                        }
                        //  entity.SaveChanges();
                    }

                    return message;

                }
                catch
                {
                    message = "0";
                    return message;
                }


            }
            else
            {
                return "0";
            }
        }

        public usersLogs GetByID(int logId)
        {
            usersLogs item = new usersLogs();

            if (logId > 0)
            {
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemlist = entity.usersLogs.Where(u => u.logId == logId).ToList();

                        item = itemlist.Where(u => u.logId == logId)
                               .Select(S => new usersLogs
                               {
                                   logId = S.logId,
                                   sInDate = S.sInDate,
                                   sOutDate = S.sOutDate,
                                   posId = S.posId,
                                   userId = S.userId,
                               })
                               .FirstOrDefault();
                        return item;

                    }

                }
                catch (Exception ex)
                {
                    return item;

                }
            }
            else
            {
                return item;
            }



        }


    }
}