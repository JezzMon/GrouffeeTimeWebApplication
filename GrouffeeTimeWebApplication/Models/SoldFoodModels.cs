namespace GrouffeeTimeWebApplication.Models;

//public record SoldFoodModel(string FoodName, string Description, int TotalUnitSold);
public record SoldFoodModel(string FoodName, string Description, int TotalUnitSold, decimal TotalRevenue);

public record SoldFoodsVm(DateTime StartDate, DateTime EndDate, IEnumerable<SoldFoodModel> SoldFoods);