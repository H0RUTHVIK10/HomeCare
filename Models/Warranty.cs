using System.ComponentModel.DataAnnotations;

namespace HomeCare.Models
{
    public class Warranty
    {
        public int Id { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [StringLength(150)]
        public string? Provider { get; set; }

        [StringLength(500)]
        public string? Terms { get; set; }

        // Foreign Key
        public int ApplianceId { get; set; }

        // Relationship
        public Appliance Appliance { get; set; } = null!;
    }
}