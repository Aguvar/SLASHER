using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LogServer.Controllers
{
    [RoutePrefix("api")]
    public class LogController : ApiController
    {
        // GET: api/Log
        [HttpGet]
        [Route("Log/")]
        public HttpResponseMessage Get()
        {

            return Request.CreateResponse(HttpStatusCode.OK, GameLog.GetInstance().GetLog());
        }
    }
}
