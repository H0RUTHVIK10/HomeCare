using System.ComponentModel.DataAnnotations;

namespace HomeCare.Models
{
    public class ServiceRecord
    {
        public int Id { get; set; }

        [Required]
        public DateTime ServiceDate { get; set; }

        [Required]
        [StringLength(100)]
        public string ServiceType { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? TechnicianName { get; set; }

        [StringLength(150)]
        public string? ServiceProvider { get; set; }

        public decimal? Cost { get; set; }

        [StringLength(50)]
        public string ServiceStatus { get; set; } = "Completed"; // Completed, Scheduled, In Progress

        public DateTime? NextServiceDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Foreign Key
        public int ApplianceId { get; set; }

        // Relationship
        public Appliance Appliance { get; set; } = null!;
    }
}