using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace APIServices.Controllers
{
    [Produces("application/json")]
    [Route("api/State")]
    public class StateController : Controller
    {
        // GET: api/State
        [HttpGet]
        public int Get()
        {
            if (State.Delay > -1)
                return State.Delay;
            else
                return new Random().Next(0, 5) * 1000;
        }

        // GET: api/State/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/State
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/State/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
