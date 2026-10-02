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
    public class ReminderController : Controller
    {
        private readonly HomeCareDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IApplianceAuthorizationService _authService;

        public ReminderController(
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

            var model = new ReminderFormViewModel
            {
                ApplianceId = appliance.Id,
                ApplianceName = appliance.Name,
                ApplianceBrand = appliance.Brand,
                ReminderDate = DateTime.Today.AddDays(7),
                ReminderType = "Maintenance"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReminderFormViewModel model)
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

            var reminder = new Reminder
            {
                ApplianceId = model.ApplianceId,
                UserId = userId.Value,
                Title = model.Title.Trim(),
                Description = model.Description?.Trim(),
                ReminderDate = model.ReminderDate,
                ReminderType = model.ReminderType,
                IsCompleted = model.IsCompleted,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Reminder '{reminder.Title}' scheduled successfully!";
            return RedirectToAction("Details", "Appliance", new { id = model.ApplianceId, tab = "reminders" });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessReminderAsync(id, userId.Value))
            {
                return Forbid();
            }

            var reminder = await _context.Reminders
                .Include(r => r.Appliance)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reminder == null) return NotFound();

            var model = new ReminderFormViewModel
            {
                Id = reminder.Id,
                ApplianceId = reminder.ApplianceId,
                ApplianceName = reminder.Appliance.Name,
                ApplianceBrand = reminder.Appliance.Brand,
                Title = reminder.Title,
                Description = reminder.Description,
                ReminderDate = reminder.ReminderDate,
                ReminderType = reminder.ReminderType,
                IsCompleted = reminder.IsCompleted
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ReminderFormViewModel model)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessReminderAsync(model.Id, userId.Value))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var reminder = await _context.Reminders.FindAsync(model.Id);
            if (reminder == null) return NotFound();

            reminder.Title = model.Title.Trim();
            reminder.Description = model.Description?.Trim();
            reminder.ReminderDate = model.ReminderDate;
            reminder.ReminderType = model.ReminderType;
            reminder.IsCompleted = model.IsCompleted;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reminder updated successfully.";
            return RedirectToAction("Details", "Appliance", new { id = reminder.ApplianceId, tab = "reminders" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleComplete(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessReminderAsync(id, userId.Value))
            {
                return Forbid();
            }

            var reminder = await _context.Reminders.FindAsync(id);
            if (reminder != null)
            {
                reminder.IsCompleted = !reminder.IsCompleted;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = reminder.IsCompleted
                    ? $"Task '{reminder.Title}' marked as completed."
                    : $"Task '{reminder.Title}' marked as incomplete.";

                return RedirectToAction("Details", "Appliance", new { id = reminder.ApplianceId, tab = "reminders" });
            }

            return RedirectToAction("Index", "Appliance");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessReminderAsync(id, userId.Value))
            {
                return Forbid();
            }

            var reminder = await _context.Reminders.FindAsync(id);
            if (reminder != null)
            {
                var applianceId = reminder.ApplianceId;
                _context.Reminders.Remove(reminder);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Reminder deleted.";
                return RedirectToAction("Details", "Appliance", new { id = applianceId, tab = "reminders" });
            }

            return RedirectToAction("Index", "Appliance");
        }
    }
}
