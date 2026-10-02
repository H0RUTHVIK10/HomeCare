using HomeCare.Data;
using HomeCare.Services;
using HomeCare.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HomeCare.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly HomeCareDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IHealthScoreService _healthScoreService;
        private readonly INotificationService _notificationService;

        public DashboardController(
            HomeCareDbContext context,
            IUserContext userContext,
            IHealthScoreService healthScoreService,
            INotificationService notificationService)
        {
            _context = context;
            _userContext = userContext;
            _healthScoreService = healthScoreService;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? homeId)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            // Scan and generate automated reminders and notifications for the user
            await _notificationService.CheckAndGenerateRemindersAndNotificationsAsync(userId.Value);

            // Fetch user's homes
            var userHomes = await _context.Homes
                .Where(h => h.UserId == userId.Value)
                .OrderBy(h => h.Name)
                .ToListAsync();

            if (userHomes.Count == 0)
            {
                // Create a default home if user has none
                var defaultHome = new Models.Home
                {
                    Name = "My Residence",
                    UserId = userId.Value,
                    HomeType = "Primary Residence"
                };
                _context.Homes.Add(defaultHome);
                await _context.SaveChangesAsync();
                userHomes.Add(defaultHome);
            }

            // Determine active home
            if (homeId.HasValue && userHomes.Any(h => h.Id == homeId.Value))
            {
                _userContext.ActiveHomeId = homeId.Value;
            }

            var activeHomeId = _userContext.ActiveHomeId;
            var activeHome = userHomes.FirstOrDefault(h => h.Id == activeHomeId);

            var viewModel = new DashboardViewModel
            {
                ActiveHomeId = activeHome?.Id,
                ActiveHomeName = activeHome != null ? activeHome.Name : "All Homes",
                TotalHomes = userHomes.Count
            };

            // Build homes dropdown
            viewModel.HomesList.Add(new SelectListItem("All Homes", ""));
            foreach (var h in userHomes)
            {
                viewModel.HomesList.Add(new SelectListItem(h.Name, h.Id.ToString(), h.Id == activeHome?.Id));
            }

            // Build query for appliances based on active home filter
            var applianceQuery = _context.Appliances
                .Include(a => a.Home)
                .Include(a => a.Category)
                .Include(a => a.Warranty)
                .Include(a => a.ServiceRecords)
                .Include(a => a.Documents)
                .Include(a => a.Reminders)
                .Where(a => a.Home.UserId == userId.Value);

            if (activeHome != null)
            {
                applianceQuery = applianceQuery.Where(a => a.HomeId == activeHome.Id);
            }

            var appliances = await applianceQuery.ToListAsync();
            viewModel.TotalAppliances = appliances.Count;

            var now = DateTime.UtcNow;

            // Calculate Warranties Metrics
            int activeWarranties = 0;
            int expiringWarranties = 0;

            foreach (var app in appliances)
            {
                if (app.Warranty != null)
                {
                    var days = (app.Warranty.EndDate - now).TotalDays;
                    if (days > 30)
                    {
                        activeWarranties++;
                    }
                    else if (days >= 0 && days <= 30)
                    {
                        expiringWarranties++;
                        viewModel.WarrantyAlerts.Add(new WarrantyAlertItem
                        {
                            ApplianceId = app.Id,
                            ApplianceName = app.Name,
                            Brand = app.Brand,
                            HomeName = app.Home.Name,
                            Provider = app.Warranty.Provider,
                            EndDate = app.Warranty.EndDate,
                            DaysRemaining = (int)Math.Ceiling(days),
                            Status = "Expiring Soon",
                            StatusBadgeClass = "bg-warning text-dark",
                            CountdownText = days == 0 ? "Expires today!" : $"Expires in {(int)Math.Ceiling(days)} day(s)"
                        });
                    }
                    else
                    {
                        viewModel.WarrantyAlerts.Add(new WarrantyAlertItem
                        {
                            ApplianceId = app.Id,
                            ApplianceName = app.Name,
                            Brand = app.Brand,
                            HomeName = app.Home.Name,
                            Provider = app.Warranty.Provider,
                            EndDate = app.Warranty.EndDate,
                            DaysRemaining = (int)Math.Floor(days),
                            Status = "Expired",
                            StatusBadgeClass = "bg-danger",
                            CountdownText = $"Expired {Math.Abs((int)Math.Floor(days))} day(s) ago"
                        });
                    }
                }
            }

            viewModel.ActiveWarranties = activeWarranties;
            viewModel.ExpiringWarranties = expiringWarranties;

            // Total Maintenance Spending
            var allServices = appliances.SelectMany(a => a.ServiceRecords).ToList();
            viewModel.TotalMaintenanceSpending = allServices.Sum(s => s.Cost ?? 0);

            // Upcoming Maintenance Items (from Reminders & ServiceRecords)
            var upcomingItems = new List<UpcomingMaintenanceItem>();

            // Reminders
            foreach (var app in appliances)
            {
                foreach (var rem in app.Reminders.Where(r => !r.IsCompleted))
                {
                    var diffDays = (rem.ReminderDate.Date - now.Date).TotalDays;
                    string status = "Upcoming";
                    string badge = "bg-primary";

                    if (diffDays < 0)
                    {
                        status = "Overdue";
                        badge = "bg-danger";
                    }
                    else if (diffDays == 0)
                    {
                        status = "Due Today";
                        badge = "bg-warning text-dark";
                    }

                    upcomingItems.Add(new UpcomingMaintenanceItem
                    {
                        ApplianceId = app.Id,
                        ApplianceName = app.Name,
                        Brand = app.Brand,
                        HomeName = app.Home.Name,
                        ServiceType = rem.ReminderType + ": " + rem.Title,
                        DueDate = rem.ReminderDate,
                        DaysRemaining = (int)diffDays,
                        Status = status,
                        StatusBadgeClass = badge,
                        ReminderId = rem.Id
                    });
                }

                // Next scheduled services from ServiceRecords
                foreach (var s in app.ServiceRecords.Where(sr => sr.NextServiceDate.HasValue && sr.ServiceStatus != "Completed"))
                {
                    var diffDays = (s.NextServiceDate!.Value.Date - now.Date).TotalDays;
                    string status = "Upcoming";
                    string badge = "bg-info text-dark";

                    if (diffDays < 0)
                    {
                        status = "Overdue";
                        badge = "bg-danger";
                    }
                    else if (diffDays == 0)
                    {
                        status = "Due Today";
                        badge = "bg-warning text-dark";
                    }

                    upcomingItems.Add(new UpcomingMaintenanceItem
                    {
                        ApplianceId = app.Id,
                        ApplianceName = app.Name,
                        Brand = app.Brand,
                        HomeName = app.Home.Name,
                        ServiceType = s.ServiceType,
                        DueDate = s.NextServiceDate.Value,
                        DaysRemaining = (int)diffDays,
                        Status = status,
                        StatusBadgeClass = badge,
                        ServiceRecordId = s.Id
                    });
                }
            }

            viewModel.UpcomingServices = upcomingItems.Count(i => i.Status == "Upcoming" || i.Status == "Due Today");
            viewModel.OverdueServices = upcomingItems.Count(i => i.Status == "Overdue");
            viewModel.UpcomingMaintenance = upcomingItems.OrderBy(i => i.DueDate).Take(10).ToList();

            // Recent Activities
            var activities = new List<RecentActivityItem>();

            foreach (var app in appliances)
            {
                activities.Add(new RecentActivityItem
                {
                    Title = "Appliance Registered",
                    Description = $"{app.Name} ({app.Brand}) was added to {app.Home.Name}.",
                    Timestamp = app.CreatedAt,
                    Icon = "bi-plus-circle",
                    BadgeClass = "bg-success",
                    LinkUrl = $"/Appliance/Details/{app.Id}"
                });

                if (app.Warranty != null)
                {
                    activities.Add(new RecentActivityItem
                    {
                        Title = "Warranty Logged",
                        Description = $"Warranty with {app.Warranty.Provider ?? "manufacturer"} recorded for {app.Name}.",
                        Timestamp = app.Warranty.StartDate,
                        Icon = "bi-shield-check",
                        BadgeClass = "bg-primary",
                        LinkUrl = $"/Appliance/Details/{app.Id}"
                    });
                }

                foreach (var s in app.ServiceRecords)
                {
                    activities.Add(new RecentActivityItem
                    {
                        Title = "Service Completed",
                        Description = $"{s.ServiceType} performed on {app.Name}" + (s.Cost.HasValue ? $" (₹{s.Cost:N0})" : "."),
                        Timestamp = s.ServiceDate,
                        Icon = "bi-wrench-adjustable",
                        BadgeClass = "bg-info text-dark",
                        LinkUrl = $"/Appliance/Details/{app.Id}"
                    });
                }

                foreach (var doc in app.Documents)
                {
                    activities.Add(new RecentActivityItem
                    {
                        Title = "Document Uploaded",
                        Description = $"{doc.DocumentType} '{doc.DocumentName}' uploaded for {app.Name}.",
                        Timestamp = doc.UploadedDate,
                        Icon = "bi-file-earmark-arrow-up",
                        BadgeClass = "bg-secondary",
                        LinkUrl = $"/Appliance/Details/{app.Id}"
                    });
                }
            }

            foreach (var act in activities)
            {
                var diff = now - act.Timestamp;
                if (diff.TotalMinutes < 60) act.TimeAgo = $"{Math.Max(1, (int)diff.TotalMinutes)}m ago";
                else if (diff.TotalHours < 24) act.TimeAgo = $"{(int)diff.TotalHours}h ago";
                else act.TimeAgo = $"{(int)diff.TotalDays}d ago";
            }

            viewModel.RecentActivities = activities.OrderByDescending(a => a.Timestamp).Take(8).ToList();

            // Average Health Score & Smart Insights
            if (appliances.Count > 0)
            {
                var scores = appliances.Select(a => _healthScoreService.CalculateApplianceHealth(a).Score).ToList();
                viewModel.AverageHealthScore = (int)scores.Average();

                if (viewModel.AverageHealthScore >= 90)
                {
                    viewModel.HealthRating = "Excellent";
                    viewModel.HealthBadgeClass = "bg-success";
                }
                else if (viewModel.AverageHealthScore >= 70)
                {
                    viewModel.HealthRating = "Good";
                    viewModel.HealthBadgeClass = "bg-primary";
                }
                else if (viewModel.AverageHealthScore >= 40)
                {
                    viewModel.HealthRating = "Needs Attention";
                    viewModel.HealthBadgeClass = "bg-warning text-dark";
                }
                else
                {
                    viewModel.HealthRating = "Critical";
                    viewModel.HealthBadgeClass = "bg-danger";
                }
            }
            else
            {
                viewModel.AverageHealthScore = 100;
                viewModel.HealthRating = "New Profile";
                viewModel.HealthBadgeClass = "bg-info text-dark";
            }

            viewModel.SmartInsights = _healthScoreService.GenerateSystemInsights(appliances);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SwitchHome(int? homeId)
        {
            _userContext.ActiveHomeId = homeId;
            return RedirectToAction(nameof(Index), new { homeId });
        }
    }
}
