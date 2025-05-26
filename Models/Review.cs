using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Review
{
    public int Id { get; set; }

    [Required]
    [Range(1, 5)] // Оцінка від 1 до 5
    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

    [Required]
    public int BookId { get; set; }
    [ForeignKey("BookId")]
    public virtual Book? Book { get; set; }

    [Required]
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}