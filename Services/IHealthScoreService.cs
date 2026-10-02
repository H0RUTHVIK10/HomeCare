using HomeCare.Models;

namespace HomeCare.Services
{
    public class HealthScoreResult
    {
        public int Score { get; set; }
        public string Rating { get; set; } = "Good";
        public string BadgeClass { get; set; } = "bg-success";
        public string TextColorClass { get; set; } = "text-success";
        public string BorderColorClass { get; set; } = "border-success";
        public string Disclaimer { get; set; } = "HomeCare Maintenance Health Indicator (Rule-based guidance, not an engineering diagnosis)";
        public List<string> Insights { get; set; } = new();
        public List<HealthScoreFactor> Factors { get; set; } = new();
    }

    public class HealthScoreFactor
    {
        public string Title { get; set; } = string.Empty;
        public int Impact { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsPositive { get; set; }
    }

    public interface IHealthScoreService
    {
        HealthScoreResult CalculateApplianceHealth(Appliance appliance);
        List<string> GenerateSystemInsights(IEnumerable<Appliance> appliances);
    }
}
