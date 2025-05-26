using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Для ForeignKey

public class Book
{
    public int Id { get; set; } // Первинний ключ

    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    public string? Description { get; set; }

    [DataType(DataType.Date)] // Вказує, що це дата (без часу)
    public DateTime? PublicationDate { get; set; }

    [StringLength(50)]
    public string? ISBN { get; set; }

    // Зовнішній ключ для зв'язку з автором
    public int AuthorId { get; set; }

    // Навігаційна властивість: кожна книга належить одному автору
    [ForeignKey("AuthorId")]
    public virtual Author? Author { get; set; }

    public virtual ICollection<BookGenre>? BookGenres { get; set; } = new List<BookGenre>();

    public int? PublisherId { get; set; } // Зовнішній ключ (може бути null, якщо видавництво невідоме)

    [ForeignKey("PublisherId")]
    public virtual Publisher? Publisher { get; set; }
    public virtual ICollection<BorrowingRecord>? BorrowingRecords { get; set; } = new List<BorrowingRecord>();

    public virtual ICollection<Review>? Reviews { get; set; } = new List<Review>();
}