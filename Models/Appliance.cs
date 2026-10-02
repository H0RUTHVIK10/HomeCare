using System.ComponentModel.DataAnnotations;

namespace HomeCare.Models
{
    public class Appliance
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ModelNumber { get; set; }

        [StringLength(100)]
        public string? SerialNumber { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public decimal? PurchasePrice { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Under Repair, Retired, Sold

        [StringLength(100)]
        public string? Location { get; set; } // e.g. Kitchen, Living Room, Laundry

        [StringLength(255)]
        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Foreign Keys
        public int HomeId { get; set; }

        public int CategoryId { get; set; }

        // Relationships
        public Home Home { get; set; } = null!;

        public Category Category { get; set; } = null!;

        public Warranty? Warranty { get; set; }

        public ICollection<ServiceRecord> ServiceRecords { get; set; }
            = new List<ServiceRecord>();

        public ICollection<Document> Documents { get; set; }
            = new List<Document>();

        public ICollection<Reminder> Reminders { get; set; }
            = new List<Reminder>();
    }
}