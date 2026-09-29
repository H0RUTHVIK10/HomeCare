using System.ComponentModel.DataAnnotations;

namespace HomeCare.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Relationship
        public ICollection<Appliance> Appliances { get; set; } = new List<Appliance>();
    }
}