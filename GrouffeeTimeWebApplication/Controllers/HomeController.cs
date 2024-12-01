using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using GrouffeeTimeWebApplication.Models;
using GrouffeeTimeWebApplication.Data;

namespace GrouffeeTimeWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Home/Index
        public async Task<IActionResult> Index(string search = "", int categoryId = 0)
        {
            // Filter and query data directly from the database context
            var foodsQuery = from food in _context.Foods
                             join category in _context.Categories
                             on food.CategoryId equals category.Id
                             join stock in _context.Stocks
                             on food.Id equals stock.FoodId into food_stocks
                             from foodWithStock in food_stocks.DefaultIfEmpty()
                             where string.IsNullOrWhiteSpace(search) || food.FoodName.ToLower().StartsWith(search.ToLower())
                             select new Food
                             {
                                 Id = food.Id,
                                 Image = food.Image,
                                 Description = food.Description,
                                 FoodName = food.FoodName,
                                 CategoryId = food.CategoryId,
                                 Price = food.Price,
                                 CategoryName = category.CategoryName,
                                 Quantity = foodWithStock == null ? 0 : foodWithStock.Quantity
                             };

            if (categoryId > 0)
            {
                foodsQuery = foodsQuery.Where(f => f.CategoryId == categoryId);
            }

            // Retrieve the filtered list of foods
            IEnumerable<Food> foods = await foodsQuery.ToListAsync();

            // Get all categories (no filtering)
            IEnumerable<Category> categories = await _context.Categories.ToListAsync();

            // Create and populate the view model
            FoodDisplayModel foodModel = new FoodDisplayModel
            {
                Foods = foods,
                Categories = categories,
                Search = search,
                CategoryId = categoryId
            };

            return View(foodModel);
        }

        public IActionResult Privacy()
        {
            return View();

        }

        public IActionResult Location()
        {
            return View();
        }

        // Error page action with cache control
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
