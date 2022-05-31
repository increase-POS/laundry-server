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
    [RoutePrefix("api/Notification")]
    public class NotificationController : ApiController
    {

        // add or update notification 
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
                string obj = "";
                int branchId = 0;
                string objectName = "";
                string prefix = "";
                int userId = 0;
                int posId = 0;
                notification Object = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        obj = c.Value.Replace("\\", string.Empty);
                        obj = obj.Trim('"');
                        Object = JsonConvert.DeserializeObject<notification>(obj, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        //break;
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "objectName")
                    {
                        objectName = c.Value;
                    }
                    else if (c.Type == "prefix")
                    {
                        prefix = c.Value;
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                       

                        Object.ncontent = prefix + ":" + Object.ncontent;
                        Object.isActive = 1;
                        Object.createDate = DateTime.Now;
                        Object.updateDate = DateTime.Now;

                        var notEntity = entity.Set<notification>();
                        notification not = notEntity.Add(Object);
                        entity.SaveChanges();

                        #region pos notifications
                        if (posId != 0)
                        {
                            var notUserEntity = entity.Set<notificationUser>();
                            notificationUser notUser = new notificationUser()
                            {
                                notId = not.notId,
                                posId = posId,
                                isRead = false,
                                createDate = DateTime.Now,
                                updateDate = DateTime.Now,
                                createUserId = Object.createUserId,
                                updateUserId = Object.createUserId,
                            };
                            notUserEntity.Add(notUser);
                        }
                        #endregion
                        else if (userId == 0)
                        {
                            var users = (from u in entity.users.Where(x => x.isActive == 1)
                                         join b in entity.branchesUsers on u.userId equals b.userId
                                         where b.branchId == branchId
                                         select new UserModel()
                                         { userId = u.userId }
                                     ).ToList();

                            foreach (UserModel user in users)
                            {
                                var groupObjects = (from GO in entity.groupObject
                                                    join U in entity.users on GO.groupId equals U.groupId
                                                    join G in entity.groups on GO.groupId equals G.groupId
                                                    join O in entity.objects on GO.objectId equals O.objectId
                                                    join POO in entity.objects on O.parentObjectId equals POO.objectId into JP

                                                    from PO in JP.DefaultIfEmpty()
                                                    where U.userId == user.userId
                                                    select new
                                                    {
                                                        //group object
                                                        GO.id,
                                                        GO.groupId,
                                                        GO.objectId,
                                                        GO.addOb,
                                                        GO.updateOb,
                                                        GO.deleteOb,
                                                        GO.showOb,
                                                        GO.reportOb,
                                                        GO.levelOb,
                                                        //group 
                                                        GroupName = G.name,
                                                        //object
                                                        ObjectName = O.name,
                                                        O.parentObjectId,
                                                        O.objectType,
                                                        parentObjectName = PO.name,

                                                    }).ToList();

                                var element = groupObjects.Where(X => X.ObjectName == objectName).FirstOrDefault();
                                if (element.showOb == 1)
                                {
                                    // add notification to users
                                    var notUserEntity = entity.Set<notificationUser>();
                                    notificationUser notUser = new notificationUser()
                                    {
                                        notId = not.notId,
                                        userId = user.userId,
                                        isRead = false,
                                        createDate = DateTime.Now,
                                        updateDate = DateTime.Now,
                                        createUserId = Object.createUserId,
                                        updateUserId = Object.createUserId,
                                    };
                                    notUserEntity.Add(notUser);
                                }
                            }
                        }
                        else // add notification to one user whose id = userId
                        {
                            var notUserEntity = entity.Set<notificationUser>();
                            notificationUser notUser = new notificationUser()
                            {
                                notId = not.notId,
                                userId = userId,
                                isRead = false,
                                createDate = DateTime.Now,
                                updateDate = DateTime.Now,
                                createUserId = Object.createUserId,
                                updateUserId = Object.createUserId,
                            };
                            notUserEntity.Add(notUser);
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

        public void addNotifications(string objectName, string notificationObj, int branchId, string itemName)
        {
            notificationObj = notificationObj.Replace("\\", string.Empty);
            notificationObj = notificationObj.Trim('"');
            notification Object = JsonConvert.DeserializeObject<notification>(notificationObj, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
            //try
            //{
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var users = (from u in entity.users.Where(x => x.isActive == 1 && x.isAdmin == false)
                                 join b in entity.branchesUsers on u.userId equals b.userId
                                 where b.branchId == branchId
                                 select new UserModel()
                                 { userId = u.userId }
                              ).ToList();
                   
                    Object.ncontent = itemName + ":" + Object.ncontent;
                    Object.isActive = 1;
                    Object.createDate = DateTime.Now;
                    Object.updateDate = DateTime.Now;

                    var notEntity = entity.Set<notification>();
                    notification not = notEntity.Add(Object);
                   
                    entity.SaveChanges();
                    notificationUser notUser;
                    var notUserEntity = entity.Set<notificationUser>();
                    foreach (UserModel user in users)
                    {
                        var groupObjects = (from GO in entity.groupObject
                                            join U in entity.users on GO.groupId equals U.groupId
                                            join G in entity.groups on GO.groupId equals G.groupId
                                            join O in entity.objects on GO.objectId equals O.objectId
                                            join POO in entity.objects on O.parentObjectId equals POO.objectId into JP

                                            from PO in JP.DefaultIfEmpty()
                                            where U.userId == user.userId
                                            select new
                                            {
                                                //group object
                                                GO.id,
                                                GO.groupId,
                                                GO.objectId,
                                                GO.addOb,
                                                GO.updateOb,
                                                GO.deleteOb,
                                                GO.showOb,
                                                GO.reportOb,
                                                GO.levelOb,
                                                //group 
                                                GroupName = G.name,
                                                //object
                                                ObjectName = O.name,
                                                O.parentObjectId,
                                                O.objectType,
                                                parentObjectName = PO.name,

                                            }).ToList();
                     
                        var element = groupObjects.Where(X => X.ObjectName == objectName).FirstOrDefault();
                    if(element != null)
                        if (element.showOb == 1)
                        {
                            // add notification to users
                            notUser = new notificationUser()
                            {
                                notId = not.notId,
                                userId = user.userId,
                                isRead = false,                                
                                createDate = DateTime.Now,
                                updateDate = DateTime.Now,
                                createUserId = Object.createUserId,
                                updateUserId = Object.createUserId,
                            };
                            notUserEntity.Add(notUser);
                        }
                    }
                var admins = (from u in entity.users.Where(x => x.isActive == 1 && x.isAdmin == true)
                              select new UserModel()
                              { userId = u.userId }
                              ).ToList();
                foreach (UserModel user in admins)
                {
                    notUser = new notificationUser()
                    {
                        notId = not.notId,
                        userId = user.userId,
                        isRead = false,
                        createDate = DateTime.Now,
                        updateDate = DateTime.Now,
                        createUserId = Object.createUserId,
                        updateUserId = Object.createUserId,
                    };
                    notUserEntity.Add(notUser);
                }                  
                    entity.SaveChanges();
                }           
        }
       
    }
}