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
    public class HomesController : Controller
    {
        private readonly HomeCareDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IApplianceAuthorizationService _authService;
        private readonly IHealthScoreService _healthScoreService;

        public HomesController(
            HomeCareDbContext context,
            IUserContext userContext,
            IApplianceAuthorizationService authService,
            IHealthScoreService healthScoreService)
        {
            _context = context;
            _userContext = userContext;
            _authService = authService;
            _healthScoreService = healthScoreService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var homes = await _context.Homes
                .Include(h => h.Appliances)
                .ThenInclude(a => a.Warranty)
                .Include(h => h.Appliances)
                .ThenInclude(a => a.ServiceRecords)
                .Include(h => h.Appliances)
                .ThenInclude(a => a.Reminders)
                .Where(h => h.UserId == userId.Value)
                .OrderBy(h => h.Name)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var activeHomeId = _userContext.ActiveHomeId;
            var cards = new List<HomeCardViewModel>();

            foreach (var h in homes)
            {
                var appliances = h.Appliances.ToList();
                int activeW = appliances.Count(a => a.Warranty != null && a.Warranty.EndDate >= now);
                int expiringW = appliances.Count(a => a.Warranty != null && a.Warranty.EndDate >= now && (a.Warranty.EndDate - now).TotalDays <= 30);
                int overdueM = appliances.SelectMany(a => a.Reminders).Count(r => !r.IsCompleted && r.ReminderDate < now);
                decimal spending = appliances.SelectMany(a => a.ServiceRecords).Sum(s => s.Cost ?? 0);

                int avgHealth = 100;
                string rating = "Good";
                string badge = "bg-primary";

                if (appliances.Count > 0)
                {
                    var scores = appliances.Select(a => _healthScoreService.CalculateApplianceHealth(a).Score).ToList();
                    avgHealth = (int)scores.Average();

                    if (avgHealth >= 90) { rating = "Excellent"; badge = "bg-success"; }
                    else if (avgHealth >= 70) { rating = "Good"; badge = "bg-primary"; }
                    else if (avgHealth >= 40) { rating = "Needs Attention"; badge = "bg-warning text-dark"; }
                    else { rating = "Critical"; badge = "bg-danger"; }
                }

                cards.Add(new HomeCardViewModel
                {
                    Id = h.Id,
                    Name = h.Name,
                    Address = h.Address,
                    City = h.City,
                    State = h.State,
                    HomeType = h.HomeType,
                    CreatedAt = h.CreatedAt,
                    ApplianceCount = appliances.Count,
                    ActiveWarrantiesCount = activeW,
                    ExpiringWarrantiesCount = expiringW,
                    OverdueMaintenanceCount = overdueM,
                    TotalSpending = spending,
                    AverageHealthScore = avgHealth,
                    HealthRating = rating,
                    HealthBadgeClass = badge,
                    IsCurrentActiveHome = h.Id == activeHomeId
                });
            }

            var model = new HomeListViewModel
            {
                Homes = cards
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Overview(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessHomeAsync(id, userId.Value))
            {
                return Forbid();
            }

            var home = await _context.Homes
                .Include(h => h.Appliances)
                .ThenInclude(a => a.Category)
                .Include(h => h.Appliances)
                .ThenInclude(a => a.Warranty)
                .Include(h => h.Appliances)
                .ThenInclude(a => a.ServiceRecords)
                .Include(h => h.Appliances)
                .ThenInclude(a => a.Reminders)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (home == null) return NotFound();

            var now = DateTime.UtcNow;
            var appliances = home.Appliances.ToList();

            int activeW = appliances.Count(a => a.Warranty != null && a.Warranty.EndDate >= now);
            int expiringW = appliances.Count(a => a.Warranty != null && a.Warranty.EndDate >= now && (a.Warranty.EndDate - now).TotalDays <= 30);
            int overdueM = appliances.SelectMany(a => a.Reminders).Count(r => !r.IsCompleted && r.ReminderDate < now);

            var allServices = appliances.SelectMany(a => a.ServiceRecords).ToList();
            decimal totalSpending = allServices.Sum(s => s.Cost ?? 0);
            decimal currentMonthSpending = allServices
                .Where(s => s.ServiceDate.Month == now.Month && s.ServiceDate.Year == now.Year)
                .Sum(s => s.Cost ?? 0);

            // Most serviced appliance
            var mostServiced = appliances
                .OrderByDescending(a => a.ServiceRecords.Count)
                .FirstOrDefault(a => a.ServiceRecords.Count > 0);

            // Most expensive appliance
            var mostExpensive = appliances
                .OrderByDescending(a => a.PurchasePrice ?? 0)
                .FirstOrDefault(a => a.PurchasePrice.HasValue);

            int avgHealth = 100;
            string rating = "Good";
            string badge = "bg-primary";

            if (appliances.Count > 0)
            {
                var scores = appliances.Select(a => _healthScoreService.CalculateApplianceHealth(a).Score).ToList();
                avgHealth = (int)scores.Average();

                if (avgHealth >= 90) { rating = "Excellent"; badge = "bg-success"; }
                else if (avgHealth >= 70) { rating = "Good"; badge = "bg-primary"; }
                else if (avgHealth >= 40) { rating = "Needs Attention"; badge = "bg-warning text-dark"; }
                else { rating = "Critical"; badge = "bg-danger"; }
            }

            var appViewModels = new List<ApplianceItemViewModel>();
            foreach (var a in appliances)
            {
                var health = _healthScoreService.CalculateApplianceHealth(a);
                bool hasW = a.Warranty != null;
                string wStatus = "None";
                string wBadge = "bg-secondary";
                string wText = "No Warranty";

                if (hasW)
                {
                    var days = (a.Warranty!.EndDate - now).TotalDays;
                    if (days > 30) { wStatus = "Active"; wBadge = "bg-success"; wText = $"{Math.Ceiling(days)} days left"; }
                    else if (days >= 0) { wStatus = "Expiring Soon"; wBadge = "bg-warning text-dark"; wText = $"{Math.Ceiling(days)} days left"; }
                    else { wStatus = "Expired"; wBadge = "bg-danger"; wText = $"Expired"; }
                }

                appViewModels.Add(new ApplianceItemViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Brand = a.Brand,
                    CategoryName = a.Category?.Name ?? "Uncategorized",
                    CategoryIcon = a.Category?.Icon ?? "bi-plug",
                    ModelNumber = a.ModelNumber,
                    SerialNumber = a.SerialNumber,
                    PurchaseDate = a.PurchaseDate,
                    PurchasePrice = a.PurchasePrice,
                    Status = a.Status,
                    Location = a.Location,
                    ImagePath = a.ImagePath,
                    HomeName = home.Name,
                    HasWarranty = hasW,
                    WarrantyStatus = wStatus,
                    WarrantyBadgeClass = wBadge,
                    WarrantyCountdownText = wText,
                    HealthScore = health.Score,
                    HealthRating = health.Rating,
                    HealthBadgeClass = health.BadgeClass
                });
            }

            var model = new HomeOverviewViewModel
            {
                Home = home,
                TotalAppliances = appliances.Count,
                ActiveWarranties = activeW,
                ExpiringWarranties = expiringW,
                OverdueMaintenance = overdueM,
                TotalMaintenanceSpending = totalSpending,
                CurrentMonthSpending = currentMonthSpending,
                MostServicedApplianceName = mostServiced?.Name,
                MostServicedCount = mostServiced?.ServiceRecords.Count ?? 0,
                MostExpensiveApplianceName = mostExpensive?.Name,
                MostExpensivePrice = mostExpensive?.PurchasePrice,
                AverageHealthScore = avgHealth,
                HealthRating = rating,
                HealthBadgeClass = badge,
                Appliances = appViewModels
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new HomeFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HomeFormViewModel model)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var home = new Home
            {
                Name = model.Name.Trim(),
                Address = model.Address?.Trim(),
                City = model.City?.Trim(),
                State = model.State?.Trim(),
                ZipCode = model.ZipCode?.Trim(),
                HomeType = model.HomeType,
                UserId = userId.Value,
                CreatedAt = DateTime.UtcNow
            };

            _context.Homes.Add(home);
            await _context.SaveChangesAsync();

            // Set as active home
            _userContext.ActiveHomeId = home.Id;

            TempData["SuccessMessage"] = $"Home '{home.Name}' added successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessHomeAsync(id, userId.Value))
            {
                return Forbid();
            }

            var home = await _context.Homes.FindAsync(id);
            if (home == null) return NotFound();

            var model = new HomeFormViewModel
            {
                Id = home.Id,
                Name = home.Name,
                Address = home.Address,
                City = home.City,
                State = home.State,
                ZipCode = home.ZipCode,
                HomeType = home.HomeType
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HomeFormViewModel model)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessHomeAsync(model.Id, userId.Value))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var home = await _context.Homes.FindAsync(model.Id);
            if (home == null) return NotFound();

            home.Name = model.Name.Trim();
            home.Address = model.Address?.Trim();
            home.City = model.City?.Trim();
            home.State = model.State?.Trim();
            home.ZipCode = model.ZipCode?.Trim();
            home.HomeType = model.HomeType;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Home '{home.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessHomeAsync(id, userId.Value))
            {
                return Forbid();
            }

            var userHomesCount = await _context.Homes.CountAsync(h => h.UserId == userId.Value);
            if (userHomesCount <= 1)
            {
                TempData["ErrorMessage"] = "You must have at least one home in your profile.";
                return RedirectToAction(nameof(Index));
            }

            var home = await _context.Homes.FindAsync(id);
            if (home != null)
            {
                var name = home.Name;
                _context.Homes.Remove(home);
                await _context.SaveChangesAsync();

                if (_userContext.ActiveHomeId == id)
                {
                    var nextHome = await _context.Homes.FirstOrDefaultAsync(h => h.UserId == userId.Value);
                    _userContext.ActiveHomeId = nextHome?.Id;
                }

                TempData["SuccessMessage"] = $"Home '{name}' and all its appliances have been deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetActive(int id)
        {
            _userContext.ActiveHomeId = id;
            TempData["SuccessMessage"] = "Active home switched.";
            return RedirectToAction(nameof(Index));
        }
    }
}
