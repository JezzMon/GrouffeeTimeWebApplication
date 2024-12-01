using System.ComponentModel.DataAnnotations;

namespace GrouffeeTimeWebApplication.Models;

public class CheckoutModel
{
    [Required]
    [MaxLength(30)]
    public string? Name { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(30)]
    public string? Email { get; set; }

    [Required]
    public string? MobileNumber { get; set; }

    [MaxLength(100)]
    public string? Address { get; set; }

    [Required]
    public string? PaymentMethod { get; set; }
}
