using System;

namespace LibraryWeb.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewDate { get; set; }
        public int BookId { get; set; }
        public string? BookTitle { get; set; } // Додамо для зручності
        public int UserId { get; set; }
        public string? Username { get; set; } // Додамо для зручності
    }
}