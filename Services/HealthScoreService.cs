using HomeCare.Models;

namespace HomeCare.Services
{
    public class HealthScoreService : IHealthScoreService
    {
        public HealthScoreResult CalculateApplianceHealth(Appliance appliance)
        {
            var result = new HealthScoreResult();
            var factors = new List<HealthScoreFactor>();
            var insights = new List<string>();

            int currentScore = 100;
            var now = DateTime.UtcNow;

            // 1. Age Calculation
            var purchaseDate = appliance.PurchaseDate ?? appliance.CreatedAt;
            var ageYears = (now - purchaseDate).TotalDays / 365.25;

            if (ageYears <= 1.0)
            {
                factors.Add(new HealthScoreFactor
                {
                    Title = "Appliance Age",
                    Impact = 0,
                    Description = "Brand new or under 1 year old.",
                    IsPositive = true
                });
            }
            else if (ageYears <= 3.0)
            {
                currentScore -= 5;
                factors.Add(new HealthScoreFactor
                {
                    Title = "Appliance Age",
                    Impact = -5,
                    Description = $"{ageYears:F1} years old (Moderate age).",
                    IsPositive = false
                });
            }
            else if (ageYears <= 5.0)
            {
                currentScore -= 10;
                factors.Add(new HealthScoreFactor
                {
                    Title = "Appliance Age",
                    Impact = -10,
                    Description = $"{ageYears:F1} years old (Mature life stage).",
                    IsPositive = false
                });
            }
            else if (ageYears <= 8.0)
            {
                currentScore -= 20;
                factors.Add(new HealthScoreFactor
                {
                    Title = "Appliance Age",
                    Impact = -20,
                    Description = $"{ageYears:F1} years old (Aging hardware).",
                    IsPositive = false
                });
                insights.Add("This appliance is over 5 years old. Inspect seals, hoses, and electrical connectors regularly.");
            }
            else
            {
                currentScore -= 30;
                factors.Add(new HealthScoreFactor
                {
                    Title = "Appliance Age",
                    Impact = -30,
                    Description = $"{ageYears:F1} years old (High operational wear).",
                    IsPositive = false
                });
                insights.Add("High operating age. Consider budgeting for an energy-efficient replacement or comprehensive overhaul.");
            }

            // 2. Warranty Status
            if (appliance.Warranty != null)
            {
                if (appliance.Warranty.EndDate >= now)
                {
                    var daysRemaining = (appliance.Warranty.EndDate - now).TotalDays;
                    if (daysRemaining <= 30)
                    {
                        currentScore -= 5;
                        factors.Add(new HealthScoreFactor
                        {
                            Title = "Warranty Expiring Soon",
                            Impact = -5,
                            Description = $"Warranty expires in {Math.Ceiling(daysRemaining)} days.",
                            IsPositive = false
                        });
                        insights.Add("Warranty expires soon. Keep your invoice and warranty card ready in case check-up is needed.");
                    }
                    else
                    {
                        currentScore += 5;
                        factors.Add(new HealthScoreFactor
                        {
                            Title = "Active Warranty",
                            Impact = +5,
                            Description = $"Protected by {appliance.Warranty.Provider ?? "manufacturer"} warranty ({Math.Ceiling(daysRemaining)} days remaining).",
                            IsPositive = true
                        });
                    }
                }
                else
                {
                    currentScore -= 10;
                    var daysAgo = (now - appliance.Warranty.EndDate).TotalDays;
                    factors.Add(new HealthScoreFactor
                    {
                        Title = "Warranty Expired",
                        Impact = -10,
                        Description = $"Warranty expired {Math.Floor(daysAgo)} days ago.",
                        IsPositive = false
                    });
                    insights.Add("Manufacturer warranty has expired. Regular preventive maintenance is recommended to avoid out-of-pocket repair costs.");
                }
            }
            else
            {
                currentScore -= 5;
                factors.Add(new HealthScoreFactor
                {
                    Title = "No Warranty Registered",
                    Impact = -5,
                    Description = "No active warranty record found.",
                    IsPositive = false
                });
                insights.Add("No warranty record is tracked. Add purchase details or invoice to verify coverage.");
            }

            // 3. Maintenance Regularity & Recent Services
            var services = appliance.ServiceRecords?.ToList() ?? new List<ServiceRecord>();
            var recentServices = services.Where(s => (now - s.ServiceDate).TotalDays <= 365).ToList();
            var recentRepairs = recentServices.Where(s =>
                s.ServiceType.Contains("Repair", StringComparison.OrdinalIgnoreCase) ||
                (s.Description != null && s.Description.Contains("Repair", StringComparison.OrdinalIgnoreCase))).ToList();

            if (recentServices.Count > 0)
            {
                var latestService = services.OrderByDescending(s => s.ServiceDate).First();
                var daysSinceService = (now - latestService.ServiceDate).TotalDays;

                if (daysSinceService <= 180)
                {
                    currentScore += 10;
                    factors.Add(new HealthScoreFactor
                    {
                        Title = "Recent Preventive Maintenance",
                        Impact = +10,
                        Description = $"Serviced within last 6 months ({latestService.ServiceDate:MMM dd, yyyy}).",
                        IsPositive = true
                    });
                }
                else
                {
                    factors.Add(new HealthScoreFactor
                    {
                        Title = "Annual Service Record",
                        Impact = 0,
                        Description = $"Last serviced on {latestService.ServiceDate:MMM dd, yyyy}.",
                        IsPositive = true
                    });
                }
            }
            else if (ageYears > 1.0)
            {
                currentScore -= 15;
                factors.Add(new HealthScoreFactor
                {
                    Title = "No Service Records",
                    Impact = -15,
                    Description = "Appliance has had no documented maintenance since purchase.",
                    IsPositive = false
                });
                insights.Add("This appliance has not been serviced recently. Schedule a routine maintenance check to maximize longevity.");
            }

            // 4. Frequent Repairs Check
            if (recentRepairs.Count >= 2)
            {
                currentScore -= 20;
                factors.Add(new HealthScoreFactor
                {
                    Title = "Frequent Repairs",
                    Impact = -20,
                    Description = $"{recentRepairs.Count} repairs recorded in the last 12 months.",
                    IsPositive = false
                });
                insights.Add("This appliance has multiple repair records this year. Frequent breakdowns indicate recurring component stress.");
            }
            else if (recentRepairs.Count == 1)
            {
                currentScore -= 10;
                factors.Add(new HealthScoreFactor
                {
                    Title = "Recent Breakdown/Repair",
                    Impact = -10,
                    Description = "1 repair recorded in the last 12 months.",
                    IsPositive = false
                });
            }

            // 5. Overdue Maintenance / Reminders
            var reminders = appliance.Reminders?.ToList() ?? new List<Reminder>();
            var overdueReminders = reminders.Where(r => !r.IsCompleted && r.ReminderDate < now).ToList();
            if (overdueReminders.Count > 0)
            {
                int penalty = overdueReminders.Count * 15;
                currentScore -= penalty;
                factors.Add(new HealthScoreFactor
                {
                    Title = "Overdue Maintenance Reminders",
                    Impact = -penalty,
                    Description = $"{overdueReminders.Count} reminder(s) past due date.",
                    IsPositive = false
                });
                insights.Add($"You have {overdueReminders.Count} overdue maintenance task(s) for this appliance. Complete them promptly.");
            }

            // Check if upcoming next service date on any record is overdue
            var overdueNextServices = services.Where(s => s.NextServiceDate.HasValue && s.NextServiceDate.Value < now && s.ServiceStatus != "Completed").ToList();
            if (overdueNextServices.Count > 0)
            {
                currentScore -= 10;
                factors.Add(new HealthScoreFactor
                {
                    Title = "Scheduled Service Overdue",
                    Impact = -10,
                    Description = "Next scheduled service date has passed.",
                    IsPositive = false
                });
                insights.Add("Scheduled service date has passed without recorded completion.");
            }

            // 6. Documents Check
            var documents = appliance.Documents?.ToList() ?? new List<Document>();
            if (documents.Count == 0)
            {
                insights.Add("No invoice or warranty document uploaded yet. Store digital copies to avoid misplacing physical receipts.");
            }

            // Clamp score between 0 and 100
            currentScore = Math.Clamp(currentScore, 0, 100);
            result.Score = currentScore;
            result.Factors = factors;
            result.Insights = insights;

            // Rating & Classes
            if (currentScore >= 90)
            {
                result.Rating = "Excellent";
                result.BadgeClass = "bg-success";
                result.TextColorClass = "text-success";
                result.BorderColorClass = "border-success";
            }
            else if (currentScore >= 70)
            {
                result.Rating = "Good";
                result.BadgeClass = "bg-primary";
                result.TextColorClass = "text-primary";
                result.BorderColorClass = "border-primary";
            }
            else if (currentScore >= 40)
            {
                result.Rating = "Needs Attention";
                result.BadgeClass = "bg-warning text-dark";
                result.TextColorClass = "text-warning";
                result.BorderColorClass = "border-warning";
            }
            else
            {
                result.Rating = "Critical";
                result.BadgeClass = "bg-danger";
                result.TextColorClass = "text-danger";
                result.BorderColorClass = "border-danger";
            }

            return result;
        }

        public List<string> GenerateSystemInsights(IEnumerable<Appliance> appliances)
        {
            var insights = new List<string>();
            var applianceList = appliances.ToList();
            var now = DateTime.UtcNow;

            if (applianceList.Count == 0)
            {
                insights.Add("Add your first appliance to start tracking warranties, scheduled services, and health scores.");
                return insights;
            }

            // Expiring warranties
            var expiringWarranties = applianceList
                .Where(a => a.Warranty != null && a.Warranty.EndDate >= now && (a.Warranty.EndDate - now).TotalDays <= 30)
                .ToList();

            if (expiringWarranties.Count > 0)
            {
                insights.Add($"{expiringWarranties.Count} appliance warranty(ies) will expire within 30 days. Review coverage and consider renewals.");
            }

            // Overdue maintenance
            var overdueTasks = applianceList
                .SelectMany(a => a.Reminders ?? new List<Reminder>())
                .Count(r => !r.IsCompleted && r.ReminderDate < now);

            if (overdueTasks > 0)
            {
                insights.Add($"There are {overdueTasks} overdue maintenance reminder(s) pending across your household.");
            }

            // Total spending trend
            var allServices = applianceList.SelectMany(a => a.ServiceRecords ?? new List<ServiceRecord>()).ToList();
            var currentYearCost = allServices.Where(s => s.ServiceDate.Year == now.Year).Sum(s => s.Cost ?? 0);
            if (currentYearCost > 0)
            {
                insights.Add($"Total maintenance expenditure recorded for {now.Year} is ₹{currentYearCost:N2}.");
            }

            // Needs attention appliances
            int needsAttentionCount = 0;
            foreach (var app in applianceList)
            {
                var health = CalculateApplianceHealth(app);
                if (health.Score < 70)
                {
                    needsAttentionCount++;
                }
            }

            if (needsAttentionCount > 0)
            {
                insights.Add($"{needsAttentionCount} appliance(s) have health scores below 70 and require preventive attention.");
            }

            return insights;
        }
    }
}
