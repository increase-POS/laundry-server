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
    [RoutePrefix("api/setting")]
    public class SettingController : ApiController
    {
        // GET api/<controller> get all setting
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


                        var list = entity.setting

                           .Select(c => new
                           {
                               c.settingId,
                               c.name,
                               c.notes,

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
            //                var settingList = entity.setting

            //               .Select(c => new  {
            //                 c.settingId ,
            //                 c.name,
            //                 c.notes, 

            //})
            //               .ToList();



            //                if (settingList == null)
            //                    return NotFound();
            //                else
            //                    return Ok(settingList);
            //            }
            //        }
            //        //else
            //            return NotFound();
        }
      

        // GET api/<controller> get all setting
        [HttpPost]
        [Route("GetByNotes")]
        public string   GetByNotes(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request); 
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string notes ="";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "notes")
                    {
                        notes = c.Value;
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {

                    List<setting> settingList1 = entity.setting.ToList();
                    var list = settingList1.Where(c => c.notes == notes).Select(c => new setting
                    {
                        settingId = c.settingId,
                        name = c.name,
                        notes = c.notes,
                    }).ToList();

                    return TokenManager.GenerateToken(list);
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

            //if (valid) // APIKey is valid
            //{
            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        List<setting> settingList1 = entity.setting.ToList();
            //    var    settingList = settingList1.Where(c => c.notes == notes).Select(c => new setting
            //        {
            //            settingId = c.settingId,
            //            name = c.name,
            //            notes = c.notes,
            //        }).ToList();


            //        if (settingList == null)
            //            return NotFound();
            //        else
            //            return Ok(settingList);
            //    }
            //}
            ////else
            //return NotFound();
        }

    }
}