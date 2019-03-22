using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace LoadInfoAdapterStub.Controllers
{
    [Produces("application/xml")]
    [Route("api/LoadInfo")]
    public class LoadInfoController : Controller
    {
        private static LoadInfoResponse _response = new LoadInfoResponse();

        [Route("LoadInfo")]
        [HttpGet]
        public IActionResult LoadInfo()
        {
            return Ok(_response);
        }

        [Route("LoadInfo")]
        [HttpPost]
        public IActionResult LoadInfo([FromBody]LoadInfoResponse loadInfoResponse)
        {
            _response = loadInfoResponse;
            return Ok(_response);
        }

        [Route("LoadInfos")]
        [HttpGet]
        public IActionResult LoadInfos(int from, int to)
        {
            var count = to > 0 ? new Random().Next(from, to) : from;
            return Ok(Enumerable.Repeat(_response, count).ToList());
        }
    }
}