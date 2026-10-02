using HomeCare.Models;

namespace HomeCare.Services
{
    public interface INotificationService
    {
        Task CheckAndGenerateRemindersAndNotificationsAsync(int userId);
        Task<List<Notification>> GetUserNotificationsAsync(int userId, int limit = 20);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkAsReadAsync(int notificationId, int userId);
        Task MarkAllAsReadAsync(int userId);
        Task ClearAllAsync(int userId);
        Task CreateNotificationAsync(int userId, string title, string message, string type = "System", string? linkUrl = null);
    }
}
