using GrouffeeTimeWebApplication.Data;
using GrouffeeTimeWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: ReportsController
    public async Task<ActionResult> Index(DateTime? sDate = null, DateTime? eDate = null)
    {
        try
        {
            // by default, get last 7 days record
            DateTime startDate = sDate ?? DateTime.UtcNow.AddDays(-7);
            DateTime endDate = eDate ?? DateTime.UtcNow;

            var startDateParam = new SqlParameter("@startDate", startDate);
            var endDateParam = new SqlParameter("@endDate", endDate);
            var topSellingFoods = await _context.Database.SqlQueryRaw<SoldFoodModel>("exec Usp_GetSellingFoodsByDate @startDate,@endDate", startDateParam, endDateParam).ToListAsync();

            var vm = new SoldFoodsVm(startDate, endDate, topSellingFoods);
            return View(vm);
        }
        catch (Exception ex)
        {
            TempData["errorMessage"] = "Something went wrong\n" + ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }
}
