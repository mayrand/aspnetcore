using System;
namespace restful_api_building.Models
{
    public class AuthorDto
    {
        public Guid id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Genre { get; set; }
    }
}