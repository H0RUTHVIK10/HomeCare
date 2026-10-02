using System.ComponentModel.DataAnnotations;

namespace HomeCare.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Icon { get; set; } // e.g. bi-snow, bi-tv, bi-water, bi-fan

        [StringLength(250)]
        public string? Description { get; set; }

        // Relationship
        public ICollection<Appliance> Appliances { get; set; } = new List<Appliance>();
    }
}