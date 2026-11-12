using System.ComponentModel.DataAnnotations;

namespace BookStoreUI.Models;

public class BookEntryViewModel
{
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Author { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0.01", "10000000")]
    public decimal? Price { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    [Display(Name = "Available quantity")]
    public int? AvailableQuantity { get; set; }
}