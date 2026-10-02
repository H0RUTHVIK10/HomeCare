using HomeCare.Data;
using HomeCare.Models;
using HomeCare.Services;
using HomeCare.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeCare.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly HomeCareDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IApplianceAuthorizationService _authService;
        private readonly IFileService _fileService;

        public DocumentController(
            HomeCareDbContext context,
            IUserContext userContext,
            IApplianceAuthorizationService authService,
            IFileService fileService)
        {
            _context = context;
            _userContext = userContext;
            _authService = authService;
            _fileService = fileService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(DocumentUploadViewModel model)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessApplianceAsync(model.ApplianceId, userId.Value))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid document upload submission. Please ensure all required fields are filled.";
                return RedirectToAction("Details", "Appliance", new { id = model.ApplianceId, tab = "documents" });
            }

            var uploadResult = await _fileService.SaveDocumentAsync(model.File, "documents");
            if (!uploadResult.Success)
            {
                TempData["ErrorMessage"] = uploadResult.ErrorMessage ?? "Document upload failed.";
                return RedirectToAction("Details", "Appliance", new { id = model.ApplianceId, tab = "documents" });
            }

            var doc = new Document
            {
                ApplianceId = model.ApplianceId,
                DocumentName = model.DocumentName.Trim(),
                DocumentType = model.DocumentType.Trim(),
                FilePath = uploadResult.FilePath!,
                FileSize = uploadResult.FileSize,
                ContentType = uploadResult.ContentType,
                UploadedDate = DateTime.UtcNow
            };

            _context.Documents.Add(doc);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Document '{doc.DocumentName}' uploaded successfully!";
            return RedirectToAction("Details", "Appliance", new { id = model.ApplianceId, tab = "documents" });
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessDocumentAsync(id, userId.Value))
            {
                return Forbid();
            }

            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return NotFound();

            var physicalPath = _fileService.GetPhysicalPath(doc.FilePath);
            if (!System.IO.File.Exists(physicalPath))
            {
                TempData["ErrorMessage"] = "Physical file was not found on the server.";
                return RedirectToAction("Details", "Appliance", new { id = doc.ApplianceId, tab = "documents" });
            }

            var contentType = doc.ContentType ?? "application/octet-stream";
            var extension = Path.GetExtension(physicalPath);
            var downloadFileName = $"{doc.DocumentName}{extension}";

            return PhysicalFile(physicalPath, contentType, downloadFileName);
        }

        [HttpGet]
        public async Task<IActionResult> Preview(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessDocumentAsync(id, userId.Value))
            {
                return Forbid();
            }

            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return NotFound();

            var physicalPath = _fileService.GetPhysicalPath(doc.FilePath);
            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound("Document file not found on disk.");
            }

            var contentType = doc.ContentType ?? "application/pdf";
            return PhysicalFile(physicalPath, contentType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userContext.CurrentUserId;
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!await _authService.CanAccessDocumentAsync(id, userId.Value))
            {
                return Forbid();
            }

            var doc = await _context.Documents.FindAsync(id);
            if (doc != null)
            {
                var applianceId = doc.ApplianceId;
                _fileService.DeleteFile(doc.FilePath);

                _context.Documents.Remove(doc);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Document '{doc.DocumentName}' deleted.";
                return RedirectToAction("Details", "Appliance", new { id = applianceId, tab = "documents" });
            }

            return RedirectToAction("Index", "Appliance");
        }
    }
}
