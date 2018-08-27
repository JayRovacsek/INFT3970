using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace INFT3970Project.Controllers
{
    [Produces("application/json")]
    [Route("api/Availability")]
    public class AvailabilityController : Controller
    {
        [HttpGet]
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }
    }
}