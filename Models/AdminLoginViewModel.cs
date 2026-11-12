using System.ComponentModel.DataAnnotations;

namespace BookStoreUI.Models;

public class AdminLoginViewModel
{
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}