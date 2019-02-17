using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using restful_api_building.Services;
using restful_api_building.Models;
using AutoMapper;

namespace restful_api_building.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private ILibraryRepository _libraryRepository;
        public AuthorsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet]
        //[Route("GetAuthors")]
        public IActionResult GetAuthors()
        {
            //throw new Exception();
            var authorsFromRepo = _libraryRepository.GetAuthors();
            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
            return Ok(authors);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public IActionResult GetAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);
            if (authorFromRepo == null)
                return NotFound();
            var author = Mapper.Map<AuthorDto>(authorFromRepo);
            return Ok(author);
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
