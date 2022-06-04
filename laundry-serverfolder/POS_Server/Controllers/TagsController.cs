﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS_Server.Models;
using POS_Server.Models.VM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web;
using System.Data.Entity.Validation;
using LinqKit;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/Tags")]
    public class TagsController : ApiController
    {
        // GET api/<controller> get all tags
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
                int categoryId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "categoryId")
                    {
                        categoryId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var searchPredicate = PredicateBuilder.New<tags>();
                    if (categoryId != 0)
                        searchPredicate.And(x => x.categoryId == categoryId);

                    var tagsList = entity.tags.Where(searchPredicate)
                   .Select(S => new TagsModel()
                   {
                       tagId = S.tagId,
                       tagName = S.tagName,
                       categoryId = S.categoryId,
                       notes = S.notes,
                       createUserId = S.createUserId,
                       updateUserId = S.updateUserId,
                       createDate = S.createDate,
                       updateDate = S.updateDate,
                       isActive = S.isActive,
                   }).ToList();

                    // can delet or not
                    if (tagsList.Count > 0)
                    {
                        foreach (TagsModel item in tagsList)
                        {
                            canDelete = false;
                            if (item.isActive == 1)
                            {
                                int cId = (int)item.tagId;
                                var casht = entity.items.Where(x => x.tagId == cId).Select(x => new { x.tagId }).FirstOrDefault();

                                if ((casht is null))
                                    canDelete = true;
                            }
                            item.canDelete = canDelete;
                        }
                    }
                    return TokenManager.GenerateToken(tagsList);
                }
            }
        }
       
           
        // add or update card 
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
                tags Object = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        itemObject = c.Value.Replace("\\", string.Empty);
                        itemObject = itemObject.Trim('"');
                        Object = JsonConvert.DeserializeObject<tags>(itemObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                        {
                            tags tmpObject = new tags();
                            if (Object.tagId == 0)
                            {
                                Object.createDate = DateTime.Now;
                                Object.updateDate = DateTime.Now;
                                Object.updateUserId = Object.createUserId;
                                entity.tags.Add(Object);
                            }
                            else
                            {
                                tmpObject = entity.tags.Find(Object.tagId);
                                tmpObject.tagName = Object.tagName;
                                tmpObject.categoryId = Object.categoryId;
                                tmpObject.notes = Object.notes;
                                tmpObject.updateUserId = Object.updateUserId;
                                tmpObject.updateDate = DateTime.Now;
                            }
                        message =  entity.SaveChanges().ToString();
                    }
                        return TokenManager.GenerateToken(message);
                }
                catch {return TokenManager.GenerateToken("0");}
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
                int tagId = 0;
                int userId = 0;
                Boolean final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        tagId = int.Parse(c.Value);
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
                            tags cardObj = entity.tags.Find(tagId);
                            entity.tags.Remove(cardObj);
                            message = entity.SaveChanges().ToString();
                            return TokenManager.GenerateToken(message);
                        }
                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }
                }
                else
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            tags cardObj = entity.tags.Find(tagId);

                            cardObj.isActive = 0;
                            cardObj.updateUserId = userId;
                            cardObj.updateDate = DateTime.Now;
                            message = entity.SaveChanges().ToString();
                            return TokenManager.GenerateToken(message);
                        }
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
}