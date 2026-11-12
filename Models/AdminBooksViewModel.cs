namespace BookStoreUI.Models;

public class AdminBooksViewModel
{
    public BookEntryViewModel BookEntry { get; set; } = new();
    public List<Book> Books { get; set; } = [];
    public string? ErrorMessage { get; set; }
}