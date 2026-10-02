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
    public class AnalyticsController : Controller
    {
        private readonly HomeCareDbContext _context;
        private readonly IUserContext _userContext;

        public AnalyticsController(HomeCareDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? homeId)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var userHomes = await _context.Homes
                .Where(h => h.UserId == userId.Value)
                .OrderBy(h => h.Name)
                .ToListAsync();

            var activeHome = homeId.HasValue ? userHomes.FirstOrDefault(h => h.Id == homeId.Value) : null;

            var query = _context.Appliances
                .Include(a => a.Home)
                .Include(a => a.Category)
                .Include(a => a.Warranty)
                .Include(a => a.ServiceRecords)
                .Where(a => a.Home.UserId == userId.Value);

            if (activeHome != null)
            {
                query = query.Where(a => a.HomeId == activeHome.Id);
            }

            var appliances = await query.ToListAsync();
            var allServices = appliances.SelectMany(a => a.ServiceRecords).ToList();

            var viewModel = new AnalyticsViewModel
            {
                SelectedHomeId = activeHome?.Id,
                SelectedHomeName = activeHome != null ? activeHome.Name : "All Homes",
                TotalAppliancesCount = appliances.Count,
                TotalServicesCount = allServices.Count,
                TotalSpending = allServices.Sum(s => s.Cost ?? 0),
                TotalRepairsCount = allServices.Count(s => s.ServiceType.Contains("Repair", StringComparison.OrdinalIgnoreCase))
            };

            viewModel.AverageCostPerService = viewModel.TotalServicesCount > 0
                ? viewModel.TotalSpending / viewModel.TotalServicesCount
                : 0;

            // Homes dropdown
            viewModel.HomesList.Add(new SelectListItem("All Homes", ""));
            foreach (var h in userHomes)
            {
                viewModel.HomesList.Add(new SelectListItem(h.Name, h.Id.ToString(), h.Id == activeHome?.Id));
            }

            // 1. Category Breakdown
            var categoryGroups = appliances
                .GroupBy(a => a.Category?.Name ?? "Uncategorized")
                .OrderByDescending(g => g.Count())
                .ToList();

            viewModel.CategoryLabels = categoryGroups.Select(g => g.Key).ToList();
            viewModel.CategoryCounts = categoryGroups.Select(g => g.Count()).ToList();

            // 2. Monthly Maintenance Cost (Last 12 Months)
            var now = DateTime.UtcNow;
            for (int i = 11; i >= 0; i--)
            {
                var monthDate = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                var monthLabel = monthDate.ToString("MMM yyyy");
                var cost = allServices
                    .Where(s => s.ServiceDate.Year == monthDate.Year && s.ServiceDate.Month == monthDate.Month)
                    .Sum(s => s.Cost ?? 0);

                viewModel.MonthlyCostLabels.Add(monthLabel);
                viewModel.MonthlyCostValues.Add(cost);
            }

            // 3. Top Appliances by Maintenance Cost
            var applianceCosts = appliances
                .Select(a => new
                {
                    Name = $"{a.Name} ({a.Brand})",
                    TotalCost = a.ServiceRecords.Sum(s => s.Cost ?? 0)
                })
                .Where(x => x.TotalCost > 0)
                .OrderByDescending(x => x.TotalCost)
                .Take(7)
                .ToList();

            viewModel.ApplianceCostLabels = applianceCosts.Select(x => x.Name).ToList();
            viewModel.ApplianceCostValues = applianceCosts.Select(x => x.TotalCost).ToList();

            // 4. Warranty Status Breakdown
            int activeW = 0;
            int expiringW = 0;
            int expiredW = 0;
            int noW = 0;

            foreach (var a in appliances)
            {
                if (a.Warranty == null)
                {
                    noW++;
                }
                else
                {
                    var days = (a.Warranty.EndDate - now).TotalDays;
                    if (days > 30) activeW++;
                    else if (days >= 0) expiringW++;
                    else expiredW++;
                }
            }

            viewModel.WarrantyStatusLabels = new List<string> { "Active (>30d)", "Expiring (<=30d)", "Expired", "No Warranty" };
            viewModel.WarrantyStatusCounts = new List<int> { activeW, expiringW, expiredW, noW };

            // 5. Age Distribution
            int ageUnder1 = 0, age1to3 = 0, age3to5 = 0, age5Plus = 0;
            foreach (var a in appliances)
            {
                var pDate = a.PurchaseDate ?? a.CreatedAt;
                var ageYears = (now - pDate).TotalDays / 365.25;
                if (ageYears <= 1.0) ageUnder1++;
                else if (ageYears <= 3.0) age1to3++;
                else if (ageYears <= 5.0) age3to5++;
                else age5Plus++;
            }

            viewModel.AgeLabels = new List<string> { "< 1 Year", "1 - 3 Years", "3 - 5 Years", "> 5 Years" };
            viewModel.AgeCounts = new List<int> { ageUnder1, age1to3, age3to5, age5Plus };

            // 6. Service Type Breakdown
            var serviceTypeGroups = allServices
                .GroupBy(s => s.ServiceType)
                .OrderByDescending(g => g.Count())
                .Take(6)
                .ToList();

            viewModel.ServiceTypeLabels = serviceTypeGroups.Select(g => g.Key).ToList();
            viewModel.ServiceTypeCounts = serviceTypeGroups.Select(g => g.Count()).ToList();

            return View(viewModel);
        }
    }
}
