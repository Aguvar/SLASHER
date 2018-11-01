using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LogServer.Models
{
    [RoutePrefix("api")]
    public class LogController : ApiController
    {
        // GET: api/Log
        [HttpGet]
        [Route("Log/")]
        public HttpResponseMessage Get()
        {

            return Request.CreateResponse(HttpStatusCode.OK, "Holi"); ;
        }

        //// GET: api/Log/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/Log
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT: api/Log/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE: api/Log/5
        //public void Delete(int id)
        //{
        //}
    }
}
