using System.ComponentModel.DataAnnotations;

namespace HomeCare.Models
{
    public class Warranty
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [StringLength(150)]
        public string? Provider { get; set; }

        [StringLength(100)]
        public string? WarrantyNumber { get; set; }

        [StringLength(500)]
        public string? CoverageDetails { get; set; }

        [StringLength(50)]
        public string? ContactNumber { get; set; }

        [StringLength(500)]
        public string? Terms { get; set; }

        // Foreign Key
        public int ApplianceId { get; set; }

        // Relationship
        public Appliance Appliance { get; set; } = null!;
    }
}