using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HomeCare.ViewModels
{
    public class DocumentUploadViewModel
    {
        public int ApplianceId { get; set; }
        public string ApplianceName { get; set; } = string.Empty;
        public string ApplianceBrand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document title is required.")]
        [StringLength(150)]
        [Display(Name = "Document Title")]
        public string DocumentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document type is required.")]
        [StringLength(50)]
        [Display(Name = "Document Category")]
        public string DocumentType { get; set; } = "Invoice"; // Invoice, Warranty, Service Receipt, Manual, Other

        [Required(ErrorMessage = "Please select a file to upload.")]
        [Display(Name = "Choose File (PDF, JPG, PNG - Max 10MB)")]
        public IFormFile File { get; set; } = null!;
    }
}
