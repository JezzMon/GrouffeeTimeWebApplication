using GrouffeeTimeWebApplication.Data;
using GrouffeeTimeWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GrouffeeTimeWebApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        // Reports
        public async Task<ActionResult> Index(DateTime? sDate = null, DateTime? eDate = null)
        {
            try
            {
                DateTime startDate = sDate ?? DateTime.UtcNow.AddDays(-7);
                DateTime endDate = eDate ?? DateTime.UtcNow;

                var startDateParam = new SqlParameter("@startDate", startDate);
                var endDateParam = new SqlParameter("@endDate", endDate);
                var topSellingFoods = await _context.Database
                    .SqlQueryRaw<SoldFoodModel>("exec Usp_GetSellingFoodsByDate @startDate,@endDate", startDateParam, endDateParam)
                    .ToListAsync();

                var vm = new SoldFoodsVm(startDate, endDate, topSellingFoods);
                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = "Something went wrong\n" + ex.Message;
                return RedirectToAction("Index", "Category");
            }
        }

        // Order Management
        public async Task<IActionResult> AllOrders(bool getAll = true)
        {
            var orders = _context.Orders
                .Include(x => x.OrderStatus)
                .Include(x => x.OrderDetail)
                .ThenInclude(x => x.Food)
                .ThenInclude(x => x.Category)
                .AsQueryable();

            if (!getAll)
            {
                var userId = GetUserId();
                orders = orders.Where(a => a.UserId == userId);
            }

            return View(await orders.ToListAsync());
        }

        public async Task<IActionResult> TogglePaymentStatus(int orderId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    throw new InvalidOperationException($"order with id:{orderId} is not found");
                }

                order.IsPaid = !order.IsPaid;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(AllOrders));
        }

        public async Task<IActionResult> UpdateOrderStatus(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                throw new InvalidOperationException($"Order with id:{orderId} does not found.");
            }

            var orderStatusList = await _context.orderStatuses.Select(orderStatus =>
                new SelectListItem
                {
                    Value = orderStatus.Id.ToString(),
                    Text = orderStatus.StatusName,
                    Selected = order.OrderStatusId == orderStatus.Id
                }).ToListAsync();

            var data = new UpdateOrderStatus
            {
                OrderId = orderId,
                OrderStatusId = order.OrderStatusId,
                OrderStatusList = orderStatusList
            };

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(UpdateOrderStatus data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    data.OrderStatusList = await _context.orderStatuses.Select(orderStatus =>
                        new SelectListItem
                        {
                            Value = orderStatus.Id.ToString(),
                            Text = orderStatus.StatusName,
                            Selected = orderStatus.Id == data.OrderStatusId
                        }).ToListAsync();

                    return View(data);
                }

                var order = await _context.Orders.FindAsync(data.OrderId);
                if (order == null)
                {
                    throw new InvalidOperationException($"order with id:{data.OrderId} is not found");
                }

                order.OrderStatusId = data.OrderStatusId;
                await _context.SaveChangesAsync();

                TempData["msg"] = "Updated successfully";
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Something went wrong";
            }

            return RedirectToAction(nameof(UpdateOrderStatus), new { orderId = data.OrderId });
        }

        // Food Management
        public async Task<IActionResult> ManageFood()
        {
            var foods = await _context.Foods.Include(a => a.Category).ToListAsync();
            return View(foods);
        }

        [HttpPost]
        public async Task<IActionResult> AddFood(Food food)
        {
            _context.Foods.Add(food);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageFood));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFood(Food food)
        {
            _context.Foods.Update(food);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageFood));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFood(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food != null)
            {
                _context.Foods.Remove(food);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageFood));
        }

        // Category Management
        public async Task<IActionResult> ManageCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageCategories));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageCategories));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageCategories));
        }

        // Utility Method
        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            return _userManager.GetUserId(principal);
        }
    }
}
