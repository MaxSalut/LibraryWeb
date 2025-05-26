public class BookGenre
{
    // Композитний первинний ключ буде налаштований у DbContext
    public int BookId { get; set; }
    public virtual Book? Book { get; set; }

    public int GenreId { get; set; }
    public virtual Genre? Genre { get; set; }
}