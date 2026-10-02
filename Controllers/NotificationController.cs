using HomeCare.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeCare.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly IUserContext _userContext;

        public NotificationController(
            INotificationService notificationService,
            IUserContext userContext)
        {
            _notificationService = notificationService;
            _userContext = userContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var notifications = await _notificationService.GetUserNotificationsAsync(userId.Value, limit: 50);
            return View(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return Json(new { count = 0 });

            var count = await _notificationService.GetUnreadCountAsync(userId.Value);
            return Json(new { count });
        }

        [HttpGet]
        public async Task<IActionResult> GetRecent()
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return Json(new List<object>());

            var notifications = await _notificationService.GetUserNotificationsAsync(userId.Value, limit: 5);
            var result = notifications.Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                n.NotificationType,
                n.IsRead,
                n.LinkUrl,
                TimeAgo = GetTimeAgo(n.CreatedAt)
            });

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (userId.HasValue)
            {
                await _notificationService.MarkAsReadAsync(id, userId.Value);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _userContext.CurrentUserId;
            if (userId.HasValue)
            {
                await _notificationService.MarkAllAsReadAsync(userId.Value);
                TempData["SuccessMessage"] = "All notifications marked as read.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAll()
        {
            var userId = _userContext.CurrentUserId;
            if (userId.HasValue)
            {
                await _notificationService.ClearAllAsync(userId.Value);
                TempData["SuccessMessage"] = "Notification history cleared.";
            }
            return RedirectToAction(nameof(Index));
        }

        private static string GetTimeAgo(DateTime dt)
        {
            var diff = DateTime.UtcNow - dt;
            if (diff.TotalMinutes < 60) return $"{Math.Max(1, (int)diff.TotalMinutes)}m ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            return $"{(int)diff.TotalDays}d ago";
        }
    }
}
