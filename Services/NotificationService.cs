using HomeCare.Data;
using HomeCare.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeCare.Services
{
    public class NotificationService : INotificationService
    {
        private readonly HomeCareDbContext _context;

        public NotificationService(HomeCareDbContext context)
        {
            _context = context;
        }

        public async Task CheckAndGenerateRemindersAndNotificationsAsync(int userId)
        {
            if (userId <= 0) return;

            var now = DateTime.UtcNow;
            var cutOff24hAgo = now.AddHours(-24);

            // Fetch appliances belonging to this user
            var appliances = await _context.Appliances
                .Include(a => a.Home)
                .Include(a => a.Warranty)
                .Include(a => a.Reminders)
                .Include(a => a.ServiceRecords)
                .Where(a => a.Home.UserId == userId)
                .ToListAsync();

            var recentNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.CreatedAt >= cutOff24hAgo)
                .ToListAsync();

            var notificationsToAdd = new List<Notification>();

            foreach (var app in appliances)
            {
                // 1. Warranty Checks
                if (app.Warranty != null)
                {
                    var daysUntilExpiry = (app.Warranty.EndDate - now).TotalDays;
                    string? warrantyAlertMsg = null;
                    string alertType = "Warranty";

                    if (daysUntilExpiry <= 0 && daysUntilExpiry >= -7)
                    {
                        warrantyAlertMsg = $"{app.Name} ({app.Brand}) warranty expired on {app.Warranty.EndDate:MMM dd, yyyy}.";
                    }
                    else if (daysUntilExpiry > 0 && daysUntilExpiry <= 1)
                    {
                        warrantyAlertMsg = $"{app.Name} ({app.Brand}) warranty expires TOMORROW ({app.Warranty.EndDate:MMM dd, yyyy}).";
                    }
                    else if (daysUntilExpiry > 1 && daysUntilExpiry <= 7)
                    {
                        warrantyAlertMsg = $"{app.Name} ({app.Brand}) warranty expires in {Math.Ceiling(daysUntilExpiry)} days.";
                    }
                    else if (daysUntilExpiry > 7 && daysUntilExpiry <= 15)
                    {
                        warrantyAlertMsg = $"{app.Name} ({app.Brand}) warranty expires in {Math.Ceiling(daysUntilExpiry)} days.";
                    }
                    else if (daysUntilExpiry > 15 && daysUntilExpiry <= 30)
                    {
                        warrantyAlertMsg = $"{app.Name} ({app.Brand}) warranty expires in {Math.Ceiling(daysUntilExpiry)} days.";
                    }

                    if (warrantyAlertMsg != null)
                    {
                        bool exists = recentNotifications.Any(n =>
                            n.NotificationType == alertType &&
                            n.Message.Contains(app.Name) &&
                            n.CreatedAt >= cutOff24hAgo);

                        if (!exists)
                        {
                            notificationsToAdd.Add(new Notification
                            {
                                UserId = userId,
                                Title = "Warranty Notice",
                                Message = warrantyAlertMsg,
                                NotificationType = alertType,
                                LinkUrl = $"/Appliance/Details/{app.Id}",
                                CreatedAt = now
                            });
                        }
                    }
                }

                // 2. Reminders checks (upcoming / overdue)
                if (app.Reminders != null)
                {
                    foreach (var reminder in app.Reminders.Where(r => !r.IsCompleted))
                    {
                        var daysUntilReminder = (reminder.ReminderDate - now).TotalDays;
                        string? reminderMsg = null;

                        if (daysUntilReminder < 0)
                        {
                            reminderMsg = $"{app.Name}: Reminder '{reminder.Title}' is overdue since {reminder.ReminderDate:MMM dd, yyyy}.";
                        }
                        else if (daysUntilReminder <= 1)
                        {
                            reminderMsg = $"{app.Name}: Reminder '{reminder.Title}' is due soon ({reminder.ReminderDate:MMM dd, yyyy}).";
                        }
                        else if (daysUntilReminder <= 7)
                        {
                            reminderMsg = $"{app.Name}: Maintenance '{reminder.Title}' is due in {Math.Ceiling(daysUntilReminder)} days.";
                        }

                        if (reminderMsg != null)
                        {
                            bool exists = recentNotifications.Any(n =>
                                n.NotificationType == "Maintenance" &&
                                n.Message.Contains(reminder.Title) &&
                                n.CreatedAt >= cutOff24hAgo);

                            if (!exists)
                            {
                                notificationsToAdd.Add(new Notification
                                {
                                    UserId = userId,
                                    Title = "Maintenance Reminder",
                                    Message = reminderMsg,
                                    NotificationType = "Maintenance",
                                    LinkUrl = $"/Appliance/Details/{app.Id}",
                                    CreatedAt = now
                                });
                            }
                        }
                    }
                }

                // 3. NextServiceDate checks
                if (app.ServiceRecords != null)
                {
                    foreach (var sr in app.ServiceRecords.Where(s => s.NextServiceDate.HasValue && s.ServiceStatus != "Completed"))
                    {
                        var daysUntilService = (sr.NextServiceDate!.Value - now).TotalDays;
                        string? serviceMsg = null;

                        if (daysUntilService < 0)
                        {
                            serviceMsg = $"{app.Name} scheduled {sr.ServiceType} was due on {sr.NextServiceDate:MMM dd, yyyy}.";
                        }
                        else if (daysUntilService <= 7)
                        {
                            serviceMsg = $"{app.Name} scheduled {sr.ServiceType} is due in {Math.Ceiling(daysUntilService)} days.";
                        }

                        if (serviceMsg != null)
                        {
                            bool exists = recentNotifications.Any(n =>
                                n.NotificationType == "Service" &&
                                n.Message.Contains(app.Name) &&
                                n.CreatedAt >= cutOff24hAgo);

                            if (!exists)
                            {
                                notificationsToAdd.Add(new Notification
                                {
                                    UserId = userId,
                                    Title = "Scheduled Service Due",
                                    Message = serviceMsg,
                                    NotificationType = "Service",
                                    LinkUrl = $"/Appliance/Details/{app.Id}",
                                    CreatedAt = now
                                });
                            }
                        }
                    }
                }
            }

            if (notificationsToAdd.Count > 0)
            {
                await _context.Notifications.AddRangeAsync(notificationsToAdd);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(int userId, int limit = 20)
        {
            if (userId <= 0) return new List<Notification>();

            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            if (userId <= 0) return 0;
            return await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notif = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notif != null && !notif.IsRead)
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unread.Count > 0)
            {
                foreach (var n in unread)
                {
                    n.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearAllAsync(int userId)
        {
            var all = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            if (all.Count > 0)
            {
                _context.Notifications.RemoveRange(all);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CreateNotificationAsync(int userId, string title, string message, string type = "System", string? linkUrl = null)
        {
            if (userId <= 0) return;

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                NotificationType = type,
                LinkUrl = linkUrl,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }
    }
}
