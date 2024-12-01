using System.ComponentModel.DataAnnotations;

namespace GrouffeeTimeWebApplication.Models
{
    public class CategoryDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        public string CategoryName { get; set; }
    }
}
