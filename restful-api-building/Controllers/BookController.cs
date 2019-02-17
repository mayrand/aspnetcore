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
    [Route("api/authors/{authorId}/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private ILibraryRepository _libraryRepository;
        public BooksController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet()]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if(!_libraryRepository.AuthorExists(authorId))
            return NotFound();
            var booksForAuthorFromRepo = _libraryRepository.GetBooksForAuthor(authorId);
            var booksForAuthor = Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepo);
            return Ok(booksForAuthor);
        }
    }
}