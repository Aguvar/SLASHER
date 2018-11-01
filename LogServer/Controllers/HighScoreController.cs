using LogServer.Models;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LogServer.Controllers
{
    [RoutePrefix("api")]
    public class HighScoreController : ApiController
    {
        // GET: api/HighScore
        [HttpGet]
        [Route("Highscores/")]
        public HttpResponseMessage Get()
        {
            try
            {
                HighScoreHandler highScoreHandler = HighScoreHandler.GetInstance();
                return Request.CreateResponse(HttpStatusCode.OK, highScoreHandler.GetHighScores());
            }
            catch (System.Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
    }
}
