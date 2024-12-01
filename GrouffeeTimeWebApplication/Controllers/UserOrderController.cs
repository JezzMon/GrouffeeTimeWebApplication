using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrouffeeTimeWebApplication.Data;

namespace GrouffeeTimeWebApplication.Controllers
{
    [Authorize(Roles = "User")]
    public class UserOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public UserOrderController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<IActionResult> UserOrders(bool getAll = false)
        {
            var orders = _context.Orders
                           .Include(x => x.OrderStatus)
                           .Include(x => x.OrderDetail)
                           .ThenInclude(x => x.Food)
                           .ThenInclude(x => x.Category).AsQueryable();

            if (!getAll)
            {
                var userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("User is not logged-in");
                }
                orders = orders.Where(a => a.UserId == userId);
            }

            return View(await orders.ToListAsync());
        }

        public async Task<IActionResult> ChangeOrderStatus(UpdateOrderStatus data)
        {
            var order = await _context.Orders.FindAsync(data.OrderId);

            if (order == null)
            {
                return NotFound($"Order with id:{data.OrderId} is not found");
            }

            order.OrderStatusId = data.OrderStatusId;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(UserOrders));
        }

        public async Task<IActionResult> TogglePaymentStatus(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound($"Order with id:{orderId} is not found");
            }

            order.IsPaid = !order.IsPaid;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(UserOrders));
        }

        public async Task<IActionResult> GetOrderStatuses()
        {
            var statuses = await _context.orderStatuses.ToListAsync();
            return Json(statuses);
        }

        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            return Json(order);
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}
