using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class BorrowingRecord
{
    public int Id { get; set; }

    [Required]
    public DateTime BorrowDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; } // Фактична дата повернення, null якщо ще не повернуто

    [Required]
    public int BookId { get; set; }
    [ForeignKey("BookId")]
    public virtual Book? Book { get; set; }

    [Required]
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}