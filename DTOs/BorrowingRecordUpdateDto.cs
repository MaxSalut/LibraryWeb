using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryWeb.DTOs
{
    public class BorrowingRecordUpdateDto
    {
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }
    }
}