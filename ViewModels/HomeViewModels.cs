using System.ComponentModel.DataAnnotations;
using HomeCare.Models;

namespace HomeCare.ViewModels
{
    public class HomeListViewModel
    {
        public List<HomeCardViewModel> Homes { get; set; } = new();
        public int TotalHomes => Homes.Count;
        public int TotalAppliances => Homes.Sum(h => h.ApplianceCount);
        public decimal TotalSpending => Homes.Sum(h => h.TotalSpending);
    }

    public class HomeCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string HomeType { get; set; } = "Primary Residence";
        public DateTime CreatedAt { get; set; }

        public int ApplianceCount { get; set; }
        public int ActiveWarrantiesCount { get; set; }
        public int ExpiringWarrantiesCount { get; set; }
        public int OverdueMaintenanceCount { get; set; }
        public decimal TotalSpending { get; set; }
        public int AverageHealthScore { get; set; }
        public string HealthRating { get; set; } = "Good";
        public string HealthBadgeClass { get; set; } = "bg-primary";
        public bool IsCurrentActiveHome { get; set; }
    }

    public class HomeOverviewViewModel
    {
        public Home Home { get; set; } = null!;
        public int TotalAppliances { get; set; }
        public int ActiveWarranties { get; set; }
        public int ExpiringWarranties { get; set; }
        public int OverdueMaintenance { get; set; }
        public decimal TotalMaintenanceSpending { get; set; }
        public decimal CurrentMonthSpending { get; set; }

        public string? MostServicedApplianceName { get; set; }
        public int MostServicedCount { get; set; }

        public string? MostExpensiveApplianceName { get; set; }
        public decimal? MostExpensivePrice { get; set; }

        public int AverageHealthScore { get; set; }
        public string HealthRating { get; set; } = "Good";
        public string HealthBadgeClass { get; set; } = "bg-primary";

        public List<ApplianceItemViewModel> Appliances { get; set; } = new();
    }

    public class HomeFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Home name is required.")]
        [StringLength(100, ErrorMessage = "Home name cannot exceed 100 characters.")]
        [Display(Name = "Home / Property Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Street Address")]
        public string? Address { get; set; }

        [StringLength(100)]
        [Display(Name = "City")]
        public string? City { get; set; }

        [StringLength(100)]
        [Display(Name = "State / Province")]
        public string? State { get; set; }

        [StringLength(20)]
        [Display(Name = "Postal / Zip Code")]
        public string? ZipCode { get; set; }

        [Required]
        [Display(Name = "Property Type")]
        public string HomeType { get; set; } = "Primary Residence"; // Primary Residence, Rental Apartment, Vacation Home, Office
    }
}
