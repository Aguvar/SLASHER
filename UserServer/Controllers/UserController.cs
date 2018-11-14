using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UserServer.Exceptions;
using UserServer.Services;

namespace UserServer.Controllers
{
    [RoutePrefix("api")]
    public class UserController : ApiController
    {
        private UserServices userServices;

        [HttpGet]
        [Route("users/")]
        public IEnumerable<User> Get()
        {
            userServices = new UserServices();
            return userServices.GetUsers();
        }

        [HttpGet]
        [Route("users/{nickname}")]
        public User Get([FromUri]string nickname)
        {
            userServices = new UserServices();
            return userServices.Get(nickname);
        }

        [HttpPost]
        [Route("users/")]
        public HttpResponseMessage Post([FromBody]User user)
        {
            userServices = new UserServices();
            try
            {
                userServices.Add(user);
                return Request.CreateResponse(HttpStatusCode.Created, user.Nickname);
            } catch(ElementAlreadyExistsException e)
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }
        }

        [HttpPut]
        [Route("users/{nickname}")]
        public HttpResponseMessage Put([FromUri]string nickname, [FromBody]User user)
        {
            userServices = new UserServices();
            try
            {
                userServices.Update(user, nickname);
                return Request.CreateResponse(HttpStatusCode.OK);
            } catch(ElementNotFoundException e)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [HttpDelete]
        [Route("users/{nickname}")]
        public HttpResponseMessage Delete([FromUri]string nickname)
        {
            userServices = new UserServices();
            try
            {
                userServices.Delete(nickname);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (ElementNotFoundException e)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }
    }
}
