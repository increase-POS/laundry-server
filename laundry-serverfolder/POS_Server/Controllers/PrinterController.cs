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
    [RoutePrefix("api/PrinterController")]
    public class PrinterController : ApiController
    {
        // GET api/<controller>
        [HttpPost]
        [Route("GetAll")]
      public string   GetAll(string token)
        {
            // public ResponseVM GetPurinv(string token)

           
            
            
          token = TokenManager.readToken(HttpContext.Current.Request); 
 var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
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

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {


                    using (incposdbEntities entity = new incposdbEntities())
                    {


                        var list = (from S in entity.printers
                                    select new
                                    {

                                        S.printerId,
                                        S.name,
                                        S.printFor,
                                        S.createDate,
                                        S.updateDate,
                                        S.createUserId,
                                        S.updateUserId,



                                    }).ToList();

                        return TokenManager.GenerateToken(list);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }



//           
//            
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
//                    var List = (from S in entity.printers
//                                select new
//                                {

//                                    S.printerId,
//                                    S.name,
//                                    S.printFor,
//                                    S.createDate,
//                                    S.updateDate,
//                                    S.createUserId,
//                                    S.updateUserId,



//                                }).ToList();
//                    /*
//public int printerId { get; set; }
//        public string name { get; set; }
//        public string printFor { get; set; }
//        public Nullable<System.DateTime> createDate { get; set; }
//        public Nullable<System.DateTime> updateDate { get; set; }
//        public Nullable<int> createUserId { get; set; }
//        public Nullable<int> updateUserId { get; set; }


//                    */



//                    if (List == null)
//                        return NotFound();
//                    else
//                        return Ok(List);
//                }
//            }
//            //else
//            return NotFound();
        }

        // GET api/<controller>
        [HttpPost]
        [Route("GetByID")]
      public string   GetByID(string token)
        {
            // public ResponseVM GetPurinv(string token)int printerId
           
            
            
          token = TokenManager.readToken(HttpContext.Current.Request); 
 var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int printerId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "printerId")
                    {
                        printerId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {

                    var item = entity.printers
                   .Where(u => u.printerId == printerId)
                   .Select(S => new
                   {
                       S.printerId,
                       S.name,
                       S.printFor,
                       S.createDate,
                       S.updateDate,
                       S.createUserId,
                       S.updateUserId,


                   })
                           .FirstOrDefault();
                    return TokenManager.GenerateToken(item);
                }
            }



            //var re = Request;
            //
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
            //        var row = entity.printers
            //       .Where(u => u.printerId == printerId)
            //       .Select(S => new
            //       {
            //           S.printerId,
            //           S.name,
            //           S.printFor,
            //           S.createDate,
            //           S.updateDate,
            //           S.createUserId,
            //           S.updateUserId,


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
      public string   Save(string token)
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
                printers newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<printers>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {


                    printers tmpObject;
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

                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var locationEntity = entity.Set<printers>();
                            if (newObject.printerId == 0)
                            {
                                newObject.createDate = DateTime.Now;
                                newObject.updateDate = DateTime.Now;
                                newObject.updateUserId = newObject.createUserId;


                                locationEntity.Add(newObject);
                                entity.SaveChanges();
                                message = newObject.printerId.ToString();
                            }
                            else
                            {
                                tmpObject = entity.printers.Where(p => p.printerId == newObject.printerId).FirstOrDefault();

                                tmpObject.updateDate = DateTime.Now;
                                tmpObject.updateUserId = newObject.updateUserId;

                                tmpObject.name = newObject.name;
                                //  tmpObject.printerId = newObject.printerId;


                                tmpObject.printFor = newObject.printFor;




                                entity.SaveChanges();

                                message = tmpObject.printerId.ToString();
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
            //
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
            //    printers newObject = JsonConvert.DeserializeObject<printers>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
            //    if (newObject.updateUserId == 0 || newObject.updateUserId == null)
            //    {
            //        Nullable<int> id = null;
            //        newObject.updateUserId = id;
            //    }
            //    if (newObject.createUserId == 0 || newObject.createUserId == null)
            //    {
            //        Nullable<int> id = null;
            //        newObject.createUserId = id;
            //    }

            //    try
            //    {
            //        using (incposdbEntities entity = new incposdbEntities())
            //        {
            //            var locationEntity = entity.Set<printers>();
            //            if (newObject.printerId == 0)
            //            {
            //                newObject.createDate = DateTime.Now;
            //                newObject.updateDate = DateTime.Now;
            //                newObject.updateUserId = newObject.createUserId;


            //                locationEntity.Add(newObject);
            //                entity.SaveChanges();
            //                message = newObject.printerId.ToString();
            //            }
            //            else
            //            {
            //                var tmpObject = entity.printers.Where(p => p.printerId == newObject.printerId).FirstOrDefault();

            //                tmpObject.updateDate = DateTime.Now;
            //                tmpObject.updateUserId = newObject.updateUserId;

            //                tmpObject.name = newObject.name;
            //              //  tmpObject.printerId = newObject.printerId;


            //                       tmpObject.printFor = newObject.printFor;




            //                entity.SaveChanges();

            //                message = tmpObject.printerId.ToString();
            //            }
            //            //  entity.SaveChanges();
            //        }
            //    }
            //    catch
            //    {
            //        message = "-1";
            //    }
            //}
            //return message;
        }

        [HttpPost]
        [Route("Delete")]
            public string   Delete(string token)
        {//int printerId
            string message = "";
           
            
            
          token = TokenManager.readToken(HttpContext.Current.Request); 
 var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int printerId = 0;


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "printerId")
                    {
                        printerId = int.Parse(c.Value);
                    }

                }

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        printers objectDelete = entity.printers.Find(printerId);

                        entity.printers.Remove(objectDelete);
                        message = entity.SaveChanges().ToString();

                   
                    }
                    return TokenManager.GenerateToken(message);
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }


            }


            //var re = Request;
            //
            //string token = "";
            //int message = 0;
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}

            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);
            //if (valid)
            //{

            //        try
            //        {
            //            using (incposdbEntities entity = new incposdbEntities())
            //            {
            //                printers objectDelete = entity.printers.Find(printerId);

            //                entity.printers.Remove(objectDelete);
            //                message = entity.SaveChanges();

            //                return message.ToString();
            //            }
            //        }
            //        catch
            //        {
            //            return "-1";
            //        }


            //}
            //else
            //    return "-3";
        }



    }
}