namespace GrouffeeTimeWebApplication.Models
{
    public class StockDisplayModel
    {
        public int Id { get; set; }

        public int FoodId { get; set; }

        public int Quantity { get; set; }

        public string? FoodName { get; set; }
    }
}
