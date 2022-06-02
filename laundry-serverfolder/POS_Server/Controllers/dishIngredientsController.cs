﻿using LinqKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS_Server.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/dishIngredients")]
    public class dishIngredientsController : ApiController
    {
       
        [HttpPost]
        [Route("GetByItemUnitId")]
        public string GetByItemUnitId(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int itemUnitId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        itemUnitId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var List1 = entity.dishIngredients.ToList();
                    var List = List1.Where(S => S.itemUnitId == itemUnitId).Select(S => new dishIngredients
                    {

                        dishIngredId = S.dishIngredId,
                        name = S.name,
                        itemUnitId = S.itemUnitId,
                        notes = S.notes,
                        isActive = S.isActive,
                        createDate = S.createDate,
                        updateDate = S.updateDate,
                        createUserId = S.createUserId,
                        updateUserId = S.updateUserId,



                    }).ToList();


                    return TokenManager.GenerateToken(List);

                }
            }
        }       
        
        // add or update menu settings 
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
                string itemObject = "";
                dishIngredients newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        itemObject = c.Value.Replace("\\", string.Empty);
                        itemObject = itemObject.Trim('"');
                        newObject = JsonConvert.DeserializeObject<dishIngredients>(itemObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
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
                if (newObject.itemUnitId == 0 || newObject.itemUnitId == null)
                {
                    Nullable<int> id = null;
                    newObject.itemUnitId = id;
                }
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        dishIngredients tmpObject = new dishIngredients();
                        if (newObject.dishIngredId == 0)
                        {
                            newObject.createDate = DateTime.Now;
                            newObject.updateDate = DateTime.Now;
                            newObject.updateUserId = newObject.createUserId;
                            entity.dishIngredients.Add(newObject);
                        }
                        else
                        {
                            tmpObject = entity.dishIngredients.Find(newObject.dishIngredId);
                            tmpObject.name = newObject.name;
                            tmpObject.itemUnitId = newObject.itemUnitId;
                            tmpObject.notes = newObject.notes;
                            tmpObject.isActive = newObject.isActive;
                            tmpObject.updateUserId = newObject.createUserId;
                            tmpObject.updateDate = DateTime.Now;
                        }
                        message = entity.SaveChanges().ToString();
                    }
                    return TokenManager.GenerateToken(message);
                }
                catch { return TokenManager.GenerateToken("0"); }
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
                int dishIngredId = 0;
                int userId = 0;
                Boolean final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        dishIngredId = int.Parse(c.Value);
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

                            dishIngredients objDelete = entity.dishIngredients.Find(dishIngredId);
                            entity.dishIngredients.Remove(objDelete);
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

                            dishIngredients objDelete = entity.dishIngredients.Find(dishIngredId);
                            objDelete.isActive = 0;
                            objDelete.updateUserId = userId;
                            objDelete.updateDate = DateTime.Now;
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
    }
}
