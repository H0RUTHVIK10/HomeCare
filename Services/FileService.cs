using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace HomeCare.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        private static readonly HashSet<string> AllowedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".jpg", ".jpeg", ".png"
        };

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/pjpeg"
        };

        private static readonly HashSet<string> DangerousExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".bat", ".cmd", ".ps1", ".dll", ".vbs", ".js", ".sh", ".com", ".scr", ".msi", ".jar", ".aspx", ".asp", ".php", ".py"
        };

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<(bool Success, string? FilePath, string? ErrorMessage, long FileSize, string? ContentType)> SaveDocumentAsync(
            IFormFile file, string subFolder = "documents")
        {
            if (file == null || file.Length == 0)
            {
                return (false, null, "No file was uploaded.", 0, null);
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (false, null, "File size exceeds the 10 MB maximum limit.", 0, null);
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (DangerousExtensions.Contains(extension))
            {
                return (false, null, "Executable or script files are strictly prohibited.", 0, null);
            }

            if (!AllowedDocumentExtensions.Contains(extension))
            {
                return (false, null, "Only PDF, JPG, JPEG, and PNG files are allowed.", 0, null);
            }

            var contentType = file.ContentType.ToLowerInvariant();
            if (!AllowedMimeTypes.Contains(contentType))
            {
                return (false, null, $"Invalid file MIME type: {file.ContentType}.", 0, null);
            }

            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", subFolder);
            if (!Directory.Exists(uploadsRoot))
            {
                Directory.CreateDirectory(uploadsRoot);
            }

            // Generate randomized secure filename to prevent path traversal and collision
            var safeFileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(uploadsRoot, safeFileName);

            // Path traversal safety check
            var fullUploadsDir = Path.GetFullPath(uploadsRoot);
            var fullFilePath = Path.GetFullPath(physicalPath);
            if (!fullFilePath.StartsWith(fullUploadsDir, StringComparison.OrdinalIgnoreCase))
            {
                return (false, null, "Invalid file destination path detected.", 0, null);
            }

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/{subFolder}/{safeFileName}";
            return (true, relativePath, null, file.Length, contentType);
        }

        public async Task<(bool Success, string? FilePath, string? ErrorMessage)> SaveImageAsync(
            IFormFile file, string subFolder = "appliances")
        {
            if (file == null || file.Length == 0)
            {
                return (false, null, "No file was uploaded.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (false, null, "Image size exceeds the 10 MB maximum limit.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedImageExtensions.Contains(extension) || DangerousExtensions.Contains(extension))
            {
                return (false, null, "Only JPG, PNG, and WebP images are allowed.");
            }

            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", subFolder);
            if (!Directory.Exists(uploadsRoot))
            {
                Directory.CreateDirectory(uploadsRoot);
            }

            var safeFileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(uploadsRoot, safeFileName);

            var fullUploadsDir = Path.GetFullPath(uploadsRoot);
            var fullFilePath = Path.GetFullPath(physicalPath);
            if (!fullFilePath.StartsWith(fullUploadsDir, StringComparison.OrdinalIgnoreCase))
            {
                return (false, null, "Invalid file destination path detected.");
            }

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/{subFolder}/{safeFileName}";
            return (true, relativePath, null);
        }

        public bool DeleteFile(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return false;

            try
            {
                var physicalPath = GetPhysicalPath(relativePath);
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    return true;
                }
            }
            catch
            {
                // Silently return false if file delete fails
            }
            return false;
        }

        public string GetPhysicalPath(string relativePath)
        {
            var cleaned = relativePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(_environment.WebRootPath, cleaned);
        }
    }
}
