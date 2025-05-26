using System.ComponentModel.DataAnnotations;

namespace LibraryWeb.DTOs
{
    public class ReviewUpdateDto
    {
        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment cannot be longer than 1000 characters.")]
        public string? Comment { get; set; }
    }
}