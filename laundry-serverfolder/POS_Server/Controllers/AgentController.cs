using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS_Server.Classes;
using POS_Server.Models;
using POS_Server.Models.VM;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/Agent")]
    public class AgentController : ApiController
    {
        // GET api/Agent
        [HttpPost]
        [Route("Get")]
        public string Get(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string type = "";
            Boolean canDelete = false;
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var agentsList = entity.agents
                   .Where(p => p.type == type)
                   .Select(p => new AgentModel
                   {
                       agentId = p.agentId,
                       name = p.name,
                       code = p.code,
                       company = p.company,
                       address = p.address,
                       email = p.email,
                       phone = p.phone,
                       mobile = p.mobile,
                       image = p.image,
                       type = p.type,
                       accType = p.accType,
                       balance = p.balance,
                       balanceType = p.balanceType,
                       notes = p.notes,
                       isActive = p.isActive,
                       createDate = p.createDate,
                       updateDate = p.updateDate,
                       maxDeserve = p.maxDeserve,
                       fax = p.fax,
                       isLimited = p.isLimited,
                       payType = p.payType,
                       canReserve = p.canReserve,
                       disallowReason = p.disallowReason,
                       residentSecId = p.residentSecId,
                       GPSAddress = p.GPSAddress,
                   })
                   .ToList();
                    if (agentsList.Count > 0)
                    {
                        for (int i = 0; i < agentsList.Count; i++)
                        {
                            canDelete = false;
                            if (agentsList[i].isActive == 1)
                            {
                                int agentId = (int)agentsList[i].agentId;
                                var invoicesL = entity.invoices.Where(x => x.agentId == agentId).Select(b => new { b.invoiceId }).FirstOrDefault();
                                var cachTransferL = entity.cashTransfer.Where(x => x.agentId == agentId).Select(x => new { x.cashTransId }).FirstOrDefault();
                                if ((invoicesL is null) && (cachTransferL is null))
                                    canDelete = true;
                            }
                            agentsList[i].canDelete = canDelete;
                        }
                    }
                    return TokenManager.GenerateToken(agentsList);
                }
            }
        }
        [HttpPost]
        [Route("GetActive")]
        public string GetActive(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            string type = "";
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {

                    var agentsList = entity.agents
                   .Where(p => p.type == type && p.isActive == 1)
                   .Select(p => new
                   {
                       p.agentId,
                       p.name,
                       p.code,
                       p.company,
                       p.address,
                       p.email,
                       p.phone,
                       p.mobile,
                       p.image,
                       p.type,
                       p.accType,
                       p.balance,
                       p.balanceType,
                       p.notes,
                       p.maxDeserve,
                       p.fax,
                       p.isActive,
                       p.createDate,
                       p.isLimited,
                       p.payType,
                       p.canReserve,
                       p.disallowReason,
                        p.residentSecId,
                     p.GPSAddress,
                     p.membershipId,
                   })
                   .ToList();

                    return TokenManager.GenerateToken(agentsList);

                }
            }
        }

        [HttpPost]
        [Route("GetActiveForAccount")]
        public string GetActiveForAccount(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            string type = "";
            string payType = "";
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                    if (c.Type == "payType")
                    {
                        payType = c.Value;
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {

                    var agentsList = entity.agents
                   .Where(p => p.type == type && (p.isActive == 1 ||
                                                 (p.isActive == 0 && payType == "p" && p.balanceType == 0) ||
                                                 (p.isActive == 0 && payType == "d" && p.balanceType == 1)))
                   .Select(p => new
                   {
                       p.agentId,
                       p.name,
                       p.code,
                       p.company,
                       p.address,
                       p.email,
                       p.phone,
                       p.mobile,
                       p.image,
                       p.type,
                       p.accType,
                       p.balance,
                       p.balanceType,
                       p.notes,
                       p.maxDeserve,
                       p.fax,
                       p.isActive,
                       p.createDate,
                       p.isLimited,
                       p.payType,
                       p.canReserve,
                       p.disallowReason,
                       p.residentSecId,
                    p.GPSAddress,
                   })
                   .ToList();
                    return TokenManager.GenerateToken(agentsList);
                }
            }
        }

        [HttpPost]
        [Route("GetAgentByID")]
        public string GetAgentByID(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request); var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                {
                    int agentId = 0;
                    IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                    foreach (Claim c in claims)
                    {
                        if (c.Type == "agentId")
                        {
                            agentId = int.Parse(c.Value);
                        }
                    }
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var agent = entity.agents
                       .Where(p => p.agentId == agentId)
                       .Select(p => new { p.agentId, p.name, p.accType, p.address, p.balance, p.balanceType, p.code, p.company, p.createDate, p.createUserId, p.email, p.mobile, p.notes, p.phone, p.type, p.image, p.maxDeserve, p.fax, p.isActive, p.updateDate, p.updateUserId, p.isLimited, p.payType, p.disallowReason, p.canReserve,
                             p.residentSecId,
                            p.GPSAddress,
                       })
                       .FirstOrDefault();
                        return TokenManager.GenerateToken(agent);
                    }
                }
            }
        }

        // add or update agent
        [HttpPost]
        [Route("Save")]
        public string Save(string token)
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
                string agentObject = "";
                agents agentObj = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        agentObject = c.Value.Replace("\\", string.Empty);
                        agentObject = agentObject.Trim('"');
                        agentObj = JsonConvert.DeserializeObject<agents>(agentObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    agents agent;
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var agentEntity = entity.Set<agents>();
                        if (agentObj.agentId == 0)
                        {
                            ProgramInfo programInfo = new ProgramInfo();
                            int agentMaxCount = 0;
                            if (agentObj.type == "c")
                                agentMaxCount = programInfo.getCustomerCount();
                            else if (agentObj.type == "v")
                                agentMaxCount = programInfo.getVendorCount();

                            int agentCount = entity.agents.Where(x => x.type == agentObj.type).Count();
                            if (agentCount >= agentMaxCount)
                            {
                                message = "-1";
                                return TokenManager.GenerateToken(message);
                            }
                            else
                            {
                                agentObj.createDate = DateTime.Now;
                                agentObj.updateDate = DateTime.Now;
                                agentObj.updateUserId = agentObj.createUserId;
                                agentObj.balanceType = 0;
                               agent = agentEntity.Add(agentObj);
                            }
                        }
                        else
                        {
                            agent = entity.agents.Where(p => p.agentId == agentObj.agentId).First();
                            agent.accType = agentObj.accType;
                            agent.address = agentObj.address;
                            agent.code = agentObj.code;
                            agent.company = agentObj.company;
                            agent.email = agentObj.email;
                            agent.image = agentObj.image;
                            agent.mobile = agentObj.mobile;
                            agent.name = agentObj.name;
                            agent.notes = agentObj.notes;
                            agent.phone = agentObj.phone;
                            agent.type = agentObj.type;
                            agent.maxDeserve = agentObj.maxDeserve;
                            agent.fax = agentObj.fax;
                            agent.updateDate = DateTime.Now;// server current date
                            agent.updateUserId = agentObj.updateUserId;
                            agent.isActive = agentObj.isActive;
                            agent.balance = agentObj.balance;
                            agent.balanceType = agentObj.balanceType;
                            agent.isLimited = agentObj.isLimited;
                            agent.payType = agentObj.payType;
                            agent.canReserve = agentObj.canReserve;
                            agent.disallowReason = agentObj.disallowReason;
                            agent.residentSecId = agentObj.residentSecId;
                            agent.GPSAddress = agentObj.GPSAddress;
                        }
                        entity.SaveChanges();
                        message = agent.agentId.ToString();

                    }
                    return TokenManager.GenerateToken(message);
                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }
            }
        }
        [HttpPost]
        [Route("Delete")]
        public string Delete(string token)
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
                int agentId = 0;
                int userId = 0;
                Boolean final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        agentId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "final")
                    {
                        final = bool.Parse(c.Value);
                    }
                }
                if (!final)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var tmpAgent = entity.agents.Where(p => p.agentId == agentId).First();
                            tmpAgent.isActive = 0;
                            tmpAgent.updateDate = DateTime.Now;
                            tmpAgent.updateUserId = userId;

                            message = entity.SaveChanges().ToString();
                        }
                        return TokenManager.GenerateToken(message);
                    }
                    catch
                    {
                        return TokenManager.GenerateToken("0");
                    }
                }
                else
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var tmpAgent = entity.agents.Where(p => p.agentId == agentId).First();
                            entity.agents.Remove(tmpAgent);
                            message = entity.SaveChanges().ToString();
                        }
                        return TokenManager.GenerateToken(message);
                    }
                    catch
                    {
                        return TokenManager.GenerateToken("0");
                    }
                }
            }

        }

        [Route("PostUserImage")]
        public IHttpActionResult PostUserImage()
        {

            try
            {
                var httpRequest = HttpContext.Current.Request;

                foreach (string file in httpRequest.Files)
                {

                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                    var postedFile = httpRequest.Files[file];
                    string imageName = postedFile.FileName;
                    string imageWithNoExt = Path.GetFileNameWithoutExtension(postedFile.FileName);

                    if (postedFile != null && postedFile.ContentLength > 0)
                    {

                        int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB

                        IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png", ".bmp", ".jpeg", ".tiff", ".jfif" };
                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();

                        if (!AllowedFileExtensions.Contains(extension))
                        {

                            var message = string.Format("Please Upload image of type .jpg,.gif,.png, .jfif, .bmp , .jpeg ,.tiff");
                            return Ok(message);
                        }
                        else if (postedFile.ContentLength > MaxContentLength)
                        {

                            var message = string.Format("Please Upload a file upto 1 mb.");

                            return Ok(message);
                        }
                        else
                        {
                            if (!Directory.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\agent")))
                                Directory.CreateDirectory(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\agent"));
                            //  check if image exist
                            var pathCheck = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\agent"), imageWithNoExt);
                            var files = Directory.GetFiles(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\agent"), imageWithNoExt + ".*");
                            if (files.Length > 0)
                            {
                                File.Delete(files[0]);
                            }

                            //Userimage myfolder name where i want to save my image
                            var filePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\agent"), imageName);
                            postedFile.SaveAs(filePath);

                        }
                    }

                    var message1 = string.Format("Image Updated Successfully.");
                    return Ok(message1);
                }
                var res = string.Format("Please Upload a image.");

                return Ok(res);
            }
            catch (Exception ex)
            {
                var res = string.Format("some Message");

                return Ok(res);
            }
        }

        [HttpGet]
        [Route("GetImage")]
        public HttpResponseMessage GetImage(string imageName)
        {
            if (String.IsNullOrEmpty(imageName))
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            string localFilePath;

            localFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\agent"), imageName);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = imageName;

            return response;
        }

        [HttpPost]
        [Route("UpdateImage")]
        public string UpdateImage(string token)
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
                string agentObject = "";
                agents agentObj = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        agentObject = c.Value.Replace("\\", string.Empty);
                        agentObject = agentObject.Trim('"');
                        agentObj = JsonConvert.DeserializeObject<agents>(agentObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    agents agent;
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var agentEntity = entity.Set<agents>();
                        agent = entity.agents.Where(p => p.agentId == agentObj.agentId).First();
                        agent.image = agentObj.image;
                        entity.SaveChanges();
                    }
                    message = agent.agentId.ToString();
                    return TokenManager.GenerateToken(message);
                }

                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }
            }

        }
       

        [HttpPost]
        [Route("GetLastNumOfCode")]
        public string GetLastNumOfCode(string token)
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
                string type = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                }
                List<string> numberList;
                int lastNum = 0;
                using (incposdbEntities entity = new incposdbEntities())
                {
                    numberList = entity.agents.Where(b => b.code.Contains(type + "-")).Select(b => b.code).ToList();

                    for (int i = 0; i < numberList.Count; i++)
                    {
                        string code = numberList[i];
                        string s = code.Substring(code.LastIndexOf("-") + 1);
                        numberList[i] = s;
                    }
                    if (numberList.Count > 0)
                    {
                        numberList.Sort();
                        lastNum = int.Parse(numberList[numberList.Count - 1]);
                    }
                }
                message = lastNum.ToString();
                return TokenManager.GenerateToken(message);
            }
        }

        [HttpPost]
        [Route("GetAgentsByMembershipId")]
        public string GetAgentsByMembershipId(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                int membershipId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        membershipId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var List = (from S in entity.agents
                          
                                where S.membershipId == membershipId
                                select new AgentModel()
                                {
                                    agentId = S.agentId,
                                    name = S.name,
                                    code = S.code,
                                    company = S.company,
                                    address = S.address,
                                    email = S.email,
                                    phone = S.phone,
                                    mobile = S.mobile,
                                    image = S.image,
                                    type = S.type,
                                    accType = S.accType,
                                    balance = S.balance,
                                    balanceType = S.balanceType,
                                    notes = S.notes,
                                    isActive = S.isActive,
                                    createDate = S.createDate,
                                    updateDate = S.updateDate,
                                    maxDeserve = S.maxDeserve,
                                    fax = S.fax,
                                    isLimited = S.isLimited,
                                    payType = S.payType,
                                    canReserve = S.canReserve,
                                    disallowReason = S.disallowReason,
                                    residentSecId = S.residentSecId,
                                    GPSAddress = S.GPSAddress,

                             
                                  
                                    membershipId = S.membershipId,
                                


                                }).ToList();
                    return TokenManager.GenerateToken(List);


                }
            }
        }


        public int UpdateMembershipId(int agentId,int membershipId)
        {
           
            int message =0;
                try
                {
                    agents agent;
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var agentEntity = entity.Set<agents>();
                        agent = entity.agents.Where(p => p.agentId == agentId).First();
                        agent.membershipId = membershipId;
                        entity.SaveChanges();
                    }
                    message = agent.agentId;
                    return  message;
                }

                catch
                {
                    message = 0;
                    return message;
                }
           
        }
        public int resetMembershipId(  int membershipId)
        {

            int message = 0;
            try
            {
                List<agents> agentlist;
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var agentEntity = entity.Set<agents>();
                    agentlist = entity.agents.Where(p => p.membershipId == membershipId).ToList();
                    foreach (agents row in agentlist)
                    {
                        row.membershipId = null;
                    }
                  
                    entity.SaveChanges();
                }
                message =1;
                return message;
            }

            catch
            {
                message = 0;
                return message;
            }

        }


        //[HttpPost]
        //[Route("UpdateAgentsPoints")]
        //public string UpdateAgentsPoints(string token)
        //{
        //    token = TokenManager.readToken(HttpContext.Current.Request);
        //    string message = "";
        //    var strP = TokenManager.GetPrincipal(token);
        //    if (strP != "0") //invalid authorization
        //    {
        //        return TokenManager.GenerateToken(strP);
        //    }
        //    else
        //    {
        //        string strObject = "";
        //        List<agents> newListObj = null;
                
        //        int updateUserId = 0;
        //        //int points = 0;
        //        int posId = 0;
        //        IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
        //        foreach (Claim c in claims)
        //        {
        //            if (c.Type == "newList")
        //            {
        //                strObject = c.Value.Replace("\\", string.Empty);
        //                strObject = strObject.Trim('"');
        //                newListObj = JsonConvert.DeserializeObject<List<agents>>(strObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

        //            }
                
        //            else if (c.Type == "updateUserId")
        //            {
        //                updateUserId = int.Parse(c.Value);
        //            }
        //            else if (c.Type == "posId")
        //            {
        //                posId = int.Parse(c.Value);
        //            }

        //        }


        //        try
        //        {

        //            int res = 0;
        //            foreach (var row in newListObj)
        //            {
        //                res= UpdateAgentPoints(row.agentId,row.points,updateUserId, posId,"-");
        //                if(res==0)
        //                {
        //                    return TokenManager.GenerateToken("0");
        //                    //return TokenManager.GenerateToken("noup");
        //                }
        //            }

        //            message = "1";
        //            return TokenManager.GenerateToken(message);
        //        }
        //        catch (Exception ex)
        //        {
        //            message = "0";
        //            return TokenManager.GenerateToken(message);
        //            //return TokenManager.GenerateToken(ex.ToString());
        //        }

        //    }

        //}

// تعديل النقاط لزبون محدد بالقيمة الموجودة في ال agent
        [HttpPost]
        [Route("UpdateAgentPoints")]
        public string UpdateAgentPoints(string token)
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
               agents newObj = null;

               // int updateUserId = 0;
                //int points = 0;
                int posId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        strObject = c.Value.Replace("\\", string.Empty);
                        strObject = strObject.Trim('"');
                        newObj = JsonConvert.DeserializeObject<agents>(strObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }

                    //else if (c.Type == "updateUserId")
                    //{
                    //    updateUserId = int.Parse(c.Value);
                    //}
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }

                }


                try
                {

                    int res = 0;
                    //foreach (var row in newListObj)
                    //{
                        res = UpdateAgentPoints(newObj.agentId, newObj.points, (int)newObj.updateUserId, posId, "-");
                        if (res == 0)
                        {
                        return TokenManager.GenerateToken("0");
                        //return TokenManager.GenerateToken("noup");
                    }
                    //}

                    message = "1";
                    return TokenManager.GenerateToken(message);
                }
                catch (Exception ex)
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                    //return TokenManager.GenerateToken(ex.ToString());
                }

            }

        }


        // تعديل النقاط لكل الزبائن بزيادة او انقاص الرصيد بمقدار القيمة المرسلة
  [HttpPost]
        [Route("UpdateAllAgentsPoints")]
        public string UpdateAllAgentsPoints(string token)
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
                //List<agents> newListObj = null;

                int updateUserId = 0;
                int points = 0;
                int posId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    //if (c.Type == "newList")
                    //{
                    //    strObject = c.Value.Replace("\\", string.Empty);
                    //    strObject = strObject.Trim('"');
                    //    newListObj = JsonConvert.DeserializeObject<List<agents>>(strObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    //}

                    if (c.Type == "updateUserId")
                    {
                        updateUserId = int.Parse(c.Value);
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }
                    else if (c.Type == "points")
                    {
                        points = int.Parse(c.Value);
                    }

                }


                try
                {

                    int res = 0;
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                         
                        var newListObj = entity.agents.Where(p => p.type =="c").ToList() ;

                        foreach (var row in newListObj)
                        {
                            res = UpdatePoint(row.agentId, points, updateUserId, posId, "-");
                            if (res == 0)
                            {
                                return TokenManager.GenerateToken("0");
                                //return TokenManager.GenerateToken("noup");
                            }
                        }
                    }
                    message = "1";
                    return TokenManager.GenerateToken(message);
                }
                catch (Exception ex)
                {
                    message = "0";
                    //  return TokenManager.GenerateToken(message);
                    return TokenManager.GenerateToken(ex.ToString());
                }

            }

        }

   
        // تصفير نقاط كل الزبائن
        [HttpPost]
        [Route("resetAllAgentsPoints")]
        public string resetAllAgentsPoints(string token)
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
                //List<agents> newListObj = null;

                int updateUserId = 0;
                int points = 0;
                int posId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    //if (c.Type == "newList")
                    //{
                    //    strObject = c.Value.Replace("\\", string.Empty);
                    //    strObject = strObject.Trim('"');
                    //    newListObj = JsonConvert.DeserializeObject<List<agents>>(strObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    //}

                    if (c.Type == "updateUserId")
                    {
                        updateUserId = int.Parse(c.Value);
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }
                    //else if (c.Type == "points")
                    //{
                    //    points = int.Parse(c.Value);
                    //}

                }


                try
                {

                    int res = 0;
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var newListObj = entity.agents.Where(p => p.type == "c").ToList();

                        foreach (var row in newListObj)
                        {
                            res = UpdatePoint(row.agentId, row.points*(-1), updateUserId, posId, "-");
                            if (res == 0)
                            {
                                return TokenManager.GenerateToken("0");
                                //return TokenManager.GenerateToken("noup");
                            }
                        }
                    }
                    message = "1";
                    return TokenManager.GenerateToken(message);
                }
                catch (Exception ex)
                {
                    message = "0";
                    //  return TokenManager.GenerateToken(message);
                    return TokenManager.GenerateToken(ex.ToString());
                }

            }

        }


        //تصفير نقاط والنقاط الكلية لكل الزبائن
        [HttpPost]
        [Route("resetAllPointsAndHistory")]
        public string resetAllPointsAndHistory(string token)
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
                //List<agents> newListObj = null;

                int updateUserId = 0;
                int points = 0;
                int posId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    //if (c.Type == "newList")
                    //{
                    //    strObject = c.Value.Replace("\\", string.Empty);
                    //    strObject = strObject.Trim('"');
                    //    newListObj = JsonConvert.DeserializeObject<List<agents>>(strObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    //}

                    if (c.Type == "updateUserId")
                    {
                        updateUserId = int.Parse(c.Value);
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }
                    //else if (c.Type == "points")
                    //{
                    //    points = int.Parse(c.Value);
                    //}

                }


                try
                {

                    int res = 0;
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var newListObj = entity.agents.Where(p => p.type == "c").ToList();

                        foreach (var row in newListObj)
                        {
                            res = ResetPointAndHistory(row.agentId, row.points * (-1), updateUserId, posId, "-");
                            if (res == 0)
                            {
                                return TokenManager.GenerateToken("0");
                                //return TokenManager.GenerateToken("noup");
                            }
                        }
                    }
                    message = "1";
                    return TokenManager.GenerateToken(message);
                }
                catch (Exception ex)
                {
                    message = "0";
                    //  return TokenManager.GenerateToken(message);
                    return TokenManager.GenerateToken(ex.ToString());
                }

            }

        }


        public int UpdatePoint(int agentId, int pointsValue, int updateUserId, int? posId, string transNum)
        {
            CashTransferController ctctrlr = new CashTransferController();

            int message = 0;
            try
            {
                int pointdif = 0;
                //List<agents> agentlist;
                CashTransferController cashctrlr = new CashTransferController();
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var agentEntity = entity.Set<agents>();
                    agents row = entity.agents.Where(p => p.agentId == agentId).ToList().First();

                    //foreach (agents row in agentlist)
                    //{
                    //if (row.points != points)
                    //{

                    int finalpoint = pointsValue + row.points;
                    if (finalpoint < 0)
                    {
                        finalpoint = 0;
                        // decrease value is all db points
                        pointdif = -1 * row.points;
                    }
                    else
                    {
                        pointdif = pointsValue;
                    }

                    //add cash transfer for point
                    row.points = finalpoint;
                    if (pointsValue > 0)
                    {
                        row.pointsHistory = row.pointsHistory + pointsValue;
                    }
                    row.updateUserId = updateUserId;

                    if (pointdif != 0)
                    {
                        cashTransfer newcashtr = new cashTransfer();
                        newcashtr.agentId = agentId;
                        newcashtr.createUserId = updateUserId;
                        newcashtr.updateUserId = updateUserId;
                        newcashtr.points = pointdif;
                        newcashtr.posId = posId;
                        newcashtr.processType = "point";
                        newcashtr.side = "c";
                        string num = ctctrlr.generateTransNumber("pnt");

                        newcashtr.transNum = num;
                        newcashtr.transType = "point";
                        int resId = cashctrlr.Save(newcashtr);
                        if (resId == 0)
                        {
                            message = 0;
                            return message;
                        }
                    }
                   

                    entity.SaveChanges();
                    

                }
                message = 1;
                return message;
            }

            catch
            {
                message = 0;
                return message;
            }

        }

        public int ResetPointAndHistory(int agentId, int pointsValue, int updateUserId, int? posId, string transNum)
        {
            CashTransferController ctctrlr = new CashTransferController();
            int message = 0;
            try
            {
                int pointdif = 0;
                //List<agents> agentlist;
                CashTransferController cashctrlr = new CashTransferController();
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var agentEntity = entity.Set<agents>();
                    agents row = entity.agents.Where(p => p.agentId == agentId).ToList().First();

                  

                    int finalpoint = pointsValue + row.points;
                    if (finalpoint < 0)
                    {
                        finalpoint = 0;
                        // decrease value is all db points
                        pointdif = -1 * row.points;
                    }
                    else
                    {
                        pointdif = pointsValue;
                    }

                    
                    row.points = finalpoint;

                    row.pointsHistory = 0;

                    row.updateUserId = updateUserId;
                    if (pointdif != 0)
                    {

                 
                        //add cash transfer for point
                        cashTransfer newcashtr = new cashTransfer();
                    newcashtr.agentId = agentId;
                    newcashtr.createUserId = updateUserId;
                    newcashtr.updateUserId = updateUserId;
                    newcashtr.points = pointdif;
                    newcashtr.posId = posId;
                    newcashtr.processType = "point";
                    newcashtr.side = "c";
                    string num = ctctrlr.generateTransNumber("pnt");
                    newcashtr.transNum = num;
                    newcashtr.transType = "point";
                    int resId = cashctrlr.Save(newcashtr);
                    if (resId == 0)
                    {
                        message = 0;
                        return message;
                    }
}
                 

                    entity.SaveChanges();
                       
                    
                    

                }
                message = 1;
                return message;
            }

            catch
            {
                message = 0;
                return message;
            }

        }


        public int UpdateAgentPoints(int agentId, int points, int updateUserId, int? posId, string transNum)
        {
            CashTransferController ctctrlr = new CashTransferController();
            int message = 0;
            try
            {
              
                CashTransferController cashctrlr = new CashTransferController();
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var agentEntity = entity.Set<agents>();
                    agents row = entity.agents.Where(p => p.agentId == agentId).ToList().First();

              
                
                    if (row.points != points)
                    {

                        int pointdif = points - row.points;

                
                        row.points = points;
                        if (pointdif > 0)
                        {
                            row.pointsHistory = row.pointsHistory + pointdif;
                        }
                        row.updateUserId = updateUserId;
                        //add cash transfer for point
                        if (pointdif != 0)
                        {
                            cashTransfer newcashtr = new cashTransfer();
                            newcashtr.agentId = agentId;
                            newcashtr.createUserId = updateUserId;
                            newcashtr.updateUserId = updateUserId;
                            newcashtr.points = pointdif;
                            newcashtr.posId = posId;
                            newcashtr.processType = "point";
                            newcashtr.side = "c";
                            string num = ctctrlr.generateTransNumber("pnt");
                            newcashtr.transNum = num;
                            newcashtr.transType = "point";
                            int resId = cashctrlr.Save(newcashtr);
                            if (resId == 0)
                            {
                                message = 0;
                                return message;
                            }

                        }

                        entity.SaveChanges();
                    }

                }
                message = 1;
                return message;
            }

            catch
            {
                message = 0;
                return message;
            }

        }

    }
}
