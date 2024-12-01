using System.ComponentModel.DataAnnotations;

namespace GrouffeeTimeWebApplication.Models
{
    public class StockDto
    {
        public int FoodId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative value.")]
        public int Quantity { get; set; }
    }
}
