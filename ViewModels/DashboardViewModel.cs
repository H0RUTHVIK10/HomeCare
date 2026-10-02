using Microsoft.AspNetCore.Mvc.Rendering;

namespace HomeCare.ViewModels
{
    public class DashboardViewModel
    {
        public int? ActiveHomeId { get; set; }
        public string ActiveHomeName { get; set; } = "All Homes";
        public List<SelectListItem> HomesList { get; set; } = new();

        // Summary Cards
        public int TotalHomes { get; set; }
        public int TotalAppliances { get; set; }
        public int ActiveWarranties { get; set; }
        public int ExpiringWarranties { get; set; }
        public int UpcomingServices { get; set; }
        public int OverdueServices { get; set; }
        public decimal TotalMaintenanceSpending { get; set; }

        // Household Health
        public int AverageHealthScore { get; set; }
        public string HealthRating { get; set; } = "Good";
        public string HealthBadgeClass { get; set; } = "bg-primary";

        // Lists
        public List<UpcomingMaintenanceItem> UpcomingMaintenance { get; set; } = new();
        public List<WarrantyAlertItem> WarrantyAlerts { get; set; } = new();
        public List<RecentActivityItem> RecentActivities { get; set; } = new();
        public List<string> SmartInsights { get; set; } = new();
    }

    public class UpcomingMaintenanceItem
    {
        public int ApplianceId { get; set; }
        public string ApplianceName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string HomeName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DaysRemaining { get; set; }
        public string Status { get; set; } = "Upcoming"; // Upcoming, Due Today, Overdue, Completed
        public string StatusBadgeClass { get; set; } = "bg-primary";
        public int? ReminderId { get; set; }
        public int? ServiceRecordId { get; set; }
    }

    public class WarrantyAlertItem
    {
        public int ApplianceId { get; set; }
        public string ApplianceName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string HomeName { get; set; } = string.Empty;
        public string? Provider { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysRemaining { get; set; }
        public string Status { get; set; } = "Active"; // Active, Expiring Soon, Expired
        public string StatusBadgeClass { get; set; } = "bg-success"; // Green, Yellow, Red
        public string CountdownText { get; set; } = string.Empty;
    }

    public class RecentActivityItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
        public string Icon { get; set; } = "bi-bell";
        public string BadgeClass { get; set; } = "bg-info";
        public string? LinkUrl { get; set; }
    }
}
