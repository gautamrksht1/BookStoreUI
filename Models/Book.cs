using System.Text.Json.Serialization;

namespace BookStoreUI.Models;

public class Book
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("available_quantity")]
    public int AvailableQuantity { get; set; }
}
