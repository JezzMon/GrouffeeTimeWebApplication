using GrouffeeTimeWebApplication.Data;
using GrouffeeTimeWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrouffeeTimeWebApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StockController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search = "")
        {
            var stocks = await GetStocksInternal(search);
            return View(stocks);
        }

        public async Task<IActionResult> ManageStock(int foodId)
        {
            var existingStock = await GetStockByFoodIdInternal(foodId);

            var stock = new StockDto
            {
                FoodId = foodId,
                Quantity = existingStock != null 
                    ? existingStock.Quantity 
                    : 0
            };
            return View(stock);
        }

        [HttpPost]
        public async Task<IActionResult> ManageStock(StockDto stock)
        {
            if (!ModelState.IsValid)
            {
                return View(stock);
            }

            try
            {
                await ManageStockInternal(stock);
                TempData["successMessage"] = "Stock is updated successfully.";
            }
            catch (Exception)
            {
                TempData["errorMessage"] = "Something went wrong!!";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<Stock?> GetStockByFoodIdInternal(int foodId) => 
            await _context.Stocks.FirstOrDefaultAsync(s => s.FoodId == foodId);

        private async Task ManageStockInternal(StockDto stockToManage)
        {
            var existingStock = await GetStockByFoodIdInternal(stockToManage.FoodId);

            if (existingStock is null)
            {
                var stock = new Stock { FoodId = stockToManage.FoodId, Quantity = stockToManage.Quantity };
                _context.Stocks.Add(stock);
            }
            else
            {
                existingStock.Quantity = stockToManage.Quantity;
            }
            await _context.SaveChangesAsync();
        }

        private async Task<IEnumerable<StockDisplayModel>> GetStocksInternal(string search = "")
        {
            var stocks = await (from food in _context.Foods
                                join stock in _context.Stocks
                                on food.Id equals stock.FoodId
                                into food_stock
                                from foodStock in food_stock.DefaultIfEmpty()
                                where string.IsNullOrWhiteSpace(search) || food.FoodName.ToLower().Contains(search.ToLower())
                                select new StockDisplayModel
                                {
                                    FoodId = food.Id,
                                    FoodName = food.FoodName,
                                    Quantity = foodStock == null ? 0 : foodStock.Quantity
                                }
                                ).ToListAsync();
            return stocks;
        }
    }
}
