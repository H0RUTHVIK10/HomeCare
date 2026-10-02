using HomeCare.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeCare.Data
{
    public class HomeCareDbContext : DbContext
    {
        public HomeCareDbContext(DbContextOptions<HomeCareDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Home> Homes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Appliance> Appliances { get; set; }
        public DbSet<Warranty> Warranties { get; set; }
        public DbSet<ServiceRecord> ServiceRecords { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration & Unique Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // User → Homes
            modelBuilder.Entity<User>()
                .HasMany(u => u.Homes)
                .WithOne(h => h.User)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → Notifications
            modelBuilder.Entity<User>()
                .HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → Reminders (optional direct FK)
            modelBuilder.Entity<Reminder>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reminders)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Home → Appliances
            modelBuilder.Entity<Home>()
                .HasMany(h => h.Appliances)
                .WithOne(a => a.Home)
                .HasForeignKey(a => a.HomeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Category → Appliances
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Appliances)
                .WithOne(a => a.Category)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appliance → Warranty (one-to-one)
            modelBuilder.Entity<Appliance>()
                .HasOne(a => a.Warranty)
                .WithOne(w => w.Appliance)
                .HasForeignKey<Warranty>(w => w.ApplianceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Appliance → Service Records
            modelBuilder.Entity<Appliance>()
                .HasMany(a => a.ServiceRecords)
                .WithOne(s => s.Appliance)
                .HasForeignKey(s => s.ApplianceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Appliance → Documents
            modelBuilder.Entity<Appliance>()
                .HasMany(a => a.Documents)
                .WithOne(d => d.Appliance)
                .HasForeignKey(d => d.ApplianceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Appliance → Reminders
            modelBuilder.Entity<Appliance>()
                .HasMany(a => a.Reminders)
                .WithOne(r => r.Appliance)
                .HasForeignKey(r => r.ApplianceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Decimal precision
            modelBuilder.Entity<Appliance>()
                .Property(a => a.PurchasePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ServiceRecord>()
                .Property(s => s.Cost)
                .HasPrecision(18, 2);

            // Performance Indexes (Requirement 39)
            modelBuilder.Entity<Home>()
                .HasIndex(h => h.UserId);

            modelBuilder.Entity<Appliance>()
                .HasIndex(a => a.HomeId);

            modelBuilder.Entity<Appliance>()
                .HasIndex(a => a.CategoryId);

            modelBuilder.Entity<Appliance>()
                .HasIndex(a => a.SerialNumber);

            modelBuilder.Entity<Appliance>()
                .HasIndex(a => a.Status);

            modelBuilder.Entity<Warranty>()
                .HasIndex(w => w.EndDate);

            modelBuilder.Entity<ServiceRecord>()
                .HasIndex(s => s.ServiceDate);

            modelBuilder.Entity<ServiceRecord>()
                .HasIndex(s => s.NextServiceDate);

            modelBuilder.Entity<Reminder>()
                .HasIndex(r => r.ReminderDate);

            modelBuilder.Entity<Reminder>()
                .HasIndex(r => r.UserId);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });
        }
    }
}