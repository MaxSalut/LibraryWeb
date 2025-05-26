using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryWeb.DTOs
{
    public class BookCreateDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters.")]
        public string Title { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot be longer than 2000 characters.")]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PublicationDate { get; set; }

        [StringLength(50, ErrorMessage = "ISBN cannot be longer than 50 characters.")]
        public string? ISBN { get; set; }

        [Required(ErrorMessage = "Author ID is required.")]
        public int AuthorId { get; set; }

        public int? PublisherId { get; set; }

        public List<int>? GenreIds { get; set; } = new List<int>();
    }
}