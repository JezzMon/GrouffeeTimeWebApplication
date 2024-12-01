using GrouffeeTimeWebApplication.Data;
using GrouffeeTimeWebApplication.Models;
using GrouffeeTimeWebApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GrouffeeTimeWebApplication.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IInvoiceService _invoiceService;

        public CartController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            IInvoiceService invoiceService)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _invoiceService = invoiceService;
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }

        public async Task<IActionResult> AddItem(int foodId, int qty = 1, int redirect = 0)
        {
            string userId = GetUserId();

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("user is not logged-in");
                }
                var cart = await GetCart(userId);

                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };

                    _context.ShoppingCarts.Add(cart);
                }

                _context.SaveChanges();

                var cartItem = _context.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.FoodId == foodId);

                if (cartItem is not null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var food = _context.Foods.Find(foodId);
                    cartItem = new CartDetail
                    {
                        FoodId = foodId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = food.Price
                    };
                    _context.CartDetails.Add(cartItem);
                }
                _context.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var cartItemCount = await GetCartItemCount(userId);

            if (redirect == 0)
            {
                return Ok(cartItemCount);
            }
            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> RemoveItem(int foodId)
        {
            string userId = GetUserId();

            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("user is not logged-in");
                }

                var cart = await GetCart(userId);

                if (cart is null)
                {
                    throw new InvalidOperationException("Invalid cart");
                }

                var cartItem = _context.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.FoodId == foodId);

                if (cartItem is null)
                {
                    throw new InvalidOperationException("No items in cart");
                }
                else if (cartItem.Quantity == 1)
                {
                    _context.CartDetails.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = cartItem.Quantity - 1;
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log or handle exception
            }

            var cartItemCount = await GetCartItemCount(userId);

            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> GetUserCart()
        {
            var userId = GetUserId();

            if (userId == null)
            {
                throw new InvalidOperationException("Invalid userid");
            }

            var shoppingCart = await _context.ShoppingCarts
                                  .Include(a => a.CartDetails)
                                  .ThenInclude(a => a.Food)
                                  .ThenInclude(a => a.Stock)
                                  .Include(a => a.CartDetails)
                                  .ThenInclude(a => a.Food)
                                  .ThenInclude(a => a.Category)
                                  .Where(a => a.UserId == userId).FirstOrDefaultAsync();
            return View(shoppingCart);
        }

        public async Task<ShoppingCart> GetCart(string userId)
        {
            var cart = await _context.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
            return cart;
        }

        public async Task<IActionResult> GetTotalItemInCart()
        {
            int cartItem = await GetCartItemCount();
            return Ok(cartItem);
        }

        public async Task<int> GetCartItemCount(string userId = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }

            var data = await (from cart in _context.ShoppingCarts
                              join cartDetail in _context.CartDetails
                              on cart.Id equals cartDetail.ShoppingCartId
                              where cart.UserId == userId
                              select new { cartDetail.Id }
                        ).ToListAsync();

            return data.Count;
        }

        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("User is not logged-in");
                }

                var cart = await GetCart(userId);

                if (cart is null)
                {
                    throw new InvalidOperationException("Invalid cart");
                }

                var cartDetail = _context.CartDetails
                                    .Where(a => a.ShoppingCartId == cart.Id).ToList();

                if (cartDetail.Count == 0)
                {
                    throw new InvalidOperationException("Cart is empty");
                }

                var pendingRecord = _context.orderStatuses.FirstOrDefault(s => s.StatusName == "Pending");

                if (pendingRecord is null)
                {
                    throw new InvalidOperationException("Order status does not have Pending status");
                }

                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    Name = model.Name,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber,
                    Address = model.Address,
                    PaymentMethod = model.PaymentMethod,
                    IsPaid = false,
                    OrderStatusId = pendingRecord.Id
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                foreach (var item in cartDetail)
                {
                    var orderDetail = new OrderDetail
                    {
                        FoodId = item.FoodId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };

                    _context.OrderDetails.Add(orderDetail);

                    var stock = await _context.Stocks.FirstOrDefaultAsync(a => a.FoodId == item.FoodId);

                    if (stock == null)
                    {
                        throw new InvalidOperationException("Stock is null");
                    }

                    if (item.Quantity > stock.Quantity)
                    {
                        throw new InvalidOperationException($"Only {stock.Quantity} items(s) are available in the stock");
                    }

                    stock.Quantity -= item.Quantity;
                }

                _context.CartDetails.RemoveRange(cartDetail);
                _context.SaveChanges();

                transaction.Commit();

                return RedirectToAction(nameof(OrderSuccess));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during checkout: {ex.Message}");
                return RedirectToAction(nameof(OrderFailure));
            }
        }

        public async Task<IActionResult> OrderSuccess()
        {
            // Get the most recent order for the current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var latestOrder = await _context.Orders
                .OrderByDescending(o => o.CreateDate)
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (latestOrder == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(latestOrder);
        }

        public IActionResult DownloadInvoice(int orderId)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null)
            {
                return NotFound();
            }

            byte[] pdfBytes = _invoiceService.GenerateInvoicePdf(order);
            return File(pdfBytes, "application/pdf", $"Invoice_{orderId}.pdf");
        }

        public IActionResult OrderFailure()
        {
            return View();
        }
    }
}