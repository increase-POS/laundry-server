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
    [RoutePrefix("api/invoicesClassMemberships")]
    public class invoicesClassMembershipsController : ApiController
    {
        [HttpPost]
        [Route("GetAll")]
        public string GetAll(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var List1 = entity.invoicesClassMemberships.ToList();
                    var List = List1.Select(S => new invoicesClassMemberships
                    {
                        invClassMemberId = S.invClassMemberId,
                        membershipId = S.membershipId,
                        invClassId = S.invClassId,
                        notes = S.notes,
                        createDate = S.createDate,
                        updateDate = S.updateDate,
                        createUserId = S.createUserId,
                        updateUserId = S.updateUserId,



                    })
                    .ToList();

                    return TokenManager.GenerateToken(List);

                }
            }
        }
      
        [HttpPost]
        [Route("GetById")]
        public string GetById(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int invClassMemberId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        invClassMemberId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var bank = entity.invoicesClassMemberships
                   .Where(S => S.invClassMemberId == invClassMemberId)
                   .Select(S => new
                   {
                       S.invClassMemberId,
                       S.membershipId,
                       S.invClassId,
                       S.notes,
                       S.createDate,
                       S.updateDate,
                       S.createUserId,
                       S.updateUserId,


                   })
                   .FirstOrDefault();
                    return TokenManager.GenerateToken(bank);

                }
            }
        }
       
        
        //update branches list by userId
        [HttpPost]
        [Route("UpdateInvclassByMembershipId")]
        public string UpdateInvclassByMembershipId(string token)
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
                List<invoicesClassMemberships> newListObj = null;
                int membershipId = 0;
                int updateUserId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "newList")
                    {
                        strObject = c.Value.Replace("\\", string.Empty);
                        strObject = strObject.Trim('"');
                        newListObj = JsonConvert.DeserializeObject<List<invoicesClassMemberships>>(strObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "membershipId")
                    {
                        membershipId = int.Parse(c.Value);
                    }
                    else
                  if (c.Type == "updateUserId")
                    {
                        updateUserId = int.Parse(c.Value);
                    }
                }

                List<invoicesClassMemberships> items = null;
                // delete old invoice items
                using (incposdbEntities entity = new incposdbEntities())
                {
                    items = entity.invoicesClassMemberships.Where(x => x.membershipId == membershipId).ToList();
                    if (items != null)
                    {
                        entity.invoicesClassMemberships.RemoveRange(items);
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
                            if (newListObj[i].membershipId == 0 || newListObj[i].membershipId == null)
                            {
                                Nullable<int> id = null;
                                newListObj[i].membershipId = id;
                            }
                            if (newListObj[i].invClassId == 0 || newListObj[i].invClassId == null)
                            {
                                Nullable<int> id = null;
                                newListObj[i].invClassId = id;
                            }
                            var branchEntity = entity.Set<invoicesClassMemberships>();

                            newListObj[i].createDate = DateTime.Now;
                            newListObj[i].updateDate = newListObj[i].createDate;
                            newListObj[i].updateUserId = updateUserId;
                            newListObj[i].membershipId = membershipId;
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