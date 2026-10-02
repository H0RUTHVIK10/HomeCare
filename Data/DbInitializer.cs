using HomeCare.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeCare.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(HomeCareDbContext context)
        {
            // Seed Categories if none exist
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category
                    {
                        Name = "Kitchen Appliances",
                        Icon = "bi-cup-hot",
                        Description = "Refrigerators, microwaves, dishwashers, cooktops, ovens"
                    },
                    new Category
                    {
                        Name = "Laundry & Cleaning",
                        Icon = "bi-moisture",
                        Description = "Washing machines, tumble dryers, robotic vacuums"
                    },
                    new Category
                    {
                        Name = "Climate Control & HVAC",
                        Icon = "bi-snow",
                        Description = "Air conditioners, heat pumps, air purifiers, humidifiers"
                    },
                    new Category
                    {
                        Name = "Home Entertainment",
                        Icon = "bi-tv",
                        Description = "Smart televisions, home theater systems, consoles"
                    },
                    new Category
                    {
                        Name = "Water & Plumbing",
                        Icon = "bi-droplet-half",
                        Description = "RO water purifiers, water heaters, geysers, softeners"
                    },
                    new Category
                    {
                        Name = "Small Electronics",
                        Icon = "bi-plug",
                        Description = "Mixers, blenders, electric kettles, toasters, irons"
                    },
                    new Category
                    {
                        Name = "Security & Smart Home",
                        Icon = "bi-shield-check",
                        Description = "CCTV security cameras, smart door locks, sensor hubs"
                    }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }
        }
    }
}
