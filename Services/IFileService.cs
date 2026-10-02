using Microsoft.AspNetCore.Http;

namespace HomeCare.Services
{
    public interface IFileService
    {
        Task<(bool Success, string? FilePath, string? ErrorMessage, long FileSize, string? ContentType)> SaveDocumentAsync(IFormFile file, string subFolder = "documents");
        Task<(bool Success, string? FilePath, string? ErrorMessage)> SaveImageAsync(IFormFile file, string subFolder = "appliances");
        bool DeleteFile(string? relativePath);
        string GetPhysicalPath(string relativePath);
    }
}
