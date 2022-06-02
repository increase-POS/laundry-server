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
    [RoutePrefix("api/BranchStore")]
    public class BranchStoreController : ApiController
    {

        // GET api/<controller> get all Objects
        [HttpPost]
        [Route("Get")]
        public string Get(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            bool canDelete = false;
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var List = (from S in entity.branchStore
                                 join B in entity.branches on S.branchId equals B.branchId into JBB 
                                 join BB in entity.branches on S.storeId equals BB.branchId into JSB
                                from JBBR in JBB.DefaultIfEmpty()
                                from JSBB in JSB.DefaultIfEmpty()
                                select new BranchStoreModel {
                      id = S.id,
                       branchId = S.branchId,
                       storeId = S.storeId,
                       notes = S.notes,
                    
                       createDate = S.createDate,
                       updateDate = S.updateDate,
                       createUserId = S.createUserId,
                       updateUserId = S.updateUserId,
                       isActive=S.isActive,
                       canDelete = true,
                                    // branch
                                    bbranchId = JSBB.branchId,
                                    bcode = JBBR.code,
                                    bname = JBBR.name,
                                    baddress = JBBR.address,
                                    bemail = JBBR.email,
                                    bphone = JBBR.phone,
                                    bmobile = JBBR.mobile,
                                    bcreateDate = JBBR.createDate,
                                    bupdateDate = JBBR.updateDate,
                                    bcreateUserId = JBBR.createUserId,
                                    bupdateUserId = JBBR.updateUserId,
                                    bnotes = JBBR.notes,
                                    bparentId = JBBR.parentId,
                                    bisActive = JBBR.isActive,
                                    btype = JBBR.type,
                                    //store
                                    sbranchId = JSBB.branchId,
                                    scode = JSBB.code,
                                    sname = JSBB.name,
                                    saddress = JSBB.address,
                                    semail = JSBB.email,
                                    sphone = JSBB.phone,
                                    smobile = JSBB.mobile,
                                    screateDate = JSBB.createDate,
                                    supdateDate = JSBB.updateDate,
                                    screateUserId = JSBB.createUserId,
                                    supdateUserId = JSBB.updateUserId,
                                    snotes = JSBB.notes,
                                    sparentId = JSBB.parentId,
                                    sisActive = JSBB.isActive,
                                    stype = JSBB.type,

                                }).ToList();
                    return TokenManager.GenerateToken(List);

                }
            }
        }
       
        //
        [HttpPost]
        [Route("UpdateStoresById")]
        public string UpdateStoresById(string token)
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

                string branchStoreObject = "";
                List<branchStore> newListObj = null;
                int branchId = 0;
                int userId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "newList")
                    {
                        branchStoreObject = c.Value.Replace("\\", string.Empty);
                        branchStoreObject = branchStoreObject.Trim('"');
                        newListObj = JsonConvert.DeserializeObject<List<branchStore>>(branchStoreObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        //break;
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else   if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }
                if (newListObj != null)
                {

               
                // delete old invoice items
                using (incposdbEntities entity = new incposdbEntities())
                {
                    List<branchStore> items = entity.branchStore.Where(x => x.branchId == branchId).ToList();
                    entity.branchStore.RemoveRange(items);
                    try { entity.SaveChanges(); }
                    catch { }

                }

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
                        if (newListObj[i].branchId == 0 || newListObj[i].branchId == null)
                        {
                            Nullable<int> id = null;
                            newListObj[i].branchId = id;
                        }
                        if (newListObj[i].storeId == 0 || newListObj[i].storeId == null)
                        {
                            Nullable<int> id = null;
                            newListObj[i].storeId = id;
                        }
                        var branchEntity = entity.Set<branchStore>();

                        newListObj[i].createDate = DateTime.Now;
                        newListObj[i].updateDate = DateTime.Now;
                        newListObj[i].updateUserId = newListObj[i].createUserId;
                        newListObj[i].branchId = branchId;
                        branchEntity.Add(newListObj[i]);

                    }
                    try
                    {
                      message=  entity.SaveChanges().ToString();
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

           // message = "1";
            return TokenManager.GenerateToken(message);
        }

      
    }
}