using HomeCare.Data;
using HomeCare.Models;
using HomeCare.Services;
using HomeCare.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeCare.Controllers
{
    [Authorize]
    public class WarrantyController : Controller
    {
        private readonly HomeCareDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IApplianceAuthorizationService _authService;

        public WarrantyController(
            HomeCareDbContext context,
            IUserContext userContext,
            IApplianceAuthorizationService authService)
        {
            _context = context;
            _userContext = userContext;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int applianceId)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessApplianceAsync(applianceId, userId.Value))
            {
                return Forbid();
            }

            var appliance = await _context.Appliances
                .Include(a => a.Warranty)
                .FirstOrDefaultAsync(a => a.Id == applianceId);

            if (appliance == null) return NotFound();

            if (appliance.Warranty != null)
            {
                // Already has warranty, redirect to Edit
                return RedirectToAction(nameof(Edit), new { id = appliance.Warranty.Id });
            }

            var model = new WarrantyFormViewModel
            {
                ApplianceId = appliance.Id,
                ApplianceName = appliance.Name,
                ApplianceBrand = appliance.Brand,
                StartDate = appliance.PurchaseDate ?? DateTime.Today,
                EndDate = (appliance.PurchaseDate ?? DateTime.Today).AddYears(1)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WarrantyFormViewModel model)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessApplianceAsync(model.ApplianceId, userId.Value))
            {
                return Forbid();
            }

            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError(nameof(model.EndDate), "Warranty end date must be after the start date.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var warranty = new Warranty
            {
                ApplianceId = model.ApplianceId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Provider = model.Provider?.Trim(),
                WarrantyNumber = model.WarrantyNumber?.Trim(),
                CoverageDetails = model.CoverageDetails?.Trim(),
                ContactNumber = model.ContactNumber?.Trim(),
                Terms = model.Terms?.Trim()
            };

            _context.Warranties.Add(warranty);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Warranty information saved successfully!";
            return RedirectToAction("Details", "Appliance", new { id = model.ApplianceId, tab = "warranty" });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessWarrantyAsync(id, userId.Value))
            {
                return Forbid();
            }

            var warranty = await _context.Warranties
                .Include(w => w.Appliance)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (warranty == null) return NotFound();

            var model = new WarrantyFormViewModel
            {
                Id = warranty.Id,
                ApplianceId = warranty.ApplianceId,
                ApplianceName = warranty.Appliance.Name,
                ApplianceBrand = warranty.Appliance.Brand,
                StartDate = warranty.StartDate,
                EndDate = warranty.EndDate,
                Provider = warranty.Provider,
                WarrantyNumber = warranty.WarrantyNumber,
                CoverageDetails = warranty.CoverageDetails,
                ContactNumber = warranty.ContactNumber,
                Terms = warranty.Terms
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WarrantyFormViewModel model)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessWarrantyAsync(model.Id, userId.Value))
            {
                return Forbid();
            }

            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError(nameof(model.EndDate), "Warranty end date must be after the start date.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var warranty = await _context.Warranties.FindAsync(model.Id);
            if (warranty == null) return NotFound();

            warranty.StartDate = model.StartDate;
            warranty.EndDate = model.EndDate;
            warranty.Provider = model.Provider?.Trim();
            warranty.WarrantyNumber = model.WarrantyNumber?.Trim();
            warranty.CoverageDetails = model.CoverageDetails?.Trim();
            warranty.ContactNumber = model.ContactNumber?.Trim();
            warranty.Terms = model.Terms?.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Warranty details updated successfully.";
            return RedirectToAction("Details", "Appliance", new { id = warranty.ApplianceId, tab = "warranty" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessWarrantyAsync(id, userId.Value))
            {
                return Forbid();
            }

            var warranty = await _context.Warranties.FindAsync(id);
            if (warranty != null)
            {
                var applianceId = warranty.ApplianceId;
                _context.Warranties.Remove(warranty);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Warranty record removed.";
                return RedirectToAction("Details", "Appliance", new { id = applianceId, tab = "warranty" });
            }

            return RedirectToAction("Index", "Appliance");
        }
    }
}
