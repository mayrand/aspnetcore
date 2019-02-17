using System;
namespace restful_api_building.Models
{
    public class BookDto
    {
        public Guid id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid AuthorId { get; set; }
    }
}