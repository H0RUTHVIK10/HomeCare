using System.ComponentModel.DataAnnotations;

namespace HomeCare.ViewModels
{
    public class WarrantyFormViewModel
    {
        public int Id { get; set; }

        public int ApplianceId { get; set; }
        public string ApplianceName { get; set; } = string.Empty;
        public string ApplianceBrand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Warranty start date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Warranty Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Warranty end date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Warranty End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1);

        [StringLength(150)]
        [Display(Name = "Warranty Provider / Company")]
        public string? Provider { get; set; }

        [StringLength(100)]
        [Display(Name = "Warranty / Policy Number")]
        public string? WarrantyNumber { get; set; }

        [StringLength(500)]
        [Display(Name = "Coverage Details")]
        public string? CoverageDetails { get; set; }

        [StringLength(50)]
        [Phone]
        [Display(Name = "Support / Contact Number")]
        public string? ContactNumber { get; set; }

        [StringLength(500)]
        [Display(Name = "Terms & Exclusions")]
        public string? Terms { get; set; }
    }
}
