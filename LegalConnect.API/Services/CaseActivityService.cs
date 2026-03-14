using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Cases;
using LegalConnect.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface ICaseActivityService
{
    Task<List<CaseActivityDto>> GetActivitiesAsync(int userId, string role, int caseId);
    Task<(bool Success, string Message, CaseActivityDto? Data)> AddActivityAsync(
        int userId, string role, string userName, int caseId, AddCaseActivityDto dto);
    Task<(bool Success, string Message)> LinkDocumentAsync(int userId, string role, int caseId, int activityId, int documentId);
}

public class CaseActivityService : ICaseActivityService
{
    private readonly AppDbContext _db;

    public CaseActivityService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CaseActivityDto>> GetActivitiesAsync(int userId, string role, int caseId)
    {
        if (!await CanAccessCaseAsync(userId, role, caseId))
            return [];

        var activities = await _db.CaseActivities
            .Where(a => a.CaseId == caseId)
            .Include(a => a.LinkedDocuments)
                .ThenInclude(d => d.CaseDocument)
            .OrderByDescending(a => a.ActivityDate)
            .ToListAsync();

        return activities.Select(a => new CaseActivityDto
        {
            Id = a.Id,
            CaseId = a.CaseId,
            ActivityType = a.ActivityType,
            Title = a.Title,
            Description = a.Description,
            Category = a.Category,
            ActivityDate = a.ActivityDate,
            EventDate = a.EventDate,
            CreatedByName = a.CreatedByName,
            CreatedByRole = a.CreatedByRole,
            LinkedDocuments = a.LinkedDocuments
                .Where(d => d.CaseDocument != null)
                .Select(d => new LinkedDocumentDto
                {
                    DocumentId = d.CaseDocumentId,
                    FileName = d.CaseDocument.FileName,
                    DocumentType = d.CaseDocument.DocumentType,
                    FileSize = d.CaseDocument.FileSize
                }).ToList()
        }).ToList();
    }

    public async Task<(bool Success, string Message, CaseActivityDto? Data)> AddActivityAsync(
        int userId, string role, string userName, int caseId, AddCaseActivityDto dto)
    {
        if (!await CanAccessCaseAsync(userId, role, caseId))
            return (false, "Case not found or access denied.", null);

        if ((dto.ActivityType == ActivityType.StatusChange || dto.ActivityType == ActivityType.CaseClosed)
            && role != "Lawyer")
            return (false, "Only the lawyer can perform this activity.", null);

        // Staff must have CanAddActivity permission
        if (role == "Staff")
        {
            var staffProfile = await _db.StaffProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
            var allowed = staffProfile != null && await _db.CaseStaffs
                .AnyAsync(cs => cs.CaseId == caseId && cs.StaffProfileId == staffProfile.Id
                             && cs.IsActive && cs.CanAddActivity);
            if (!allowed) return (false, "You do not have permission to add activities to this case.", null);
        }

        var activity = new CaseActivity
        {
            CaseId = caseId,
            CreatedByUserId = userId,
            ActivityType = dto.ActivityType,
            Title = dto.Title ?? string.Empty,
            Description = dto.Description,
            Category = string.IsNullOrWhiteSpace(dto.Category) ? ActivityCategory.Other : dto.Category,
            ActivityDate = dto.ActivityDate ?? DateTime.UtcNow,
            EventDate = dto.EventDate,
            CreatedByRole = role,
            CreatedByName = userName
        };

        _db.CaseActivities.Add(activity);
        await _db.SaveChangesAsync(); // save first to get activity.Id

        if (dto.LinkedDocumentIds.Count > 0)
        {
            var validDocIds = await _db.CaseDocuments
                .Where(d => d.CaseId == caseId && dto.LinkedDocumentIds.Contains(d.Id))
                .Select(d => d.Id)
                .ToListAsync();

            foreach (var docId in validDocIds)
                _db.CaseActivityDocuments.Add(new CaseActivityDocument
                {
                    CaseActivityId = activity.Id,
                    CaseDocumentId = docId
                });
        }

        var caseEntity = await _db.Cases.FindAsync(caseId);
        if (caseEntity != null) caseEntity.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return (true, "Activity added.", new CaseActivityDto
        {
            Id = activity.Id,
            CaseId = activity.CaseId,
            ActivityType = activity.ActivityType,
            Title = activity.Title,
            Description = activity.Description,
            Category = activity.Category,
            ActivityDate = activity.ActivityDate,
            EventDate = activity.EventDate,
            CreatedByName = activity.CreatedByName,
            CreatedByRole = activity.CreatedByRole,
            LinkedDocuments = []
        });
    }

    public async Task<(bool Success, string Message)> LinkDocumentAsync(int userId, string role, int caseId, int activityId, int documentId)
    {
        if (!await CanAccessCaseAsync(userId, role, caseId))
            return (false, "Case not found or access denied.");

        var activity = await _db.CaseActivities.FirstOrDefaultAsync(a => a.Id == activityId && a.CaseId == caseId);
        if (activity == null) return (false, "Activity not found.");

        var document = await _db.CaseDocuments.FirstOrDefaultAsync(d => d.Id == documentId && d.CaseId == caseId);
        if (document == null) return (false, "Document not found.");

        var alreadyLinked = await _db.CaseActivityDocuments
            .AnyAsync(d => d.CaseActivityId == activityId && d.CaseDocumentId == documentId);
        if (alreadyLinked) return (true, "Already linked.");

        _db.CaseActivityDocuments.Add(new CaseActivityDocument
        {
            CaseActivityId = activityId,
            CaseDocumentId = documentId
        });
        await _db.SaveChangesAsync();
        return (true, "Document linked to activity.");
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
