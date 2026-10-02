using System.ComponentModel.DataAnnotations;
using HomeCare.Models;
using HomeCare.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HomeCare.ViewModels
{
    public class ApplianceListViewModel
    {
        // Filters
        public string? SearchTerm { get; set; }
        public int? SelectedHomeId { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string? SelectedStatus { get; set; }
        public string? SelectedWarrantyStatus { get; set; }

        public List<SelectListItem> HomesList { get; set; } = new();
        public List<SelectListItem> CategoriesList { get; set; } = new();
        public List<ApplianceItemViewModel> Appliances { get; set; } = new();

        public int TotalCount => Appliances.Count;
        public int ActiveCount => Appliances.Count(a => a.Status == "Active");
        public int AttentionCount => Appliances.Count(a => a.HealthScore < 70);
    }

    public class ApplianceItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = "bi-plug";
        public string? ModelNumber { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public string Status { get; set; } = "Active";
        public string? Location { get; set; }
        public string? ImagePath { get; set; }
        public string HomeName { get; set; } = string.Empty;

        // Warranty
        public bool HasWarranty { get; set; }
        public string WarrantyStatus { get; set; } = "None"; // Active, Expiring Soon, Expired, None
        public string WarrantyBadgeClass { get; set; } = "bg-secondary";
        public string WarrantyCountdownText { get; set; } = string.Empty;

        // Service & Health
        public DateTime? NextServiceDate { get; set; }
        public int HealthScore { get; set; }
        public string HealthRating { get; set; } = "Good";
        public string HealthBadgeClass { get; set; } = "bg-success";
    }

    public class ApplianceDetailsViewModel
    {
        public Appliance Appliance { get; set; } = null!;
        public HealthScoreResult HealthScore { get; set; } = new();
        public List<TimelineItem> TimelineItems { get; set; } = new();

        // Warranty Countdown Details
        public bool HasWarranty => Appliance.Warranty != null;
        public string WarrantyCountdownText { get; set; } = string.Empty;
        public string WarrantyStatus { get; set; } = "None";
        public string WarrantyBadgeClass { get; set; } = "bg-secondary";

        // Cost & Service Stats
        public decimal TotalMaintenanceCost { get; set; }
        public decimal TotalRepairCost { get; set; }
        public decimal AverageServiceCost { get; set; }
        public int TotalServicesCount { get; set; }

        // QR Code
        public string QrCodeBase64 { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;

        // Active Tab
        public string ActiveTab { get; set; } = "overview";
    }

    public class ApplianceFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Appliance name is required.")]
        [StringLength(100)]
        [Display(Name = "Appliance Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Brand is required.")]
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Model Number")]
        public string? ModelNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Purchase Date")]
        public DateTime? PurchaseDate { get; set; }

        [Range(0, 10000000, ErrorMessage = "Price must be greater than or equal to 0.")]
        [Display(Name = "Purchase Price (₹ / $)")]
        public decimal? PurchasePrice { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Operational Status")]
        public string Status { get; set; } = "Active"; // Active, Under Repair, Retired, Sold

        [StringLength(100)]
        [Display(Name = "Location / Room")]
        public string? Location { get; set; } // Kitchen, Living Room, etc.

        public string? ExistingImagePath { get; set; }

        [Display(Name = "Appliance Photo")]
        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Please select a home.")]
        [Display(Name = "Belongs to Home")]
        public int HomeId { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public List<SelectListItem> HomesList { get; set; } = new();
        public List<SelectListItem> CategoriesList { get; set; } = new();
    }

    public class TimelineItem
    {
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = "Service"; // Purchase, Warranty, Service, Repair, Document
        public string IconClass { get; set; } = "bi-wrench";
        public string BadgeClass { get; set; } = "bg-primary";
        public decimal? Cost { get; set; }
    }
}
