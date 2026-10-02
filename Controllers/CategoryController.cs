using HomeCare.Data;
using HomeCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeCare.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly HomeCareDbContext _context;

        public CategoryController(HomeCareDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Appliances)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            var trimmedName = category.Name.Trim();
            if (await _context.Categories.AnyAsync(c => c.Name.ToLower() == trimmedName.ToLower()))
            {
                ModelState.AddModelError(nameof(category.Name), "A category with this name already exists.");
                return View(category);
            }

            category.Name = trimmedName;
            category.Icon = string.IsNullOrWhiteSpace(category.Icon) ? "bi-plug" : category.Icon.Trim();
            category.Description = category.Description?.Trim();

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Category '{category.Name}' added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            var existing = await _context.Categories.FindAsync(category.Id);
            if (existing == null) return NotFound();

            var trimmedName = category.Name.Trim();
            if (await _context.Categories.AnyAsync(c => c.Id != category.Id && c.Name.ToLower() == trimmedName.ToLower()))
            {
                ModelState.AddModelError(nameof(category.Name), "A category with this name already exists.");
                return View(category);
            }

            existing.Name = trimmedName;
            existing.Icon = string.IsNullOrWhiteSpace(category.Icon) ? "bi-plug" : category.Icon.Trim();
            existing.Description = category.Description?.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Category '{existing.Name}' updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Appliances)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            if (category.Appliances.Count > 0)
            {
                TempData["ErrorMessage"] = $"Cannot delete category '{category.Name}' because it has {category.Appliances.Count} associated appliance(s). Reassign them first.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Category '{category.Name}' deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
