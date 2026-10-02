using System.ComponentModel.DataAnnotations;

namespace HomeCare.ViewModels
{
    public class ServiceRecordFormViewModel
    {
        public int Id { get; set; }

        public int ApplianceId { get; set; }
        public string ApplianceName { get; set; } = string.Empty;
        public string ApplianceBrand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Service date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Service / Maintenance Date")]
        public DateTime ServiceDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Service type is required.")]
        [StringLength(100)]
        [Display(Name = "Service Type")]
        public string ServiceType { get; set; } = "Routine Maintenance"; // Routine Maintenance, Repair, Filter Replacement, Inspection, Installation, Other

        [StringLength(500)]
        [Display(Name = "Work Description")]
        public string? Description { get; set; }

        [StringLength(100)]
        [Display(Name = "Technician Name")]
        public string? TechnicianName { get; set; }

        [StringLength(150)]
        [Display(Name = "Service Center / Company")]
        public string? ServiceProvider { get; set; }

        [Range(0, 1000000, ErrorMessage = "Cost must be a positive number.")]
        [Display(Name = "Service Cost (₹ / $)")]
        public decimal? Cost { get; set; }

        [Required]
        [Display(Name = "Service Status")]
        public string ServiceStatus { get; set; } = "Completed"; // Completed, Scheduled, In Progress

        [DataType(DataType.Date)]
        [Display(Name = "Next Recommended Service Date")]
        public DateTime? NextServiceDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Technician Notes / Advice")]
        public string? Notes { get; set; }
    }
}
