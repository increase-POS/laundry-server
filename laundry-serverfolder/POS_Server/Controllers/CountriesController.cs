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

namespace POS_Server.Controllers
{
    [RoutePrefix("api/Countries")]
    public class CountriesController : ApiController
    {
        // GET api/<controller>
        [HttpPost]
        [Route("GetAllCountries")]
        public string   GetAllCountries(string token)
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


                        var list = entity.countriesCodes
                         .Select(c => new
                         {
                             c.countryId,
                             c.code,
                         }).ToList();



                        return TokenManager.GenerateToken(list);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }
            
        }
       

        [HttpPost]
        [Route("GetAllRegion")]
        public string   GetAllRegion(string token)
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

                        var list = entity.countriesCodes
                         .Select(c => new
                         {
                             c.countryId,
                             c.code,
                             c.currency,
                             c.name,
                             c.isDefault,
                             c.currencyId,

                         }).ToList();


                        
                        return TokenManager.GenerateToken(list);
                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }

        }

        [HttpPost]
        [Route("UpdateIsdefault")]
        public string   UpdateIsdefault(string token)
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
                int countryId = 0;
             
               
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "countryId")
                    {
                        countryId = int.Parse(c.Value);
                    }
                   
                }
                
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                       // reset all to 0
                                    List<countriesCodes> objectlist = entity.countriesCodes.Where(x => x.isDefault == 1).ToList();
                        if (objectlist.Count > 0)
                        {
                            for (int i = 0; i < objectlist.Count; i++)
                            {
                                objectlist[i].isDefault = 0;

                            }
                            entity.SaveChanges();
                        }
                        // set is selected to isdefault=1
                        countriesCodes objectrow = entity.countriesCodes.Find(countryId);

                        if (objectrow != null)
                        {
                            objectrow.isDefault = 1;
                           
       
                           int res=  entity.SaveChanges();
                            if (res > 0)
                            {
                                message = objectrow.countryId.ToString();
                            }
                            else
                            {
                                return TokenManager.GenerateToken("0");
                            }
                        }
                        else
                        {
                            return TokenManager.GenerateToken("0");
                        }
                        //  entity.SaveChanges();



                      


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
            //string message = "";
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid)
            //{


            //    try
            //    {
            //        using (incposdbEntities entity = new incposdbEntities())
            //        {
            //            // reset all to 0
            //            List<countriesCodes> objectlist = entity.countriesCodes.Where(x=>x.isDefault==1).ToList();
            //            if (objectlist.Count > 0)
            //            {
            //                for(int i=0;i< objectlist.Count; i++)
            //                {
            //                    objectlist[i].isDefault = 0;

            //                }
            //                entity.SaveChanges();
            //            }
            //            // set is selected to isdefault=1
            //            countriesCodes objectrow = entity.countriesCodes.Find(countryId);

            //            if (objectrow != null)
            //            {
            //                objectrow.isDefault = 1;

            //                message = objectrow.countryId.ToString();
            //                entity.SaveChanges();
            //            }
            //            else
            //            {
            //                message = "-1";
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