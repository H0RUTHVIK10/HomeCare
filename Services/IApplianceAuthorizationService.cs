namespace HomeCare.Services
{
    public interface IApplianceAuthorizationService
    {
        Task<bool> CanAccessHomeAsync(int homeId, int userId);
        Task<bool> CanAccessApplianceAsync(int applianceId, int userId);
        Task<bool> CanAccessDocumentAsync(int documentId, int userId);
        Task<bool> CanAccessServiceRecordAsync(int serviceRecordId, int userId);
        Task<bool> CanAccessWarrantyAsync(int warrantyId, int userId);
        Task<bool> CanAccessReminderAsync(int reminderId, int userId);
    }
}
