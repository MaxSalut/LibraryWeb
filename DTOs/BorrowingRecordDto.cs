using System;

namespace LibraryWeb.DTOs
{
    public class BorrowingRecordDto
    {
        public int Id { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; } // Може бути null, якщо книгу ще не повернули

        public int BookId { get; set; }
        public string? BookTitle { get; set; } // Для зручності

        public int UserId { get; set; }
        public string? Username { get; set; } // Для зручності
    }
}