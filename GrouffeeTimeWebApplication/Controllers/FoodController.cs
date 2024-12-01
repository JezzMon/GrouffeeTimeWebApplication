using GrouffeeTimeWebApplication.Data;
using GrouffeeTimeWebApplication.Models;
using GrouffeeTimeWebApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrouffeeTimeWebApplication.Controllers;

[Authorize(Roles = "Admin")]
public class FoodController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileService _fileService;

    public FoodController(ApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task<IActionResult> Index()
    {
        var foods = await _context.Foods.Include(a => a.Category).ToListAsync();
        return View(foods);
    }

    public async Task<IActionResult> AddFood()
    {
        var categorySelectList = await GetCategorySelectList();
        FoodDto foodToAdd = new() { CategoryList = categorySelectList };
        return View(foodToAdd);
    }

    [HttpPost]
    public async Task<IActionResult> AddFood(FoodDto foodToAdd)
    {
        var categorySelectList = await GetCategorySelectList();
        foodToAdd.CategoryList = categorySelectList;

        if (!ModelState.IsValid)
            return View(foodToAdd);

        try
        {
            if (foodToAdd.ImageFile != null)
            {
                if (foodToAdd.ImageFile.Length > 1 * 1024 * 1024)
                {
                    throw new InvalidOperationException("Image file can not exceed 1 MB");
                }

                string[] allowedExtensions = [".jpeg", ".jpg", ".png"];
                string imageName = await _fileService.SaveFile(foodToAdd.ImageFile, allowedExtensions);
                foodToAdd.Image = imageName;
            }

            Food food = new()
            {
                Id = foodToAdd.Id,
                FoodName = foodToAdd.FoodName,
                Description = foodToAdd.Description,
                Image = foodToAdd.Image,
                CategoryId = foodToAdd.CategoryId,
                Price = foodToAdd.Price
            };

            _context.Foods.Add(food);
            await _context.SaveChangesAsync();

            TempData["successMessage"] = "Food is added successfully";
            return RedirectToAction(nameof(AddFood));
        }
        catch (InvalidOperationException ex)
        {
            TempData["errorMessage"] = ex.Message;
            return View(foodToAdd);
        }
        catch (FileNotFoundException ex)
        {
            TempData["errorMessage"] = ex.Message;
            return View(foodToAdd);
        }
        catch (Exception)
        {
            TempData["errorMessage"] = "Error on saving data";
            return View(foodToAdd);
        }
    }

    public async Task<IActionResult> UpdateFood(int id)
    {
        var food = await _context.Foods.FindAsync(id);

        if (food == null)
        {
            TempData["errorMessage"] = $"Food with the id: {id} does not found";
            return RedirectToAction(nameof(Index));
        }

        var categorySelectList = await GetCategorySelectList(food.CategoryId);

        FoodDto foodToUpdate = new()
        {
            CategoryList = categorySelectList,
            FoodName = food.FoodName,
            Description = food.Description,
            CategoryId = food.CategoryId,
            Price = food.Price,
            Image = food.Image
        };

        return View(foodToUpdate);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateFood(FoodDto foodToUpdate)
    {
        var categorySelectList = await GetCategorySelectList(foodToUpdate.CategoryId);
        foodToUpdate.CategoryList = categorySelectList;

        if (!ModelState.IsValid)
            return View(foodToUpdate);

        try
        {
            string oldImage = "";

            if (foodToUpdate.ImageFile != null)
            {
                if (foodToUpdate.ImageFile.Length > 1 * 1024 * 1024)
                {
                    throw new InvalidOperationException("Image file can not exceed 1 MB");
                }

                string[] allowedExtensions = [".jpeg", ".jpg", ".png"];
                string imageName = await _fileService.SaveFile(foodToUpdate.ImageFile, allowedExtensions);
                oldImage = foodToUpdate.Image;
                foodToUpdate.Image = imageName;
            }

            Food food = new()
            {
                Id = foodToUpdate.Id,
                FoodName = foodToUpdate.FoodName,
                Description = foodToUpdate.Description,
                CategoryId = foodToUpdate.CategoryId,
                Price = foodToUpdate.Price,
                Image = foodToUpdate.Image
            };

            _context.Foods.Update(food);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(oldImage))
            {
                _fileService.DeleteFile(oldImage);
            }

            TempData["successMessage"] = "Food is updated successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["errorMessage"] = ex.Message;
            return View(foodToUpdate);
        }
        catch (FileNotFoundException ex)
        {
            TempData["errorMessage"] = ex.Message;
            return View(foodToUpdate);
        }
        catch (Exception)
        {
            TempData["errorMessage"] = "Error on saving data";
            return View(foodToUpdate);
        }
    }

    public async Task<IActionResult> DeleteFood(int id)
    {
        try
        {
            var food = await _context.Foods.FindAsync(id);

            if (food == null)
            {
                TempData["errorMessage"] = $"Food with the id: {id} does not found";
            }
            else
            {
                _context.Foods.Remove(food);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(food.Image))
                {
                    _fileService.DeleteFile(food.Image);
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["errorMessage"] = ex.Message;
        }
        catch (FileNotFoundException ex)
        {
            TempData["errorMessage"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["errorMessage"] = "Error on deleting the data";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<IEnumerable<SelectListItem>> GetCategorySelectList(int? selectedCategoryId = null)
    {
        return await _context.Categories
            .Select(category => new SelectListItem
            {
                Text = category.CategoryName,
                Value = category.Id.ToString(),
                Selected = selectedCategoryId.HasValue && category.Id == selectedCategoryId
            })
            .ToListAsync();
    }
}
