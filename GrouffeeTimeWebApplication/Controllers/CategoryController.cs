using GrouffeeTimeWebApplication.Data;
using GrouffeeTimeWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrouffeeTimeWebApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(CategoryDto category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            try
            {
                var categoryToAdd = new Category { CategoryName = category.CategoryName, Id = category.Id };

                _context.Categories.Add(categoryToAdd);
                await _context.SaveChangesAsync();

                TempData["successMessage"] = "Category added successfully";

                return RedirectToAction(nameof(AddCategory));
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = "Category could not be added!";
                return View(category);
            }
        }

        public async Task<IActionResult> UpdateCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category is null)
            {
                throw new InvalidOperationException($"Category with id: {id} does not exist");
            }

            var categoryToUpdate = new CategoryDto
            {
                Id = category.Id,
                CategoryName = category.CategoryName
            };

            return View(categoryToUpdate);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(CategoryDto categoryToUpdate)
        {
            if (!ModelState.IsValid)
            {
                return View(categoryToUpdate);
            }

            try
            {
                var category = new Category { CategoryName = categoryToUpdate.CategoryName, Id = categoryToUpdate.Id };

                _context.Categories.Update(category);
                await _context.SaveChangesAsync();

                TempData["successMessage"] = "Category is updated successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = "Category could not be updated!";
                return View(categoryToUpdate);
            }
        }

        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category is null)
            {
                throw new InvalidOperationException($"Category with id: {id} does not exist");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
