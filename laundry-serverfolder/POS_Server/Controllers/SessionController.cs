using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/Session")]
    public class SessionController : ApiController
    {
        [HttpPost]
        [Route("SetSession")]
        public IHttpActionResult SetSession(int userId)
        {
            HttpContext.Current.Session["userId"] = userId;
            return Ok();
        }
        [HttpPost]
        [Route("GetSession")]
        public IHttpActionResult GetSession(string key)
        {  
            return Ok(HttpContext.Current.Session[key]);
        }
    }
}