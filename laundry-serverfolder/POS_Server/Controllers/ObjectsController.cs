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
    [RoutePrefix("api/Object")]
    public class ObjectsController : ApiController
    {
        // GET api/<controller> get all Objects
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
                 

                bool canDelete = false;
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var List = entity.objects

                                   .Select(c => new ObjectsModel
                                   {
                                       objectId = c.objectId,
                                       name = c.name,
                                  
                                       parentObjectId = c.parentObjectId,
                                       objectType = c.objectType,
                                       translate = c.translate,
                                       icon= c.icon,
                                       translateHint=  c.translateHint,
                                   })
                                   .ToList();
                        if (List.Count > 0)
                        {
                            for (int i = 0; i < List.Count; i++)
                            {
                                canDelete = false;
                                if (List[i].isActive == 1)
                                {
                                    int objectId = (int)List[i].objectId;
                                    var operationsL = entity.groupObject.Where(x => x.objectId == objectId).Select(b => new { b.id }).FirstOrDefault();

                                    if (operationsL is null)
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

         
        }
           
       
    }
}