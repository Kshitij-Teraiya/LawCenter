using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Deals;
using LegalConnect.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface IHireRequestDocumentService
{
    Task<List<HireRequestDocumentDto>> GetDocumentsAsync(int userId, string role, int hireRequestId);
    Task<(bool Success, string Message, HireRequestDocumentDto? Data)> UploadDocumentAsync(int clientUserId, int hireRequestId, IFormFile file);
    Task<(HireRequestDocument? Doc, bool CanAccess)> GetDocumentForDownloadAsync(int userId, string role, int docId);
    Task<(bool Success, string Message)> DeleteDocumentAsync(int clientUserId, int docId);
}

public class HireRequestDocumentService : IHireRequestDocumentService
{
    private readonly AppDbContext       _db;
    private readonly IFileUploadService _fileUpload;

    private static readonly string[] AllowedExtensions =
        [".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".txt", ".xls", ".xlsx"];
    private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB

    public HireRequestDocumentService(AppDbContext db, IFileUploadService fileUpload)
    {
        _db         = db;
        _fileUpload = fileUpload;
    }

    // ── List ─────────────────────────────────────────────────────────

    public async Task<List<HireRequestDocumentDto>> GetDocumentsAsync(int userId, string role, int hireRequestId)
    {
        if (!await HasAccessAsync(userId, role, hireRequestId)) return [];

        return await _db.HireRequestDocuments
            .Where(d => d.HireRequestId == hireRequestId)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => MapToDto(d))
            .ToListAsync();
    }

    // ── Upload (Client only) ─────────────────────────────────────────

    public async Task<(bool Success, string Message, HireRequestDocumentDto? Data)> UploadDocumentAsync(
        int clientUserId, int hireRequestId, IFormFile file)
    {
        var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return (false, "Client profile not found.", null);

        var hr = await _db.HireRequests.FirstOrDefaultAsync(h =>
            h.Id == hireRequestId && h.ClientProfileId == clientProfile.Id);
        if (hr == null) return (false, "Hire request not found or access denied.", null);
        if (hr.Status == HireRequestStatus.Rejected || hr.IsDeleted)
            return (false, "Cannot upload documents to a closed hire request.", null);

        // Validate file
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return (false, $"File type '{ext}' is not allowed.", null);
        if (file.Length > MaxFileSizeBytes)
            return (false, "File size exceeds 20 MB limit.", null);

        var (_, relativePath) = await _fileUpload.SaveHireRequestDocumentAsync(file, hireRequestId);

        var doc = new HireRequestDocument
        {
            HireRequestId = hireRequestId,
            FileName      = file.FileName,
            FilePath      = relativePath,
            FileSize      = file.Length,
            ContentType   = file.ContentType,
            UploadedAt    = DateTime.UtcNow
        };
        _db.HireRequestDocuments.Add(doc);
        await _db.SaveChangesAsync();

        return (true, "Document uploaded successfully.", MapToDto(doc));
    }

    // ── Download ─────────────────────────────────────────────────────

    public async Task<(HireRequestDocument? Doc, bool CanAccess)> GetDocumentForDownloadAsync(
        int userId, string role, int docId)
    {
        var doc = await _db.HireRequestDocuments
            .Include(d => d.HireRequest)
            .FirstOrDefaultAsync(d => d.Id == docId);
        if (doc == null) return (null, false);

        var canAccess = await HasAccessAsync(userId, role, doc.HireRequestId);
        return (doc, canAccess);
    }

    // ── Delete (Client only) ─────────────────────────────────────────

    public async Task<(bool Success, string Message)> DeleteDocumentAsync(int clientUserId, int docId)
    {
        var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return (false, "Client profile not found.");

        var doc = await _db.HireRequestDocuments
            .Include(d => d.HireRequest)
            .FirstOrDefaultAsync(d => d.Id == docId && d.HireRequest.ClientProfileId == clientProfile.Id);
        if (doc == null) return (false, "Document not found or access denied.");

        await _fileUpload.DeleteHireRequestDocumentAsync(doc.FilePath);
        _db.HireRequestDocuments.Remove(doc);
        await _db.SaveChangesAsync();
        return (true, "Document deleted.");
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private async Task<bool> HasAccessAsync(int userId, string role, int hireRequestId)
    {
        if (role == "Admin") return true;

        if (role == "Client")
        {
            var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (clientProfile == null) return false;
            return await _db.HireRequests.AnyAsync(h =>
                h.Id == hireRequestId && h.ClientProfileId == clientProfile.Id);
        }

        // Lawyer: must be the assigned lawyer on the hire request
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
        if (lawyerProfile == null) return false;
        return await _db.HireRequests.AnyAsync(h =>
            h.Id == hireRequestId && h.LawyerProfileId == lawyerProfile.Id);
    }

    private static HireRequestDocumentDto MapToDto(HireRequestDocument d) => new()
    {
        Id            = d.Id,
        HireRequestId = d.HireRequestId,
        FileName      = d.FileName,
        FileSize      = d.FileSize,
        ContentType   = d.ContentType,
        UploadedAt    = d.UploadedAt
    };
}
