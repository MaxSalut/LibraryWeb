using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryWeb.DTOs
{
    public class BorrowingRecordCreateDto
    {
        [Required(ErrorMessage = "Book ID is required.")]
        public int BookId { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        [DataType(DataType.Date)] // Або DateTime, якщо потрібен час
        public DateTime DueDate { get; set; }
    }
}