using HomeCare.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeCare.Services
{
    public class ApplianceAuthorizationService : IApplianceAuthorizationService
    {
        private readonly HomeCareDbContext _context;

        public ApplianceAuthorizationService(HomeCareDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanAccessHomeAsync(int homeId, int userId)
        {
            if (homeId <= 0 || userId <= 0) return false;
            return await _context.Homes.AnyAsync(h => h.Id == homeId && h.UserId == userId);
        }

        public async Task<bool> CanAccessApplianceAsync(int applianceId, int userId)
        {
            if (applianceId <= 0 || userId <= 0) return false;
            return await _context.Appliances
                .AnyAsync(a => a.Id == applianceId && a.Home.UserId == userId);
        }

        public async Task<bool> CanAccessDocumentAsync(int documentId, int userId)
        {
            if (documentId <= 0 || userId <= 0) return false;
            return await _context.Documents
                .AnyAsync(d => d.Id == documentId && d.Appliance.Home.UserId == userId);
        }

        public async Task<bool> CanAccessServiceRecordAsync(int serviceRecordId, int userId)
        {
            if (serviceRecordId <= 0 || userId <= 0) return false;
            return await _context.ServiceRecords
                .AnyAsync(s => s.Id == serviceRecordId && s.Appliance.Home.UserId == userId);
        }

        public async Task<bool> CanAccessWarrantyAsync(int warrantyId, int userId)
        {
            if (warrantyId <= 0 || userId <= 0) return false;
            return await _context.Warranties
                .AnyAsync(w => w.Id == warrantyId && w.Appliance.Home.UserId == userId);
        }

        public async Task<bool> CanAccessReminderAsync(int reminderId, int userId)
        {
            if (reminderId <= 0 || userId <= 0) return false;
            return await _context.Reminders
                .AnyAsync(r => r.Id == reminderId && (r.UserId == userId || r.Appliance.Home.UserId == userId));
        }
    }
}
