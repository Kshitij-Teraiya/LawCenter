using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Cases;
using LegalConnect.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface ICaseDocumentService
{
    Task<List<CaseDocumentDto>> GetDocumentsAsync(int userId, string role, int caseId, CaseDocumentFilterDto filter);
    Task<(bool Success, string Message, CaseDocumentDto? Data)> UploadDocumentAsync(
        int userId, string role, string userName, int caseId, IFormFile file, UploadDocumentDto dto);
    Task<(CaseDocument? Document, bool CanAccess)> GetDocumentForDownloadAsync(int userId, string role, int documentId);
    Task<(bool Success, string Message)> DeleteDocumentAsync(int userId, string role, int documentId);
    Task<(bool Success, string Message, bool NewValue)> ToggleAvailableForDealAsync(int clientUserId, int documentId);
    Task<(bool Success, string Message, bool NewValue)> TogglePrivateAsync(int userId, int documentId);
    Task<(CaseDocument? Doc, bool CanAccess)> GetDealDocumentViaHireRequestAsync(int lawyerOrClientUserId, string role, int hireRequestId, int caseDocId);
}

public class CaseDocumentService : ICaseDocumentService
{
    private readonly AppDbContext _db;
    private readonly IFileUploadService _fileUpload;
    private readonly IFileValidationService _fileValidation;

    public CaseDocumentService(AppDbContext db, IFileUploadService fileUpload, IFileValidationService fileValidation)
    {
        _db = db;
        _fileUpload = fileUpload;
        _fileValidation = fileValidation;
    }

    public async Task<List<CaseDocumentDto>> GetDocumentsAsync(int userId, string role, int caseId, CaseDocumentFilterDto filter)
    {
        if (!await CanAccessCaseAsync(userId, role, caseId))
            return [];

        var query = _db.CaseDocuments
            .Include(d => d.UploadedBy)
            .Include(d => d.LawyerShares)
            .Where(d => d.CaseId == caseId)
            .AsQueryable();

        if (role == "Client")
        {
            // Client sees: their own docs (public+private) + lawyer public docs
            query = query.Where(d =>
                (d.UploadedByRole == "Client" && d.UploadedByUserId == userId) ||
                (d.UploadedByRole == "Lawyer" && !d.IsPrivate));
        }
        else if (role == "Lawyer")
        {
            var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            if (lawyerProfile == null) return [];
            query = query.Where(d =>
                // Lawyer's own docs (even private ones)
                (d.UploadedByRole == "Lawyer" && (!d.IsPrivate || d.UploadedByUserId == userId)) ||
                // Client public docs shared with all lawyers, or specifically with this lawyer
                (d.UploadedByRole == "Client" && !d.IsPrivate &&
                 (d.SharedWithAllLawyers || d.LawyerShares.Any(s => s.LawyerProfileId == lawyerProfile.Id))));
        }

        if (!string.IsNullOrEmpty(filter.DocumentType))
            query = query.Where(d => d.DocumentType == filter.DocumentType);

        if (filter.IsPrivate.HasValue)
            query = query.Where(d => d.IsPrivate == filter.IsPrivate.Value);

        return await query
            .OrderByDescending(d => d.UploadedDate)
            .Select(d => new CaseDocumentDto
            {
                Id = d.Id,
                CaseId = d.CaseId,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                FileSize = d.FileSize,
                ContentType = d.ContentType,
                IsPrivate = d.IsPrivate,
                SharedWithAllLawyers = d.SharedWithAllLawyers,
                IsAvailableForDeal = d.IsAvailableForDeal,
                SharedWithLawyerIds = d.LawyerShares.Select(s => s.LawyerProfileId).ToList(),
                UploadedDate = d.UploadedDate,
                UploadedByName = d.UploadedBy.FirstName + " " + d.UploadedBy.LastName,
                UploadedByRole = d.UploadedByRole
            })
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, CaseDocumentDto? Data)> UploadDocumentAsync(
        int userId, string role, string userName, int caseId, IFormFile file, UploadDocumentDto dto)
    {
        if (!await CanAccessCaseAsync(userId, role, caseId))
            return (false, "Case not found or access denied.", null);

        // Staff must have CanUploadDocument permission
        if (role == "Staff")
        {
            var staffProfile = await _db.StaffProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
            var allowed = staffProfile != null && await _db.CaseStaffs
                .AnyAsync(cs => cs.CaseId == caseId && cs.StaffProfileId == staffProfile.Id
                             && cs.IsActive && cs.CanUploadDocument);
            if (!allowed) return (false, "You do not have permission to upload documents to this case.", null);
        }

        var (isValid, error) = _fileValidation.Validate(file);
        if (!isValid)
            return (false, error, null);

        var (storedFileName, filePath) = await _fileUpload.SaveFileAsync(file, caseId);

        bool sharedWithAll = dto.SharedWithLawyerIds.Count == 0;

        // IsAvailableForDeal only makes sense for non-private client-uploaded docs
        bool isAvailableForDeal = !dto.IsPrivate && role == "Client" && dto.IsAvailableForDeal;

        var document = new CaseDocument
        {
            CaseId = caseId,
            UploadedByUserId = userId,
            UploadedByRole = role,
            DocumentType = dto.DocumentType,
            FileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            FilePath = filePath,
            FileSize = file.Length,
            ContentType = file.ContentType,
            IsPrivate = dto.IsPrivate,
            SharedWithAllLawyers = dto.IsPrivate ? true : sharedWithAll,
            IsAvailableForDeal = isAvailableForDeal
        };

        _db.CaseDocuments.Add(document);
        await _db.SaveChangesAsync();

        // Add specific lawyer shares when not sharing with all
        if (!dto.IsPrivate && !sharedWithAll && dto.SharedWithLawyerIds.Count > 0)
        {
            foreach (var lawyerProfileId in dto.SharedWithLawyerIds.Distinct())
            {
                _db.CaseDocumentLawyerShares.Add(new CaseDocumentLawyerShare
                {
                    CaseDocumentId = document.Id,
                    LawyerProfileId = lawyerProfileId
                });
            }
        }

        _db.CaseActivities.Add(new CaseActivity
        {
            CaseId = caseId,
            CreatedByUserId = userId,
            ActivityType = ActivityType.DocumentUpload,
            Description = $"Document uploaded: {file.FileName} ({dto.DocumentType})",
            ActivityDate = DateTime.UtcNow,
            CreatedByRole = role,
            CreatedByName = userName
        });

        var caseEntity = await _db.Cases.FindAsync(caseId);
        if (caseEntity != null) caseEntity.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return (true, "Document uploaded successfully.", new CaseDocumentDto
        {
            Id = document.Id,
            CaseId = document.CaseId,
            DocumentType = document.DocumentType,
            FileName = document.FileName,
            FileSize = document.FileSize,
            ContentType = document.ContentType,
            IsPrivate = document.IsPrivate,
            SharedWithAllLawyers = document.SharedWithAllLawyers,
            IsAvailableForDeal = document.IsAvailableForDeal,
            SharedWithLawyerIds = dto.SharedWithLawyerIds,
            UploadedDate = document.UploadedDate,
            UploadedByName = userName
        });
    }

    public async Task<(CaseDocument? Document, bool CanAccess)> GetDocumentForDownloadAsync(int userId, string role, int documentId)
    {
        var document = await _db.CaseDocuments
            .Include(d => d.LawyerShares)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document == null) return (null, false);

        if (!await CanAccessCaseAsync(userId, role, document.CaseId))
            return (document, false);

        if (role == "Client")
        {
            // Client can only download: their own docs OR lawyer's public docs
            if (document.UploadedByRole == "Lawyer" && document.IsPrivate)
                return (document, false);
            if (document.UploadedByRole == "Client" && document.UploadedByUserId != userId)
                return (document, false);
        }
        else if (role == "Lawyer")
        {
            var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            if (lawyerProfile == null) return (document, false);

            // Lawyer cannot download other lawyers' private docs
            if (document.UploadedByRole == "Lawyer" && document.IsPrivate && document.UploadedByUserId != userId)
                return (document, false);

            // Lawyer cannot download client private docs
            if (document.UploadedByRole == "Client" && document.IsPrivate)
                return (document, false);

            // Lawyer can only download client public docs if shared with them
            if (document.UploadedByRole == "Client" && !document.IsPrivate &&
                !document.SharedWithAllLawyers &&
                !document.LawyerShares.Any(s => s.LawyerProfileId == lawyerProfile.Id))
                return (document, false);
        }

        var user = await _db.Users.FindAsync(userId);
        _db.CaseActivities.Add(new CaseActivity
        {
            CaseId = document.CaseId,
            CreatedByUserId = userId,
            ActivityType = ActivityType.DocumentDownload,
            Description = $"Document downloaded: {document.FileName}",
            ActivityDate = DateTime.UtcNow,
            CreatedByRole = role,
            CreatedByName = user?.FullName ?? "Unknown"
        });
        await _db.SaveChangesAsync();

        return (document, true);
    }

    public async Task<(bool Success, string Message)> DeleteDocumentAsync(int userId, string role, int documentId)
    {
        var document = await _db.CaseDocuments.FirstOrDefaultAsync(d => d.Id == documentId);
        if (document == null) return (false, "Document not found.");

        var canDelete = document.UploadedByUserId == userId;
        if (!canDelete && role == "Lawyer")
        {
            var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            canDelete = lawyerProfile != null && await _db.Cases.AnyAsync(c =>
                c.Id == document.CaseId &&
                (c.LawyerProfileId == lawyerProfile.Id ||
                 c.CaseLawyers.Any(cl => cl.LawyerProfileId == lawyerProfile.Id && cl.IsActive)));
        }

        if (!canDelete) return (false, "Access denied.");

        document.IsDeleted = true;
        await _fileUpload.DeleteFileAsync(document.FilePath);
        await _db.SaveChangesAsync();
        return (true, "Document deleted.");
    }

    public async Task<(bool Success, string Message, bool NewValue)> ToggleAvailableForDealAsync(
        int clientUserId, int documentId)
    {
        var doc = await _db.CaseDocuments.FirstOrDefaultAsync(d => d.Id == documentId);
        if (doc == null) return (false, "Document not found.", false);

        // Only the client who uploaded it can toggle this flag
        if (doc.UploadedByRole != "Client" || doc.UploadedByUserId != clientUserId)
            return (false, "Only the client who uploaded this document can change this setting.", false);

        // Cannot mark a private document as available for deal
        if (doc.IsPrivate && !doc.IsAvailableForDeal)
            return (false, "Private documents cannot be shared for hire requests.", false);

        doc.IsAvailableForDeal = !doc.IsAvailableForDeal;
        await _db.SaveChangesAsync();
        return (true, doc.IsAvailableForDeal
            ? "Document will now be visible to lawyers evaluating this case via hire requests."
            : "Document removed from hire request visibility.", doc.IsAvailableForDeal);
    }

    public async Task<(bool Success, string Message, bool NewValue)> TogglePrivateAsync(int userId, int documentId)
    {
        var doc = await _db.CaseDocuments.FirstOrDefaultAsync(d => d.Id == documentId);
        if (doc == null) return (false, "Document not found.", false);

        // Only the uploader can change their own document visibility
        if (doc.UploadedByUserId != userId)
            return (false, "Only the uploader can change document visibility.", false);

        doc.IsPrivate = !doc.IsPrivate;

        // If marking private, also remove from deal share to keep state consistent
        if (doc.IsPrivate) doc.IsAvailableForDeal = false;

        await _db.SaveChangesAsync();
        return (true,
            doc.IsPrivate ? "Document is now private." : "Document is now public.",
            doc.IsPrivate);
    }

    public async Task<(CaseDocument? Doc, bool CanAccess)> GetDealDocumentViaHireRequestAsync(
        int userId, string role, int hireRequestId, int caseDocId)
    {
        // Find the hire request — must be linked to a case
        var hr = await _db.HireRequests
            .FirstOrDefaultAsync(h => h.Id == hireRequestId && h.LinkedCaseId != null);
        if (hr == null) return (null, false);

        // Verify the requestor is the assigned lawyer OR the client owner
        if (role == "Lawyer")
        {
            var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            if (lawyerProfile == null || hr.LawyerProfileId != lawyerProfile.Id) return (null, false);
        }
        else if (role == "Client")
        {
            var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (clientProfile == null || hr.ClientProfileId != clientProfile.Id) return (null, false);
        }
        else return (null, false);

        // Fetch the case document; it must belong to the linked case and be flagged
        var doc = await _db.CaseDocuments.FirstOrDefaultAsync(d =>
            d.Id == caseDocId &&
            d.CaseId == hr.LinkedCaseId &&
            d.IsAvailableForDeal &&
            !d.IsDeleted);

        return doc == null ? (null, false) : (doc, true);
    }

    private async Task<bool> CanAccessCaseAsync(int userId, string role, int caseId)
    {
        if (role == "Lawyer")
        {
            var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            return lawyerProfile != null && await _db.Cases.AnyAsync(c =>
                c.Id == caseId &&
                (c.LawyerProfileId == lawyerProfile.Id ||
                 c.CaseLawyers.Any(cl => cl.LawyerProfileId == lawyerProfile.Id && cl.IsActive)));
        }
        if (role == "Staff")
        {
            var staffProfile = await _db.StaffProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
            return staffProfile != null && await _db.Cases.AnyAsync(c =>
                c.Id == caseId &&
                c.CaseStaffs.Any(cs => cs.StaffProfileId == staffProfile.Id && cs.IsActive));
        }
        if (role == "Client")
        {
            var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            return clientProfile != null && await _db.Cases.AnyAsync(c => c.Id == caseId && c.ClientProfileId == clientProfile.Id);
        }
        return role == "Admin";
    }
}