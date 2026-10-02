using HomeCare.Data;
using HomeCare.Models;
using HomeCare.Services;
using HomeCare.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HomeCare.Controllers
{
    [Authorize]
    public class ApplianceController : Controller
    {
        private readonly HomeCareDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IApplianceAuthorizationService _authService;
        private readonly IFileService _fileService;
        private readonly IQrCodeService _qrCodeService;
        private readonly IHealthScoreService _healthScoreService;

        public ApplianceController(
            HomeCareDbContext context,
            IUserContext userContext,
            IApplianceAuthorizationService authService,
            IFileService fileService,
            IQrCodeService qrCodeService,
            IHealthScoreService healthScoreService)
        {
            _context = context;
            _userContext = userContext;
            _authService = authService;
            _fileService = fileService;
            _qrCodeService = qrCodeService;
            _healthScoreService = healthScoreService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchTerm,
            int? homeId,
            int? categoryId,
            string? status,
            string? warrantyStatus)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var query = _context.Appliances
                .Include(a => a.Home)
                .Include(a => a.Category)
                .Include(a => a.Warranty)
                .Include(a => a.ServiceRecords)
                .Include(a => a.Reminders)
                .Where(a => a.Home.UserId == userId.Value);

            // Search Filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(a =>
                    a.Name.ToLower().Contains(term) ||
                    a.Brand.ToLower().Contains(term) ||
                    (a.ModelNumber != null && a.ModelNumber.ToLower().Contains(term)) ||
                    (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(term)) ||
                    a.Category.Name.ToLower().Contains(term) ||
                    a.Home.Name.ToLower().Contains(term));
            }

            // Home Filter
            if (homeId.HasValue && homeId.Value > 0)
            {
                query = query.Where(a => a.HomeId == homeId.Value);
            }

            // Category Filter
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(a => a.CategoryId == categoryId.Value);
            }

            // Status Filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status == status);
            }

            var appliances = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var items = new List<ApplianceItemViewModel>();

            foreach (var a in appliances)
            {
                var health = _healthScoreService.CalculateApplianceHealth(a);
                string wStatus = "None";
                string wBadge = "bg-secondary";
                string wText = "No Warranty";
                bool hasW = a.Warranty != null;

                if (hasW)
                {
                    var daysRemaining = (a.Warranty!.EndDate - now).TotalDays;
                    if (daysRemaining > 30)
                    {
                        wStatus = "Active";
                        wBadge = "bg-success";
                        wText = $"{Math.Ceiling(daysRemaining)} days left";
                    }
                    else if (daysRemaining >= 0)
                    {
                        wStatus = "Expiring Soon";
                        wBadge = "bg-warning text-dark";
                        wText = $"{Math.Ceiling(daysRemaining)} days left";
                    }
                    else
                    {
                        wStatus = "Expired";
                        wBadge = "bg-danger";
                        wText = $"Expired {Math.Abs(Math.Floor(daysRemaining))} days ago";
                    }
                }

                // Check warranty filter in memory if specified
                if (!string.IsNullOrWhiteSpace(warrantyStatus) && !string.Equals(wStatus, warrantyStatus, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var nextService = a.ServiceRecords
                    .Where(s => s.NextServiceDate.HasValue && s.ServiceStatus != "Completed")
                    .OrderBy(s => s.NextServiceDate)
                    .Select(s => s.NextServiceDate)
                    .FirstOrDefault();

                items.Add(new ApplianceItemViewModel
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
                    HomeName = a.Home.Name,
                    HasWarranty = hasW,
                    WarrantyStatus = wStatus,
                    WarrantyBadgeClass = wBadge,
                    WarrantyCountdownText = wText,
                    NextServiceDate = nextService,
                    HealthScore = health.Score,
                    HealthRating = health.Rating,
                    HealthBadgeClass = health.BadgeClass
                });
            }

            // Populate Dropdowns
            var userHomes = await _context.Homes
                .Where(h => h.UserId == userId.Value)
                .OrderBy(h => h.Name)
                .Select(h => new SelectListItem(h.Name, h.Id.ToString(), h.Id == homeId))
                .ToListAsync();

            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == categoryId))
                .ToListAsync();

            var viewModel = new ApplianceListViewModel
            {
                SearchTerm = searchTerm,
                SelectedHomeId = homeId,
                SelectedCategoryId = categoryId,
                SelectedStatus = status,
                SelectedWarrantyStatus = warrantyStatus,
                HomesList = userHomes,
                CategoriesList = categories,
                Appliances = items
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, string tab = "overview")
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            // Strict User Data Isolation Ownership Check
            if (!await _authService.CanAccessApplianceAsync(id, userId.Value))
            {
                return Forbid();
            }

            var appliance = await _context.Appliances
                .Include(a => a.Home)
                .Include(a => a.Category)
                .Include(a => a.Warranty)
                .Include(a => a.ServiceRecords)
                .Include(a => a.Documents)
                .Include(a => a.Reminders)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appliance == null) return NotFound();

            var healthResult = _healthScoreService.CalculateApplianceHealth(appliance);

            // Calculate Warranty Countdown
            var now = DateTime.UtcNow;
            string warrantyCountdown = "No warranty registered";
            string warrantyStatus = "None";
            string warrantyBadge = "bg-secondary";

            if (appliance.Warranty != null)
            {
                var daysRemaining = (appliance.Warranty.EndDate - now).TotalDays;
                if (daysRemaining > 30)
                {
                    warrantyStatus = "Active";
                    warrantyBadge = "bg-success";
                    var months = (int)(daysRemaining / 30.4);
                    warrantyCountdown = months > 0 ? $"{months} month(s) & {(int)(daysRemaining % 30.4)} day(s) remaining" : $"{(int)Math.Ceiling(daysRemaining)} days remaining";
                }
                else if (daysRemaining >= 0)
                {
                    warrantyStatus = "Expiring Soon";
                    warrantyBadge = "bg-warning text-dark";
                    warrantyCountdown = $"Expiring soon: {(int)Math.Ceiling(daysRemaining)} day(s) remaining";
                }
                else
                {
                    warrantyStatus = "Expired";
                    warrantyBadge = "bg-danger";
                    warrantyCountdown = $"Expired {Math.Abs((int)Math.Floor(daysRemaining))} day(s) ago";
                }
            }

            // Generate Timeline Items
            var timeline = new List<TimelineItem>();

            if (appliance.PurchaseDate.HasValue)
            {
                timeline.Add(new TimelineItem
                {
                    Date = appliance.PurchaseDate.Value,
                    Title = "Appliance Purchased",
                    Description = $"{appliance.Name} was acquired" + (appliance.PurchasePrice.HasValue ? $" for ₹{appliance.PurchasePrice:N2}" : "."),
                    Type = "Purchase",
                    IconClass = "bi-bag-check-fill",
                    BadgeClass = "bg-info text-dark",
                    Cost = appliance.PurchasePrice
                });
            }

            if (appliance.Warranty != null)
            {
                timeline.Add(new TimelineItem
                {
                    Date = appliance.Warranty.StartDate,
                    Title = "Warranty Activated",
                    Description = $"Coverage started by {appliance.Warranty.Provider ?? "manufacturer"}. Valid until {appliance.Warranty.EndDate:MMM dd, yyyy}.",
                    Type = "Warranty",
                    IconClass = "bi-shield-check",
                    BadgeClass = "bg-success"
                });
            }

            foreach (var sr in appliance.ServiceRecords.OrderBy(s => s.ServiceDate))
            {
                bool isRepair = sr.ServiceType.Contains("Repair", StringComparison.OrdinalIgnoreCase);
                timeline.Add(new TimelineItem
                {
                    Date = sr.ServiceDate,
                    Title = sr.ServiceType,
                    Description = (string.IsNullOrWhiteSpace(sr.Description) ? "Routine maintenance service." : sr.Description) +
                                  (!string.IsNullOrWhiteSpace(sr.TechnicianName) ? $" Technician: {sr.TechnicianName}" : ""),
                    Type = isRepair ? "Repair" : "Service",
                    IconClass = isRepair ? "bi-tools" : "bi-wrench-adjustable",
                    BadgeClass = isRepair ? "bg-warning text-dark" : "bg-primary",
                    Cost = sr.Cost
                });
            }

            foreach (var doc in appliance.Documents.OrderBy(d => d.UploadedDate))
            {
                timeline.Add(new TimelineItem
                {
                    Date = doc.UploadedDate,
                    Title = $"Document Added: {doc.DocumentType}",
                    Description = $"File: {doc.DocumentName}",
                    Type = "Document",
                    IconClass = "bi-file-earmark-arrow-up",
                    BadgeClass = "bg-secondary"
                });
            }

            var sortedTimeline = timeline.OrderByDescending(t => t.Date).ToList();

            // Calculate Maintenance Costs
            var totalMaintenance = appliance.ServiceRecords.Sum(s => s.Cost ?? 0);
            var totalRepairs = appliance.ServiceRecords
                .Where(s => s.ServiceType.Contains("Repair", StringComparison.OrdinalIgnoreCase))
                .Sum(s => s.Cost ?? 0);
            var serviceCount = appliance.ServiceRecords.Count;
            var avgCost = serviceCount > 0 ? totalMaintenance / serviceCount : 0;

            // Generate QR Code
            var request = HttpContext.Request;
            var applianceUrl = $"{request.Scheme}://{request.Host}/Appliance/Details/{appliance.Id}";
            var qrCodeBase64 = _qrCodeService.GenerateBase64QrCode(applianceUrl);

            var viewModel = new ApplianceDetailsViewModel
            {
                Appliance = appliance,
                HealthScore = healthResult,
                TimelineItems = sortedTimeline,
                WarrantyStatus = warrantyStatus,
                WarrantyBadgeClass = warrantyBadge,
                WarrantyCountdownText = warrantyCountdown,
                TotalMaintenanceCost = totalMaintenance,
                TotalRepairCost = totalRepairs,
                AverageServiceCost = avgCost,
                TotalServicesCount = serviceCount,
                QrCodeBase64 = qrCodeBase64,
                QrCodeUrl = applianceUrl,
                ActiveTab = tab
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? homeId)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var userHomes = await _context.Homes
                .Where(h => h.UserId == userId.Value)
                .OrderBy(h => h.Name)
                .ToListAsync();

            if (userHomes.Count == 0)
            {
                TempData["ErrorMessage"] = "Please create a home first before adding appliances.";
                return RedirectToAction("Create", "Homes");
            }

            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            var activeHomeId = homeId ?? _userContext.ActiveHomeId ?? userHomes.First().Id;

            var model = new ApplianceFormViewModel
            {
                HomeId = activeHomeId,
                HomesList = userHomes.Select(h => new SelectListItem(h.Name, h.Id.ToString(), h.Id == activeHomeId)).ToList(),
                CategoriesList = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList(),
                PurchaseDate = DateTime.Today
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplianceFormViewModel model)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            // Verify ownership of the target home
            if (!await _authService.CanAccessHomeAsync(model.HomeId, userId.Value))
            {
                ModelState.AddModelError(nameof(model.HomeId), "Invalid home selected.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model, userId.Value);
                return View(model);
            }

            string? imagePath = null;
            if (model.ImageFile != null)
            {
                var uploadResult = await _fileService.SaveImageAsync(model.ImageFile, "appliances");
                if (!uploadResult.Success)
                {
                    ModelState.AddModelError(nameof(model.ImageFile), uploadResult.ErrorMessage ?? "Image upload failed.");
                    await PopulateDropdownsAsync(model, userId.Value);
                    return View(model);
                }
                imagePath = uploadResult.FilePath;
            }

            var appliance = new Appliance
            {
                Name = model.Name.Trim(),
                Brand = model.Brand.Trim(),
                ModelNumber = model.ModelNumber?.Trim(),
                SerialNumber = model.SerialNumber?.Trim(),
                PurchaseDate = model.PurchaseDate,
                PurchasePrice = model.PurchasePrice,
                Notes = model.Notes?.Trim(),
                Status = string.IsNullOrWhiteSpace(model.Status) ? "Active" : model.Status,
                Location = model.Location?.Trim(),
                ImagePath = imagePath,
                HomeId = model.HomeId,
                CategoryId = model.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appliances.Add(appliance);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Appliance '{appliance.Name}' was successfully registered!";
            return RedirectToAction(nameof(Details), new { id = appliance.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessApplianceAsync(id, userId.Value))
            {
                return Forbid();
            }

            var appliance = await _context.Appliances.FindAsync(id);
            if (appliance == null) return NotFound();

            var model = new ApplianceFormViewModel
            {
                Id = appliance.Id,
                Name = appliance.Name,
                Brand = appliance.Brand,
                ModelNumber = appliance.ModelNumber,
                SerialNumber = appliance.SerialNumber,
                PurchaseDate = appliance.PurchaseDate,
                PurchasePrice = appliance.PurchasePrice,
                Notes = appliance.Notes,
                Status = appliance.Status,
                Location = appliance.Location,
                ExistingImagePath = appliance.ImagePath,
                HomeId = appliance.HomeId,
                CategoryId = appliance.CategoryId
            };

            await PopulateDropdownsAsync(model, userId.Value);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplianceFormViewModel model)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessApplianceAsync(model.Id, userId.Value))
            {
                return Forbid();
            }

            if (!await _authService.CanAccessHomeAsync(model.HomeId, userId.Value))
            {
                ModelState.AddModelError(nameof(model.HomeId), "Invalid home selected.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model, userId.Value);
                return View(model);
            }

            var appliance = await _context.Appliances.FindAsync(model.Id);
            if (appliance == null) return NotFound();

            if (model.ImageFile != null)
            {
                var uploadResult = await _fileService.SaveImageAsync(model.ImageFile, "appliances");
                if (!uploadResult.Success)
                {
                    ModelState.AddModelError(nameof(model.ImageFile), uploadResult.ErrorMessage ?? "Image upload failed.");
                    await PopulateDropdownsAsync(model, userId.Value);
                    return View(model);
                }

                // Delete old image if present
                if (!string.IsNullOrWhiteSpace(appliance.ImagePath))
                {
                    _fileService.DeleteFile(appliance.ImagePath);
                }
                appliance.ImagePath = uploadResult.FilePath;
            }

            appliance.Name = model.Name.Trim();
            appliance.Brand = model.Brand.Trim();
            appliance.ModelNumber = model.ModelNumber?.Trim();
            appliance.SerialNumber = model.SerialNumber?.Trim();
            appliance.PurchaseDate = model.PurchaseDate;
            appliance.PurchasePrice = model.PurchasePrice;
            appliance.Notes = model.Notes?.Trim();
            appliance.Status = model.Status;
            appliance.Location = model.Location?.Trim();
            appliance.HomeId = model.HomeId;
            appliance.CategoryId = model.CategoryId;
            appliance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Appliance '{appliance.Name}' details updated successfully.";
            return RedirectToAction(nameof(Details), new { id = appliance.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessApplianceAsync(id, userId.Value))
            {
                return Forbid();
            }

            var appliance = await _context.Appliances
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appliance != null)
            {
                // Delete physical files
                if (!string.IsNullOrWhiteSpace(appliance.ImagePath))
                {
                    _fileService.DeleteFile(appliance.ImagePath);
                }

                foreach (var doc in appliance.Documents)
                {
                    _fileService.DeleteFile(doc.FilePath);
                }

                var name = appliance.Name;
                _context.Appliances.Remove(appliance);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Appliance '{name}' and its related records have been deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> DownloadQrCode(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessApplianceAsync(id, userId.Value))
            {
                return Forbid();
            }

            var appliance = await _context.Appliances.FindAsync(id);
            if (appliance == null) return NotFound();

            var request = HttpContext.Request;
            var applianceUrl = $"{request.Scheme}://{request.Host}/Appliance/Details/{appliance.Id}";
            var pngBytes = _qrCodeService.GenerateQrCodePngBytes(applianceUrl);

            return File(pngBytes, "image/png", $"HomeCare_QR_{appliance.Name.Replace(" ", "_")}_{appliance.Id}.png");
        }

        private async Task PopulateDropdownsAsync(ApplianceFormViewModel model, int userId)
        {
            var userHomes = await _context.Homes
                .Where(h => h.UserId == userId)
                .OrderBy(h => h.Name)
                .ToListAsync();

            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            model.HomesList = userHomes.Select(h => new SelectListItem(h.Name, h.Id.ToString(), h.Id == model.HomeId)).ToList();
            model.CategoriesList = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.CategoryId)).ToList();
        }
    }
}
