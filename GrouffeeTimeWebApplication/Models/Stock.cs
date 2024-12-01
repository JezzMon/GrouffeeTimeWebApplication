using System.ComponentModel.DataAnnotations.Schema;

namespace GrouffeeTimeWebApplication.Models
{
    [Table("Stock")]
    public class Stock
    {
        public int Id { get; set; }

        public int FoodId { get; set; }

        public int Quantity { get; set; }

        public Food? Food { get; set; }
    }
}
