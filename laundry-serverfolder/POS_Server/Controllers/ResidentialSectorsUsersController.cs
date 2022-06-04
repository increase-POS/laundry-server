using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS_Server.Models;
using POS_Server.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/ResidentialSectorsUsers")]
    public class ResidentialSectorsUsersController : ApiController
    {
              
        [HttpPost]
        [Route("UpdateResSectorsByUserId")]
        public string UpdateResSectorsByUserId(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string strObject = "";
                List<residentialSectorsUsers> newListObj = null;
                int userId = 0;
                int updateUserId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "newList")
                    {
                        strObject = c.Value.Replace("\\", string.Empty);
                        strObject = strObject.Trim('"');
                        newListObj = JsonConvert.DeserializeObject<List<residentialSectorsUsers>>(strObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else
                  if (c.Type == "updateUserId")
                    {
                        updateUserId = int.Parse(c.Value);
                    }
                }

                List<residentialSectorsUsers> items = null;
                // delete old invoice items
                using (incposdbEntities entity = new incposdbEntities())
                {
                    items = entity.residentialSectorsUsers.Where(x => x.userId == userId).ToList();
                    if (items != null)
                    {
                        entity.residentialSectorsUsers.RemoveRange(items);
                        try
                        { entity.SaveChanges(); }
                        catch (Exception ex)
                        {
                            message = "-2";
                            return TokenManager.GenerateToken(message);
                        }
                    }


                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        for (int i = 0; i < newListObj.Count; i++)
                        {
                            if (newListObj[i].updateUserId == 0 || newListObj[i].updateUserId == null)
                            {
                                Nullable<int> id = null;
                                newListObj[i].updateUserId = id;
                            }
                            if (newListObj[i].createUserId == 0 || newListObj[i].createUserId == null)
                            {
                                Nullable<int> id = null;
                                newListObj[i].createUserId = id;
                            }
                            if (newListObj[i].residentSecId == 0 || newListObj[i].residentSecId == null)
                            {
                                Nullable<int> id = null;
                                newListObj[i].residentSecId = id;
                            }
                            if (newListObj[i].userId == 0 || newListObj[i].userId == null)
                            {
                                Nullable<int> id = null;
                                newListObj[i].userId = id;
                            }
                            var branchEntity = entity.Set<residentialSectorsUsers>();

                            newListObj[i].createDate = DateTime.Now;
                            newListObj[i].updateDate = newListObj[i].createDate;
                            newListObj[i].updateUserId = updateUserId;
                            newListObj[i].userId = userId;
                            branchEntity.Add(newListObj[i]);
                            entity.SaveChanges();
                        }

                        entity.SaveChanges();


                    }



                    message = "1";
                    return TokenManager.GenerateToken(message);
                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }

            }

        }


     
    }
}