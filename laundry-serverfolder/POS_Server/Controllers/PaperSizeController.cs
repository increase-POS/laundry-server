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
    [RoutePrefix("api/PaperSizeController")]
    public class PaperSizeController : ApiController
    {
        // GET api/<controller>
        [HttpPost]
        [Route("GetAll")]
        public string   GetAll(string token)
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

                        var list = (from S in entity.paperSize
                                    select new
                                    {
                                        S.sizeId,
                                        S.paperSize1,
                                        S.printfor,
                                        S.sizeValue,

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
            //        string token = "";


            //        if (headers.Contains("APIKey"))
            //        {
            //            token = headers.GetValues("APIKey").First();
            //        }
            //        Validation validation = new Validation();
            //        bool valid = validation.CheckApiKey(token);

            //        if (valid) // APIKey is valid
            //        {
            //            using (incposdbEntities entity = new incposdbEntities())
            //            {
            //                var List = (from S in entity.paperSize
            //                            select new
            //                            {
            //                                S.sizeId,
            //                                S.paperSize1,
            //                                S.printfor,
            //                                S.sizeValue ,

            //}).ToList();



            //                if (List == null)
            //                    return NotFound();
            //                else
            //                    return Ok(List);
            //            }
            //        }
            //        //else
            //        return NotFound();
        }

            

    }
}