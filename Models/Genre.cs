using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Genre
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    // Навігаційна властивість для зв'язку багато-до-багатьох з Book через BookGenre
    public virtual ICollection<BookGenre>? BookGenres { get; set; } = new List<BookGenre>();
}