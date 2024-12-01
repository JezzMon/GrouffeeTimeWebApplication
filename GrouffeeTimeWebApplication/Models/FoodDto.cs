using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GrouffeeTimeWebApplication.Models;

public class FoodDto
{
    public int Id { get; set; }

    [Required]
    [MaxLength(40)]
    public string? FoodName { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    [Required]
    public double Price { get; set; }

    public string? Image { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public IFormFile? ImageFile { get; set; }

    public IEnumerable<SelectListItem>? CategoryList { get; set; }
}
