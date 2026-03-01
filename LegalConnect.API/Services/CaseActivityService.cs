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

        return await _db.CaseActivities
            .Where(a => a.CaseId == caseId)
            .OrderByDescending(a => a.ActivityDate)
            .Select(a => new CaseActivityDto
            {
                Id = a.Id,
                CaseId = a.CaseId,
                ActivityType = a.ActivityType,
                Description = a.Description,
                ActivityDate = a.ActivityDate,
                CreatedByName = a.CreatedByName,
                CreatedByRole = a.CreatedByRole
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, CaseActivityDto? Data)> AddActivityAsync(
        int userId, string role, string userName, int caseId, AddCaseActivityDto dto)
    {
        if (!await CanAccessCaseAsync(userId, role, caseId))
            return (false, "Case not found or access denied.", null);

        // Only lawyer can add StatusChange or CaseClosed activities
        if ((dto.ActivityType == ActivityType.StatusChange || dto.ActivityType == ActivityType.CaseClosed)
            && role != "Lawyer")
            return (false, "Only the lawyer can perform this activity.", null);

        var activity = new CaseActivity
        {
            CaseId = caseId,
            CreatedByUserId = userId,
            ActivityType = dto.ActivityType,
            Description = dto.Description,
            ActivityDate = dto.ActivityDate ?? DateTime.UtcNow,
            CreatedByRole = role,
            CreatedByName = userName
        };

        _db.CaseActivities.Add(activity);

        // Update case modified date
        var caseEntity = await _db.Cases.FindAsync(caseId);
        if (caseEntity != null) caseEntity.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return (true, "Activity added.", new CaseActivityDto
        {
            Id = activity.Id,
            CaseId = activity.CaseId,
            ActivityType = activity.ActivityType,
            Description = activity.Description,
            ActivityDate = activity.ActivityDate,
            CreatedByName = activity.CreatedByName,
            CreatedByRole = activity.CreatedByRole
        });
    }

    private async Task<bool> CanAccessCaseAsync(int userId, string role, int caseId)
    {
        if (role == "Lawyer")
        {
            var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            return lawyerProfile != null && await _db.Cases.AnyAsync(c => c.Id == caseId && c.LawyerProfileId == lawyerProfile.Id);
        }
        if (role == "Client")
        {
            var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            return clientProfile != null && await _db.Cases.AnyAsync(c => c.Id == caseId && c.ClientProfileId == clientProfile.Id);
        }
        return role == "Admin";
    }
}
