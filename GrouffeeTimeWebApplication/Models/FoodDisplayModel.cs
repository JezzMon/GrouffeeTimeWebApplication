namespace GrouffeeTimeWebApplication.Models
{
    public class FoodDisplayModel
    {
        public IEnumerable<Food> Foods { get; set; }

        public IEnumerable<Category> Categories { get; set; }

        public string Search { get; set; } = "";

        public int CategoryId { get; set; } = 0;
    }
}
