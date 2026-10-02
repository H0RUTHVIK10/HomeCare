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
    public class ServiceRecordController : Controller
    {
        private readonly HomeCareDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IApplianceAuthorizationService _authService;

        public ServiceRecordController(
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

            var appliance = await _context.Appliances.FindAsync(applianceId);
            if (appliance == null) return NotFound();

            var model = new ServiceRecordFormViewModel
            {
                ApplianceId = appliance.Id,
                ApplianceName = appliance.Name,
                ApplianceBrand = appliance.Brand,
                ServiceDate = DateTime.Today,
                NextServiceDate = DateTime.Today.AddMonths(6)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRecordFormViewModel model)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessApplianceAsync(model.ApplianceId, userId.Value))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var serviceRecord = new ServiceRecord
            {
                ApplianceId = model.ApplianceId,
                ServiceDate = model.ServiceDate,
                ServiceType = model.ServiceType.Trim(),
                Description = model.Description?.Trim(),
                TechnicianName = model.TechnicianName?.Trim(),
                ServiceProvider = model.ServiceProvider?.Trim(),
                Cost = model.Cost,
                ServiceStatus = string.IsNullOrWhiteSpace(model.ServiceStatus) ? "Completed" : model.ServiceStatus,
                NextServiceDate = model.NextServiceDate,
                Notes = model.Notes?.Trim()
            };

            _context.ServiceRecords.Add(serviceRecord);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Service record '{serviceRecord.ServiceType}' added successfully!";
            return RedirectToAction("Details", "Appliance", new { id = model.ApplianceId, tab = "services" });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessServiceRecordAsync(id, userId.Value))
            {
                return Forbid();
            }

            var record = await _context.ServiceRecords
                .Include(s => s.Appliance)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (record == null) return NotFound();

            var model = new ServiceRecordFormViewModel
            {
                Id = record.Id,
                ApplianceId = record.ApplianceId,
                ApplianceName = record.Appliance.Name,
                ApplianceBrand = record.Appliance.Brand,
                ServiceDate = record.ServiceDate,
                ServiceType = record.ServiceType,
                Description = record.Description,
                TechnicianName = record.TechnicianName,
                ServiceProvider = record.ServiceProvider,
                Cost = record.Cost,
                ServiceStatus = record.ServiceStatus,
                NextServiceDate = record.NextServiceDate,
                Notes = record.Notes
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceRecordFormViewModel model)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessServiceRecordAsync(model.Id, userId.Value))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = await _context.ServiceRecords.FindAsync(model.Id);
            if (record == null) return NotFound();

            record.ServiceDate = model.ServiceDate;
            record.ServiceType = model.ServiceType.Trim();
            record.Description = model.Description?.Trim();
            record.TechnicianName = model.TechnicianName?.Trim();
            record.ServiceProvider = model.ServiceProvider?.Trim();
            record.Cost = model.Cost;
            record.ServiceStatus = model.ServiceStatus;
            record.NextServiceDate = model.NextServiceDate;
            record.Notes = model.Notes?.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Service record updated successfully.";
            return RedirectToAction("Details", "Appliance", new { id = record.ApplianceId, tab = "services" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessServiceRecordAsync(id, userId.Value))
            {
                return Forbid();
            }

            var record = await _context.ServiceRecords.FindAsync(id);
            if (record != null)
            {
                var applianceId = record.ApplianceId;
                _context.ServiceRecords.Remove(record);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Service record deleted.";
                return RedirectToAction("Details", "Appliance", new { id = applianceId, tab = "services" });
            }

            return RedirectToAction("Index", "Appliance");
        }
    }
}
