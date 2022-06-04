﻿using Newtonsoft.Json;
using POS_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity.Migrations;
using POS_Server.Models.VM;
using System.Security.Claims;
using System.Web;
using Newtonsoft.Json.Converters;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/userSetValues")]
    public class userSetValuesController : ApiController
    {
        // GET api/<controller> get all userSetValues
        [HttpPost]
        [Route("Get")]
       public string Get(string token)
        {
            //public stringGetPurinv(string token)

           
           
            
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

                        var list = entity.userSetValues

                   .Select(c => new
                   {
                       c.id,
                       c.userId,
                       c.valId,
                       c.notes,
                       c.createDate,
                       c.updateDate,
                       c.createUserId,
                       c.updateUserId,

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

            //if (valid) // APIKey is valid
            //{
            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        var List = entity.userSetValues

            //       .Select(c => new  {
            //           c.id,
            //           c.userId,
            //           c.valId,
            //           c.note,
            //           c.createDate,
            //           c.updateDate,
            //           c.createUserId,
            //           c.updateUserId,

            //       })
            //       .ToList();



            //        if (List == null)
            //            return NotFound();
            //        else
            //            return Ok(List);
            //    }
            //}
            ////else
            //    return NotFound();
        }



       


        [HttpPost]
        [Route("Saveu")]
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
                userSetValues newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<userSetValues>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {


                    userSetValues tmpObject = null;


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
                            var locationEntity = entity.Set<userSetValues>();
                            if (newObject.id == 0)
                            {
                                newObject.createDate = DateTime.Now;
                                newObject.updateDate = DateTime.Now;
                                newObject.updateUserId = newObject.createUserId;


                                locationEntity.Add(newObject);
                                entity.SaveChanges();
                                message = newObject.id.ToString();
                            }
                            else
                            {
                              tmpObject = entity.userSetValues.Where(p => p.id == newObject.id).FirstOrDefault();

                                tmpObject.updateDate = DateTime.Now;
                                tmpObject.updateUserId = newObject.updateUserId;

                                tmpObject.valId = newObject.valId;
                                tmpObject.userId = newObject.userId;
                                tmpObject.notes = newObject.notes;

                                entity.SaveChanges();

                                message = tmpObject.id.ToString();
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
            //    // return Object.ToString();
            //    Object = Object.Replace("\\", string.Empty);
            //    Object = Object.Trim('"');
            //    userSetValues newObject = JsonConvert.DeserializeObject<userSetValues>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
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
            //            var locationEntity = entity.Set<userSetValues>();
            //            if (newObject.id == 0)
            //            {
            //                newObject.createDate = DateTime.Now;
            //                newObject.updateDate = DateTime.Now;
            //                newObject.updateUserId = newObject.createUserId;


            //                locationEntity.Add(newObject);
            //                entity.SaveChanges();
            //                message = newObject.id.ToString();
            //            }
            //            else
            //            {
            //                var tmpObject = entity.userSetValues.Where(p => p.id == newObject.id).FirstOrDefault();

            //                tmpObject.updateDate = DateTime.Now;
            //                tmpObject.updateUserId = newObject.updateUserId;

            //                tmpObject.valId = newObject.valId;
            //                tmpObject.userId = newObject.userId;
            //                tmpObject.note = newObject.note;

            //                entity.SaveChanges();

            //                message = tmpObject.id.ToString();
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


    }
}