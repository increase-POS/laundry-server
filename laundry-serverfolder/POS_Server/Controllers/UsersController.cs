using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS_Server.Classes;
using POS_Server.Models;
using POS_Server.Models.VM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/Users")]
    public class UsersController : ApiController
    {
        //get active users
        [HttpPost]
        [Route("GetActive")]
        public string GetActive(string token)
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
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var usersList = entity.users.Where(u => u.isActive == 1 && u.userId != 1)
                    .Select(u => new UserModel
                    {
                        userId = u.userId,
                        username = u.username,
                        password = u.password,
                        name = u.name,
                        lastname = u.lastname,
                        fullName = u.name + " " + u.lastname,
                        job = u.job,
                        workHours = u.workHours,
                        createDate = u.createDate,
                        updateDate = u.updateDate,
                        createUserId = u.createUserId,
                        updateUserId = u.updateUserId,
                        phone = u.phone,
                        mobile = u.mobile,
                        email = u.email,
                        notes = u.notes,
                        address = u.address,
                        isActive = u.isActive,
                        isOnline = u.isOnline,
                        image = u.image,
                        balance = u.balance,
                        balanceType = u.balanceType,
                        isAdmin = u.isAdmin,
                        groupId = u.groupId,
                        driverIsAvailable=u.driverIsAvailable,
                    })
                    .ToList();

                    return TokenManager.GenerateToken(usersList);
                }
            }
        }

        [HttpPost]
        [Route("GetActiveForAccount")]
        public string GetActiveForAccount(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string payType = "";
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
                    if (c.Type == "payType")
                    {
                        payType = c.Value;
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var usersList = entity.users.Where(u => u.userId != 1 && (u.isActive == 1 ||
                                                 (u.isActive == 0 && payType == "p" && u.balanceType == 0) ||
                                                 (u.isActive == 0 && payType == "d" && u.balanceType == 1)))
                    .Select(u => new UserModel
                    {
                        userId = u.userId,
                        username = u.username,
                        password = u.password,
                        name = u.name,
                        lastname = u.lastname,
                        fullName = u.name + " " + u.lastname,
                        job = u.job,
                        workHours = u.workHours,
                        createDate = u.createDate,
                        updateDate = u.updateDate,
                        createUserId = u.createUserId,
                        updateUserId = u.updateUserId,
                        phone = u.phone,
                        mobile = u.mobile,
                        email = u.email,
                        notes = u.notes,
                        address = u.address,
                        isActive = u.isActive,
                        isOnline = u.isOnline,
                        image = u.image,
                        balance = u.balance,
                        balanceType = u.balanceType,
                        isAdmin = u.isAdmin,
                        groupId = u.groupId,
                        driverIsAvailable = u.driverIsAvailable,
                    })
                    .ToList();

                    return TokenManager.GenerateToken(usersList);
                }
            }
        }

        [HttpPost]
        [Route("Getloginuser")]
        public string Getloginuser(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            List<UserModel> usersList = new List<UserModel>();
            UserModel user = new UserModel();
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string userName = "";
                string password = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "userName")
                    {
                        userName = c.Value;
                    }
                    else if (c.Type == "password")
                    {
                        password = c.Value;
                    }
                }

                UserModel emptyuser = new UserModel();

                emptyuser.createDate = DateTime.Now;
                emptyuser.updateDate = DateTime.Now;
                //emptyuser.username = userName;
                emptyuser.createUserId = 0;
                emptyuser.updateUserId = 0;
                emptyuser.userId = 0;
                emptyuser.isActive = 0;
                emptyuser.isOnline = 0;
                emptyuser.canDelete = false;
                emptyuser.balance = 0;
                emptyuser.balanceType = 0;
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        usersList = entity.users.Where(u => u.isActive == 1 && u.username == userName)
                        .Select(u => new UserModel
                        {
                            userId = u.userId,
                            username = u.username,
                            password = u.password,
                            name = u.name,
                            lastname = u.lastname,
                            fullName = u.name + " " + u.lastname,
                            job = u.job,
                            workHours = u.workHours,
                            createDate = u.createDate,
                            updateDate = u.updateDate,
                            createUserId = u.createUserId,
                            updateUserId = u.updateUserId,
                            phone = u.phone,
                            mobile = u.mobile,
                            email = u.email,
                            notes = u.notes,
                            address = u.address,
                            isActive = u.isActive,
                            isOnline = u.isOnline,
                            image = u.image,
                            balance = u.balance,
                            balanceType = u.balanceType,
                            isAdmin = u.isAdmin,
                        groupId = u.groupId,
                            driverIsAvailable = u.driverIsAvailable,
                        })
                        .ToList();

                        if (usersList == null || usersList.Count <= 0)
                        {
                            user = emptyuser;
                            // rong user
                            return TokenManager.GenerateToken(user);
                        }
                        else
                        {
                            user = usersList.Where(i => i.username == userName).FirstOrDefault();
                            if (user.password.Equals(password))
                            {
                                // correct username and pasword
                                return TokenManager.GenerateToken(user);
                            }
                            else
                            {
                                // rong pass return just username
                                user = emptyuser;
                                user.username = userName;
                                return TokenManager.GenerateToken(user);

                            }
                        }
                    }

                }
                catch
                {
                    return TokenManager.GenerateToken(emptyuser);
                }
            }
        }


        // return all users active and inactive
        [HttpPost]
        [Route("Get")]
        public string Get(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            Boolean canDelete = false;
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var usersList = entity.users
                    .Select(u => new UserModel()
                    {
                        userId = u.userId,
                        username = u.username,
                        password = u.password,
                        name = u.name,
                        lastname = u.lastname,
                        job = u.job,
                        workHours = u.workHours,
                        createDate = u.createDate,
                        updateDate = u.updateDate,
                        createUserId = u.createUserId,
                        updateUserId = u.updateUserId,
                        phone = u.phone,
                        mobile = u.mobile,
                        email = u.email,
                        notes = u.notes,
                        address = u.address,
                        isActive = u.isActive,
                        isOnline = u.isOnline,
                        image = u.image,
                        balance = u.balance,
                        balanceType = u.balanceType,
                        isAdmin = u.isAdmin,
                        groupId = u.groupId,
                        driverIsAvailable = u.driverIsAvailable,

                    })
                    .ToList();

                    if (usersList.Count > 0)
                    {
                        for (int i = 0; i < usersList.Count; i++)
                        {
                            canDelete = false;
                            if (usersList[i].isActive == 1)
                            {
                                int userId = (int)usersList[i].userId;
                                var usersPos = entity.posUsers.Where(x => x.userId == userId).Select(b => new { b.posUserId }).FirstOrDefault();
                                if (usersPos is null)
                                    canDelete = true;
                            }

                            usersList[i].canDelete = canDelete;
                        }
                    }
                    return TokenManager.GenerateToken(usersList.Where(u => u.userId != 1));
                }
            }
        }
        // GET api/<controller>
        [HttpPost]
        [Route("GetUserByID")]
        public string GetUserByID(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int userId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var user = entity.users
                   .Where(u => u.userId == userId)
                   .Select(u => new
                   {
                       u.userId,
                       u.username,
                       u.password,
                       u.name,
                       u.lastname,
                       u.job,
                       u.workHours,
                       u.createDate,
                       u.updateDate,
                       u.createUserId,
                       u.updateUserId,
                       u.phone,
                       u.mobile,
                       u.email,
                       u.notes,
                       u.address,
                       u.isOnline,
                       u.image,
                       u.isActive,
                       u.balance,
                       u.balanceType,
                       u.isAdmin,
                       u.driverIsAvailable,
                       u.groupId,
                   })
                   .FirstOrDefault();
                    return TokenManager.GenerateToken(user);
                }
            }
        }
        /*
        [HttpPost]
        [Route("Search")]
        public IHttpActionResult Search(string searchWords)
        {
            var re = Request;
            var headers = re.Headers;
            string token = "";
            if (headers.Contains("APIKey"))
            {
                token = headers.GetValues("APIKey").First();
            }
            Validation validation = new Validation();
            bool valid = validation.CheckApiKey(token);

            if (valid) // APIKey is valid
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var usersList = entity.users
                    .Where(p => p.name.Contains(searchWords) || p.lastname.Contains(searchWords) || p.job.Contains(searchWords) || p.workHours.Contains(searchWords))
                    .Select(u => new
                    {
                        u.userId,
                        u.username,
                        u.password,
                        u.name,
                        u.lastname,
                        u.job,
                        u.workHours,
                        u.createDate,
                        u.updateDate,
                        u.createUserId,
                        u.updateUserId,
                        u.phone,
                        u.mobile,
                        u.email,
                        u.notes,
                        u.address,
                        u.isActive,
                        u.isOnline,
                        u.image,
                        u.balance,
                        u.balanceType,
                    })
                    .ToList();

                    if (usersList == null)
                        return NotFound();
                    else
                        return Ok(usersList);
                }
            }
            //else
            return NotFound();
        }
        */

        [HttpPost]
        [Route("GetSalesMan")]
        public string GetSalesMan(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int branchId = 0;
                string deliveryPermission = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "deliveryPermission")
                    {
                        deliveryPermission = c.Value;
                    }
                }
                List<UserModel> users = new List<UserModel>();
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var usersList = (from u in entity.users.Where(us => us.isActive == 1 && us.userId != 1)
                                     join bu in entity.branchesUsers on u.userId equals bu.userId
                                     where bu.branchId == branchId
                                     select new UserModel
                                     {
                                         userId = u.userId,
                                         username = u.username,
                                         name = u.name,
                                         lastname = u.lastname,
                                         fullName = u.name + " " + u.lastname,
                                         balance = u.balance,
                                         balanceType = u.balanceType,
                                         isAdmin = u.isAdmin,
                                         groupId = u.groupId,
                                     }).ToList();

                    foreach (UserModel user in usersList)
                    {
                        var groupObjects = (from GO in entity.groupObject
                                            where GO.showOb == 1 && GO.objects.name.Contains(deliveryPermission)
                                            join U in entity.users on GO.groupId equals U.groupId
                                            where U.userId == user.userId
                                            select new
                                            {
                                                //group object
                                                GO.id,
                                                GO.showOb,
                                            }).FirstOrDefault();

                        if (groupObjects != null)
                            users.Add(user);
                    }
                    return TokenManager.GenerateToken(users);
                }
            }
        }
        [HttpPost]
        [Route("getUsersForDelivery")]
        public string getUsersForDelivery(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string job = "";
            int customerId = 0;

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
                    if (c.Type == "job")
                    {
                        job = c.Value;
                    }
                    else if (c.Type == "customerId")
                    {
                        customerId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var residentSecId = entity.agents.Where(x => x.agentId == customerId).Select(x => x.residentSecId).SingleOrDefault();
                    
                    List<UserModel> usersList = new List<UserModel>();

                    if (residentSecId != null)
                    {
                        usersList = (from u in entity.users.Where(u => u.userId != 1 && u.isActive == 1 && u.driverIsAvailable == 1)
                                     join su in entity.residentialSectorsUsers.Where(x => x.residentSecId == residentSecId) on u.userId equals su.userId
                                     select new UserModel
                                     {
                                         userId = u.userId,
                                         username = u.username,
                                         password = u.password,
                                         name = u.name,
                                         lastname = u.lastname,
                                         fullName = u.name + " " + u.lastname,
                                         job = u.job,
                                         workHours = u.workHours,
                                         createDate = u.createDate,
                                         updateDate = u.updateDate,
                                         createUserId = u.createUserId,
                                         updateUserId = u.updateUserId,
                                         phone = u.phone,
                                         mobile = u.mobile,
                                         email = u.email,
                                         isActive = u.isActive,
                                         isOnline = u.isOnline,
                                         balance = u.balance,
                                         balanceType = u.balanceType,
                                         isAdmin = u.isAdmin,
                                         driverIsAvailable = u.driverIsAvailable,
                                         groupId = u.groupId,
                                     })
                        .Distinct().ToList();
                    }
               
                    if (usersList.Count == 0)
                    {
                        usersList = entity.users.Where(u => u.userId != 1 && u.isActive == 1 && u.job == job && u.driverIsAvailable == 1)
                        .Select(u => new UserModel
                        {
                            userId = u.userId,
                            username = u.username,
                            password = u.password,
                            name = u.name,
                            lastname = u.lastname,
                            fullName = u.name + " " + u.lastname,
                            job = u.job,
                            workHours = u.workHours,
                            createDate = u.createDate,
                            updateDate = u.updateDate,
                            createUserId = u.createUserId,
                            updateUserId = u.updateUserId,
                            phone = u.phone,
                            mobile = u.mobile,
                            email = u.email,
                            notes = u.notes,
                            address = u.address,
                            isActive = u.isActive,
                            isOnline = u.isOnline,
                            image = u.image,
                            balance = u.balance,
                            balanceType = u.balanceType,
                            isAdmin = u.isAdmin,
                            driverIsAvailable = u.driverIsAvailable,
                            groupId = u.groupId,
                        })
                        .ToList();
                    }
                    return TokenManager.GenerateToken(usersList);
                }
            }
        }

        // add or update unit
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
                string userObject = "";
                users userObj = null;
                users newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        userObject = c.Value.Replace("\\", string.Empty);
                        userObject = userObject.Trim('"');
                        newObject = JsonConvert.DeserializeObject<users>(userObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                        break;
                    }
                }
                if (newObject.updateUserId == 0 || newObject.updateUserId == null)
                {
                    Nullable<int> id = null;
                    newObject.updateUserId = id;
                }
                if (newObject.createUserId == 0 || newObject.createUserId == null)
                {
                    Nullable<int> id = null;
                    newObject.createUserId = id;
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var userEntity = entity.Set<users>();
                     //   var catEntity = entity.Set<categoryuser>();
                        if (newObject.userId == 0)
                        {
                            newObject.isAdmin = false;

                            ProgramInfo programInfo = new ProgramInfo();
                            int userMaxCount = programInfo.getUserCount();
                            int usersCount = entity.users.Count();
                            if (usersCount >= userMaxCount && userMaxCount != -1)
                            {
                                message = "-1";
                                return TokenManager.GenerateToken(message);
                            }
                            else
                            {
                                newObject.createDate = DateTime.Now;
                                newObject.updateDate = DateTime.Now;
                                newObject.updateUserId = newObject.createUserId;
                                newObject.balance = 0;
                                newObject.balanceType = 0;
                                userObj = userEntity.Add(newObject);
                                // get all categories
                                //var categories = entity.categories.Where(x => x.isActive == 1).Select(x => x.categoryId).ToList();
                                //int sequence = 0;
                                //for (int i = 0; i < categories.Count; i++)
                                //{
                                //    sequence++;
                                //    int categoryId = categories[i];
                                //    categoryuser cu = new categoryuser()
                                //    {
                                //        categoryId = categoryId,
                                //        userId = userObj.userId,
                                //        sequence = sequence,
                                //        createDate = DateTime.Now,
                                //        updateDate = DateTime.Now,
                                //        createUserId = newObject.createUserId,
                                //        updateUserId = newObject.updateUserId,
                                //    };
                                //    catEntity.Add(cu);
                                //}
                                entity.SaveChanges().ToString();
                                message = userObj.userId.ToString();
                                return TokenManager.GenerateToken(message);

                            }
                        }
                        else
                        {
                            userObj = entity.users.Where(p => p.userId == newObject.userId).FirstOrDefault();
                            userObj.name = newObject.name;
                            userObj.username = newObject.username;
                            userObj.password = newObject.password;
                            userObj.name = newObject.name;
                            userObj.lastname = newObject.lastname;
                            userObj.job = newObject.job;
                            userObj.workHours = newObject.workHours;
                            userObj.updateDate = DateTime.Now;
                            userObj.updateUserId = newObject.updateUserId;
                            userObj.phone = newObject.phone;
                            userObj.mobile = newObject.mobile;
                            userObj.email = newObject.email;
                            userObj.notes = newObject.notes;
                            userObj.address = newObject.address;
                            userObj.isActive = newObject.isActive;
                            userObj.balance = newObject.balance;
                            userObj.balanceType = newObject.balanceType;
                            userObj.isOnline = newObject.isOnline;
                            userObj.driverIsAvailable = newObject.driverIsAvailable;
                         userObj.groupId = newObject.groupId;
                            entity.SaveChanges().ToString();
                            message = userObj.userId.ToString();
                            return TokenManager.GenerateToken(message);

                        }
                    }
                }
                catch (Exception ex)
                {
                    //message = "0";
                    //return TokenManager.GenerateToken(message);
                     return TokenManager.GenerateToken(ex.ToString());
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
                int delUserId = 0;
                int userId = 0;
                Boolean final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "delUserId")
                    {
                        delUserId = int.Parse(c.Value);
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
                if (final)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                           // entity.categoryuser.RemoveRange(entity.categoryuser.Where(x => x.userId == delUserId));

                            users usersDelete = entity.users.Find(delUserId);
                            entity.users.Remove(usersDelete);
                            message = entity.SaveChanges().ToString();
                            return TokenManager.GenerateToken(message);

                        }
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
                            users userDelete = entity.users.Find(delUserId);

                            userDelete.isActive = 0;
                            userDelete.updateDate = DateTime.Now;
                            userDelete.updateUserId = userId;
                            message = entity.SaveChanges().ToString();
                            return TokenManager.GenerateToken(message);

                        }
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
                            //  check if image exist
                            var pathCheck = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\user"), imageWithNoExt);
                            var files = Directory.GetFiles(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\user"), imageWithNoExt + ".*");
                            if (files.Length > 0)
                            {
                                File.Delete(files[0]);
                            }

                            //Userimage myfolder name where i want to save my image
                            var filePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\user"), imageName);
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

            localFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\user"), imageName);

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
            var re = Request;
            var headers = re.Headers;
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string userObject = "";
                users userObj = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        userObject = c.Value.Replace("\\", string.Empty);
                        userObject = userObject.Trim('"');
                        userObj = JsonConvert.DeserializeObject<users>(userObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    users user;
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var userEntity = entity.Set<users>();
                        user = entity.users.Where(p => p.userId == userObj.userId).First();
                        user.image = userObj.image;
                        entity.SaveChanges();
                    }
                    message = user.userId.ToString();
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
        [Route("CanLogIn")]
        public string CanLogIn(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int posId = 0;
                int userId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }
                List<UserModel> users = new List<UserModel>();
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var usersList = (from bu in entity.branchesUsers
                                         join B in entity.branches on bu.branchId equals B.branchId
                                         join P in entity.pos on B.branchId equals P.branchId
                                         // from u in entity.users.Where(us => us.isActive == 1 || us.userId == 1)

                                         where P.posId == posId && bu.userId == userId
                                         select new
                                         {
                                             bu.branchsUsersId,
                                             bu.branchId,
                                             bu.userId,
                                         }).ToList();
                        int can = 0;
                        if(usersList==null|| usersList.Count == 0)
                        {
                            can = 0;
                        }
                        else
                        {
                            can = 1;
                        }

                        return TokenManager.GenerateToken(can.ToString());
                    }
                }

                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
        [HttpPost]
        [Route("checkLoginAvalability")]
        public string checkLoginAvalability(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string deviceCode = "";
                int posId = 0;
                string userName = "";
                string password = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "deviceCode")
                    {
                        deviceCode = c.Value;
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userName")
                    {
                        userName = c.Value;
                    }
                    else if (c.Type == "password")
                    {
                        password = c.Value;
                    }
                }
                int res = checkLoginAvalability(posId,deviceCode,userName,password);
            return TokenManager.GenerateToken(res.ToString());

        }
    }
        public int checkLoginAvalability(int posId, string deviceCode,string userName, string password)
        {
            // 1 :  can login-
            //  0 : error 
            // -1 : package is expired 
            // -2 : device code is not correct 
            // -3 : serial is not active 
            // -4 : customer server code is wrong
            // -5 : login date is before last login date

            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {                    
                    //check support user
                    if (userName == "Support@Increase")
                    {
                        var suppUser = entity.users.Where(u => u.isActive == 1 && u.username == userName && u.password == password && u.isAdmin == true).FirstOrDefault();
                        if (suppUser != null)
                            return 1;
                    }
                    //compair login date with last login date for this user
                    var user = entity.users.Where(x => x.username == userName && x.password == password && x.isActive == 1).FirstOrDefault();
                    if(user != null)
                    {
                        var logs = entity.usersLogs.Where(x => x.userId == user.userId).OrderByDescending(x => x.sInDate).FirstOrDefault();
                        if (logs != null && logs.sInDate > DateTime.Now)
                            return -5;
                    }
                    ActivateController ac = new ActivateController();
                    int active = ac.CheckPeriod();
                    if (active == 0)
                        return -1;
                    else
                    {
                        var tmpObject = entity.posSetting.Where(x => x.posId == posId).FirstOrDefault();
                        if (tmpObject != null)
                        {      
                            // check customer code
                            if (tmpObject.posDeviceCode != deviceCode)
                            {
                                return -2;
                            }
                            //check customer server code
                            ProgramDetailsController pc = new ProgramDetailsController();
                            var programD = pc.getCustomerServerCode();
                            if (programD == null || programD.customerServerCode != ac.ServerID())
                            {
                                return -4;
                            }
                        }
                        // check serial && package avalilability
                        var serial = entity.posSetting.Where(x => x.posId == posId && x.posSerials.isActive == true).FirstOrDefault();
                        var programDetails = entity.ProgramDetails.Where(x => x.isActive == true).FirstOrDefault();
                        if (serial == null || programDetails == null)
                            return -3;
                }

                    return 1;
                }
            }
            catch
            {

                return 0;

            }

        }



    }
}

