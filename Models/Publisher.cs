using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Publisher
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    // Навігаційна властивість: одне видавництво може мати багато книг
    public virtual ICollection<Book>? Books { get; set; } = new List<Book>();
}