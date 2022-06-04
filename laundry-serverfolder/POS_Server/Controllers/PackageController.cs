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
    [RoutePrefix("api/Package")]
    public class PackageController : ApiController
    {
       
        public List<int> canNotUpdatePack()
        {
            
            List<int> listg = new List<int>();

            using (incposdbEntities entity = new incposdbEntities())
            {

                var list = (from iu in entity.itemsUnits
                            join it in entity.items on iu.itemId equals it.itemId
                            join iuloc in entity.itemsLocations on iu.itemUnitId equals iuloc.itemUnitId

                            where it.type == "p"
                            select new
                            {
                                //piuId = iu.itemUnitId,
                                //itemsLocId= iuloc.itemsLocId,
                                it.itemId,
                                iuloc.quantity,
                                //unitId=   iu.unitId,
                                //type=it.type,
                            }).ToList();

              listg = list.GroupBy(g => g.itemId).Where(q=>q.Sum(s => s.quantity)>0).Select(x =>  
                    x.First().itemId ).ToList();

            }
            return listg;
        }

        // GET api/<controller>
        [HttpPost]
        [Route("GetChildsByParentId")]
        public string GetChildsByParentId(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request); 
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int parentIUId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "parentIUId")
                    {
                        parentIUId = int.Parse(c.Value);
                    }


                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var list = entity.packages
                       .Where(u => u.parentIUId == parentIUId)
                       .Select(S => new
                       {
                           S.packageId,
                           S.parentIUId,
                           S.childIUId,
                           S.quantity,
                           S.isActive,
                           S.notes,
                           S.createUserId,
                           S.updateUserId,
                           S.createDate,
                           S.updateDate,


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

                //if (valid)
                //{
                //    using (incposdbEntities entity = new incposdbEntities())
                //    {
                //        var list = entity.packages
                //       .Where(u => u.parentIUId == parentIUId)
                //       .Select(S => new
                //       {
                //           S.packageId,
                //           S.parentIUId,
                //           S.childIUId,
                //           S.quantity,
                //           S.isActive,
                //           S.notes,
                //           S.createUserId,
                //           S.updateUserId,
                //           S.createDate,
                //           S.updateDate,


                //       })
                //       .ToList();

                //        if (list == null)
                //            return NotFound();
                //        else
                //            return Ok(list);
                //    }
                //}
                //else
                //    return NotFound();
            }

        #region
        [HttpPost]
        [Route("UpdatePackByParentId")]
        public string UpdatePackByParentId(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request); 
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                List<packages> newitofObj = new List<packages>();
                string newlist = "";
           
                int userId = 0;
                int parentIUId = 0;


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        newlist = c.Value.Replace("\\", string.Empty);
                        newlist = newlist.Trim('"');
                        newitofObj = JsonConvert.DeserializeObject<List<packages>>(newlist, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "parentIUId")
                    {
                        parentIUId = int.Parse(c.Value);
                    }


                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {
                    int res = 0;
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var iuoffer = entity.packages.Where(p => p.parentIUId == parentIUId);
                        if (iuoffer.Count() > 0)
                        {
                            entity.packages.RemoveRange(iuoffer);
                        }
                        if (newitofObj.Count() > 0)
                        {
                            foreach (packages newitofrow in newitofObj)
                            {
                                newitofrow.parentIUId = parentIUId;

                                if (newitofrow.createUserId == null || newitofrow.createUserId == 0)
                                {
                                    newitofrow.createDate = DateTime.Now;
                                    newitofrow.updateDate = DateTime.Now;

                                    newitofrow.createUserId = userId;
                                    newitofrow.updateUserId = userId;
                                }
                                else
                                {
                                    newitofrow.updateDate = DateTime.Now;
                                    newitofrow.updateUserId = userId;

                                }

                            }
                            entity.packages.AddRange(newitofObj);
                        }
                        res = entity.SaveChanges();

                        // return res;
                        return TokenManager.GenerateToken(res.ToString());
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }


            }

            //int userId = 0;
            //int parentIUId = 0;
            //var re = Request;
            //var headers = re.Headers;
            //int res = 0;
            //string token = "";
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //if (headers.Contains("parentIUId"))
            //{
            //    parentIUId = Convert.ToInt32(headers.GetValues("parentIUId").First());
            //}
            //if (headers.Contains("userId"))
            //{
            //    userId = Convert.ToInt32(headers.GetValues("userId").First());
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);
            //newplist = newplist.Replace("\\", string.Empty);
            //newplist = newplist.Trim('"');
            //List<packages> newitofObj = JsonConvert.DeserializeObject<List<packages>>(newplist, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
            //if (valid)
            //{
            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        var iuoffer = entity.packages.Where(p => p.parentIUId == parentIUId);
            //        if (iuoffer.Count() > 0)
            //        {
            //            entity.packages.RemoveRange(iuoffer);
            //        }
            //        if (newitofObj.Count() > 0)
            //        {
            //            foreach (packages newitofrow in newitofObj)
            //            {
            //                newitofrow.parentIUId = parentIUId;

            //                if (newitofrow.createUserId == null || newitofrow.createUserId == 0)
            //                {
            //                    newitofrow.createDate = DateTime.Now;
            //                    newitofrow.updateDate = DateTime.Now;

            //                    newitofrow.createUserId = userId;
            //                    newitofrow.updateUserId = userId;
            //                }
            //                else
            //                {
            //                    newitofrow.updateDate = DateTime.Now;
            //                    newitofrow.updateUserId = userId;

            //                }

            //            }
            //            entity.packages.AddRange(newitofObj);
            //        }
            //        res = entity.SaveChanges();

            //        return res;

            //    }

            //}
            //else
            //{
            //    return -1;
            //}

        }
        #endregion
        public List<PackageModel> GetChildsByParentId(int parentIUId)
        {
            List<PackageModel> list = new List<PackageModel>();

            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    list = (from S in entity.packages
                            join IU in entity.itemsUnits on S.childIUId equals IU.itemUnitId
                            join I in entity.items on IU.itemId equals I.itemId
                            where S.parentIUId == parentIUId
                            select new PackageModel()
                            {

                                packageId = S.packageId,
                                parentIUId = S.parentIUId,
                                childIUId = S.childIUId,
                                quantity = S.quantity,
                                isActive = S.isActive,
                                avgPurchasePrice = I.avgPurchasePrice,
                                citemId = I.itemId,
                                type = I.type,
                            }).ToList();

                    return list;

                }
            }
            catch
            {
                return list;
            }
        }
    }
}