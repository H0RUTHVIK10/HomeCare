using Microsoft.AspNetCore.Mvc.Rendering;

namespace HomeCare.ViewModels
{
    public class AnalyticsViewModel
    {
        public int? SelectedHomeId { get; set; }
        public string SelectedHomeName { get; set; } = "All Homes";
        public List<SelectListItem> HomesList { get; set; } = new();

        // Key KPI metrics
        public decimal TotalSpending { get; set; }
        public decimal AverageCostPerService { get; set; }
        public int TotalServicesCount { get; set; }
        public int TotalAppliancesCount { get; set; }
        public int TotalRepairsCount { get; set; }

        // Chart Data (JSON-serializable collections)
        public List<string> CategoryLabels { get; set; } = new();
        public List<int> CategoryCounts { get; set; } = new();

        public List<string> MonthlyCostLabels { get; set; } = new();
        public List<decimal> MonthlyCostValues { get; set; } = new();

        public List<string> ApplianceCostLabels { get; set; } = new();
        public List<decimal> ApplianceCostValues { get; set; } = new();

        public List<string> WarrantyStatusLabels { get; set; } = new();
        public List<int> WarrantyStatusCounts { get; set; } = new();

        public List<string> AgeLabels { get; set; } = new();
        public List<int> AgeCounts { get; set; } = new();

        public List<string> ServiceTypeLabels { get; set; } = new();
        public List<int> ServiceTypeCounts { get; set; } = new();
    }
}
