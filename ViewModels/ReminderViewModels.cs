using System.ComponentModel.DataAnnotations;

namespace HomeCare.ViewModels
{
    public class ReminderFormViewModel
    {
        public int Id { get; set; }

        public int ApplianceId { get; set; }
        public string ApplianceName { get; set; } = string.Empty;
        public string ApplianceBrand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reminder title is required.")]
        [StringLength(150)]
        [Display(Name = "Task / Reminder Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Description / Instructions")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Reminder date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime ReminderDate { get; set; } = DateTime.Today.AddDays(7);

        [Required]
        [Display(Name = "Reminder Type")]
        public string ReminderType { get; set; } = "Maintenance"; // Maintenance, Warranty, Service, Inspection, Custom

        [Display(Name = "Mark as Completed")]
        public bool IsCompleted { get; set; } = false;
    }
}
