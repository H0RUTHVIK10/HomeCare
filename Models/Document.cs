using System.ComponentModel.DataAnnotations;

namespace HomeCare.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string DocumentName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string DocumentType { get; set; } = string.Empty; // Invoice, Warranty, Service Receipt, Manual, Other

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public long? FileSize { get; set; }

        [StringLength(100)]
        public string? ContentType { get; set; }

        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public int ApplianceId { get; set; }

        // Relationship
        public Appliance Appliance { get; set; } = null!;
    }
}