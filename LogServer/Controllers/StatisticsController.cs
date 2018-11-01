using LogServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LogServer.Controllers
{
    [RoutePrefix("api")]
    public class StatisticsController : ApiController
    {
        // GET: api/Statistics
        [HttpGet]
        [Route("Statistics")]
        public HttpResponseMessage Get()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, StatisticsHandler.GetInstance().GetStatistics());
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        // GET: api/Statistics/5
        [HttpGet]
        [Route("Statistics/{nickname}")]
        public HttpResponseMessage Get(string nickname)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, StatisticsHandler.GetInstance().GetPlayerStatistics(nickname));
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
    }
}
