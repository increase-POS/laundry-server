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
    [RoutePrefix("api/membershipsOffers")]
    public class membershipsOffersController : ApiController
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
                    var List1 = entity.membershipsOffers.ToList();
                    var List = List1.Select(S => new membershipsOffers
                    {
                        membershipOfferId = S.membershipOfferId,
                        membershipId = S.membershipId,
                        offerId = S.offerId,
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
                int membershipOfferId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        membershipOfferId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var bank = entity.membershipsOffers
                   .Where(S => S.membershipOfferId == membershipOfferId)
                   .Select(S => new
                   {
                       S.membershipOfferId,
                       S.membershipId,
                       S.offerId,
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
        [Route("UpdateOffersByMembershipId")]
        public string UpdateOffersByMembershipId(string token)
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
                List<membershipsOffers> newListObj = null;
                int membershipId = 0;
                int updateUserId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "newList")
                    {
                        strObject = c.Value.Replace("\\", string.Empty);
                        strObject = strObject.Trim('"');
                        newListObj = JsonConvert.DeserializeObject<List<membershipsOffers>>(strObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

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

                List<membershipsOffers> items = null;
                // delete old invoice items
                using (incposdbEntities entity = new incposdbEntities())
                {
                    items = entity.membershipsOffers.Where(x => x.membershipId == membershipId).ToList();
                    if (items != null)
                    {
                        entity.membershipsOffers.RemoveRange(items);
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
                            if (newListObj[i].offerId == 0 || newListObj[i].offerId == null)
                            {
                                Nullable<int> id = null;
                                newListObj[i].offerId = id;
                            }
                            var branchEntity = entity.Set<membershipsOffers>();

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