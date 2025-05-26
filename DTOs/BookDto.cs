using System;
using System.Collections.Generic;

namespace LibraryWeb.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? PublicationDate { get; set; }
        public string? ISBN { get; set; }

        public AuthorDto? Author { get; set; }
        public PublisherDto? Publisher { get; set; }
        public List<GenreDto> Genres { get; set; } = new List<GenreDto>();
    }
}