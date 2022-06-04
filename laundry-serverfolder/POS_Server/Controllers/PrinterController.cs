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
        [Route("GetByID")]
        public string   GetByID(string token)
        { 
            
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
        }

        // add or update location
        [HttpPost]
        [Route("Save")]
        public string   Save(string token)
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
        }
        

    }
}