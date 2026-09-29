using System.ComponentModel.DataAnnotations;

namespace HomeCare.Models
{
    public class ServiceRecord
    {
        public int Id { get; set; }

        public DateTime ServiceDate { get; set; }

        [Required]
        [StringLength(100)]
        public string ServiceType { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? TechnicianName { get; set; }

        public decimal? Cost { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Foreign Key
        public int ApplianceId { get; set; }

        // Relationship
        public Appliance Appliance { get; set; } = null!;
    }
}