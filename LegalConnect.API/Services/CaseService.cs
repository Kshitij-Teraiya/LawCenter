using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Cases;
using LegalConnect.API.Entities;
using LegalConnect.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface ICaseService
{
    Task<PagedResult<CaseSummaryDto>> GetCasesAsync(int userId, string role, CaseFilterDto filter);
    Task<CaseDto?> GetCaseByIdAsync(int userId, string role, int caseId);
    Task<(bool Success, string Message, CaseDto? Data)> CreateCaseAsync(int userId, string role, CreateCaseDto dto);
    Task<(bool Success, string Message)> UpdateCaseAsync(int lawyerUserId, int caseId, UpdateCaseDto dto);
    Task<(bool Success, string Message)> CloseCaseAsync(int lawyerUserId, int caseId, CloseCaseDto dto);
    Task<(bool Success, string Message)> UpdateStatusAsync(int lawyerUserId, int caseId, CaseStatus status);
    Task<(bool Success, string Message)> SoftDeleteAsync(int lawyerUserId, int caseId);
    Task<List<HiredLawyerDto>> GetMyLawyersAsync(int clientUserId);
    Task<List<CaseLawyerDto>> GetCaseLawyersAsync(int userId, string role, int caseId);
    Task<(bool Success, string Message)> AddLawyerToCaseAsync(int clientUserId, int caseId, int lawyerProfileId);
    Task<(bool Success, string Message)> RemoveLawyerFromCaseAsync(int clientUserId, int caseId, int lawyerProfileId);
}

public class CaseService : ICaseService
{
    private readonly AppDbContext _db;

    public CaseService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<CaseSummaryDto>> GetCasesAsync(int userId, string role, CaseFilterDto filter)
    {
        IQueryable<Case> query;

        if (role == "Lawyer")
        {
            var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            if (lawyerProfile == null)
                return new PagedResult<CaseSummaryDto> { PageNumber = filter.PageNumber, PageSize = filter.PageSize };

            query = _db.Cases
                .Include(c => c.LawyerProfile).ThenInclude(l => l!.User)
                .Include(c => c.ClientProfile).ThenInclude(cp => cp.User)
                .Include(c => c.Documents)
                .Include(c => c.Activities)
                .Include(c => c.CaseLawyers)
                // Cases where this lawyer is directly assigned OR is in CaseLawyers
                .Where(c => c.LawyerProfileId == lawyerProfile.Id ||
                            c.CaseLawyers.Any(cl => cl.LawyerProfileId == lawyerProfile.Id && cl.IsActive));
        }
        else // Client
        {
            var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (clientProfile == null)
                return new PagedResult<CaseSummaryDto> { PageNumber = filter.PageNumber, PageSize = filter.PageSize };

            query = _db.Cases
                .Include(c => c.LawyerProfile).ThenInclude(l => l!.User)
                .Include(c => c.ClientProfile).ThenInclude(cp => cp.User)
                .Include(c => c.Documents)
                .Include(c => c.Activities)
                .Include(c => c.CaseLawyers)
                .Where(c => c.ClientProfileId == clientProfile.Id);
        }

        if (filter.Status.HasValue)
            query = query.Where(c => c.Status == filter.Status.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(c =>
                c.CaseTitle.ToLower().Contains(term) ||
                (c.CaseNumber != null && c.CaseNumber.ToLower().Contains(term)) ||
                c.CaseType.ToLower().Contains(term) ||
                c.Court.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.CaseType))
            query = query.Where(c => c.CaseType.ToLower() == filter.CaseType.ToLower());

        if (!string.IsNullOrWhiteSpace(filter.Court))
            query = query.Where(c => c.Court.ToLower().Contains(filter.Court.ToLower()));

        if (filter.ClientProfileId.HasValue && role == "Lawyer")
            query = query.Where(c => c.ClientProfileId == filter.ClientProfileId.Value);

        if (filter.FilingDateFrom.HasValue)
            query = query.Where(c => c.FilingDate >= filter.FilingDateFrom.Value);

        if (filter.FilingDateTo.HasValue)
            query = query.Where(c => c.FilingDate <= filter.FilingDateTo.Value);

        query = filter.SortBy.ToLower() switch
        {
            "title" => filter.SortDescending ? query.OrderByDescending(c => c.CaseTitle) : query.OrderBy(c => c.CaseTitle),
            "filingdate" => filter.SortDescending ? query.OrderByDescending(c => c.FilingDate) : query.OrderBy(c => c.FilingDate),
            "status" => filter.SortDescending ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
            _ => filter.SortDescending ? query.OrderByDescending(c => c.ModifiedDate) : query.OrderBy(c => c.ModifiedDate)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => MapToSummary(c))
            .ToListAsync();

        return new PagedResult<CaseSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<CaseDto?> GetCaseByIdAsync(int userId, string role, int caseId)
    {
        var query = _db.Cases
            .Include(c => c.LawyerProfile).ThenInclude(l => l!.User)
            .Include(c => c.ClientProfile).ThenInclude(cp => cp.User)
            .Include(c => c.CaseLawyers).ThenInclude(cl => cl.LawyerProfile).ThenInclude(l => l.User)
            .Include(c => c.CaseLawyers).ThenInclude(cl => cl.LawyerProfile).ThenInclude(l => l.Category)
            .Include(c => c.Activities.OrderByDescending(a => a.ActivityDate).Take(20))
            .Include(c => c.Documents.Where(d => !d.IsDeleted))
                .ThenInclude(d => d.UploadedBy)
            .Include(c => c.Documents.Where(d => !d.IsDeleted))
                .ThenInclude(d => d.LawyerShares)
            .AsQueryable();

        Case? caseEntity;
        LawyerProfile? lawyerProfile = null;

        if (role == "Lawyer")
        {
            lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            if (lawyerProfile == null) return null;
            caseEntity = await query.FirstOrDefaultAsync(c =>
                c.Id == caseId &&
                (c.LawyerProfileId == lawyerProfile.Id ||
                 c.CaseLawyers.Any(cl => cl.LawyerProfileId == lawyerProfile.Id && cl.IsActive)));
        }
        else if (role == "Client")
        {
            var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (clientProfile == null) return null;
            caseEntity = await query.FirstOrDefaultAsync(c => c.Id == caseId && c.ClientProfileId == clientProfile.Id);
        }
        else
        {
            caseEntity = await query.FirstOrDefaultAsync(c => c.Id == caseId);
        }

        if (caseEntity == null) return null;

        var dto = MapToDto(caseEntity);

        // Apply role-aware document visibility
        if (role == "Client")
        {
            dto.Documents = dto.Documents
                .Where(d => !(d.IsPrivate && d.UploadedByRole == "Lawyer"))
                .ToList();
        }
        else if (role == "Lawyer" && lawyerProfile != null)
        {
            dto.Documents = caseEntity.Documents
                .Where(d => !d.IsDeleted)
                .Where(d =>
                    // Lawyer's own docs (even private)
                    (d.UploadedByRole == "Lawyer" && (!d.IsPrivate || d.UploadedByUserId == userId)) ||
                    // Client's public docs visible to this lawyer
                    (d.UploadedByRole == "Client" && !d.IsPrivate &&
                     (d.SharedWithAllLawyers || d.LawyerShares.Any(s => s.LawyerProfileId == lawyerProfile.Id)))
                )
                .Select(d => new CaseDocumentDto
                {
                    Id = d.Id, CaseId = d.CaseId, DocumentType = d.DocumentType, FileName = d.FileName,
                    FileSize = d.FileSize, ContentType = d.ContentType, IsPrivate = d.IsPrivate,
                    SharedWithAllLawyers = d.SharedWithAllLawyers,
                    SharedWithLawyerIds = d.LawyerShares.Select(s => s.LawyerProfileId).ToList(),
                    UploadedDate = d.UploadedDate,
                    UploadedByName = d.UploadedBy?.FirstName + " " + d.UploadedBy?.LastName,
                    UploadedByRole = d.UploadedByRole
                }).ToList();
        }

        return dto;
    }

    public async Task<(bool Success, string Message, CaseDto? Data)> CreateCaseAsync(int userId, string role, CreateCaseDto dto)
    {
        if (role == "Client")
        {
            var clientProfile = await _db.ClientProfiles
                .Include(cp => cp.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (clientProfile == null)
                return (false, "Client profile not found.", null);

            var newCase = new Case
            {
                ClientProfileId = clientProfile.Id,
                LawyerProfileId = dto.LawyerProfileIds.Count > 0 ? dto.LawyerProfileIds[0] : null,
                AppointmentId = dto.AppointmentId,
                CaseTitle = dto.CaseTitle,
                CaseNumber = dto.CaseNumber,
                CaseType = dto.CaseType,
                Court = dto.Court,
                FilingDate = dto.FilingDate,
                NextHearingDate = dto.NextHearingDate,
                Status = CaseStatus.Open,
                Description = dto.Description
            };

            _db.Cases.Add(newCase);
            await _db.SaveChangesAsync();

            // Add selected lawyers to CaseLawyers junction
            foreach (var lawyerProfileId in dto.LawyerProfileIds.Distinct())
            {
                var lawyerExists = await _db.LawyerProfiles.AnyAsync(l => l.Id == lawyerProfileId);
                if (lawyerExists)
                {
                    _db.CaseLawyers.Add(new CaseLawyer
                    {
                        CaseId = newCase.Id,
                        LawyerProfileId = lawyerProfileId,
                        AddedByRole = "Client",
                        AddedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                }
            }

            _db.CaseActivities.Add(new CaseActivity
            {
                CaseId = newCase.Id,
                CreatedByUserId = userId,
                ActivityType = ActivityType.CaseCreated,
                Description = dto.LawyerProfileIds.Count > 0
                    ? $"Case '{dto.CaseTitle}' was opened by the client."
                    : $"Case '{dto.CaseTitle}' was opened by the client (no lawyer assigned yet).",
                ActivityDate = DateTime.UtcNow,
                CreatedByRole = "Client",
                CreatedByName = clientProfile.User.FullName
            });
            await _db.SaveChangesAsync();

            var result = await GetCaseByIdAsync(userId, "Client", newCase.Id);
            return (true, "Case created successfully.", result);
        }
        else
        {
            var lawyerProfile = await _db.LawyerProfiles
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.UserId == userId);
            if (lawyerProfile == null)
                return (false, "Lawyer profile not found.", null);

            if (!dto.ClientProfileId.HasValue)
                return (false, "Please select a client.", null);

            var clientProfile = await _db.ClientProfiles
                .Include(cp => cp.User)
                .FirstOrDefaultAsync(cp => cp.Id == dto.ClientProfileId.Value);
            if (clientProfile == null)
                return (false, "Client not found.", null);

            var lawyerClient = await _db.LawyerClients
                .FirstOrDefaultAsync(lc =>
                    lc.LawyerProfileId == lawyerProfile.Id &&
                    lc.ClientProfileId == dto.ClientProfileId.Value &&
                    !lc.IsDeleted);

            var newCase = new Case
            {
                LawyerProfileId = lawyerProfile.Id,
                ClientProfileId = dto.ClientProfileId.Value,
                LawyerClientId = lawyerClient?.Id ?? dto.LawyerClientId,
                AppointmentId = dto.AppointmentId,
                CaseTitle = dto.CaseTitle,
                CaseNumber = dto.CaseNumber,
                CaseType = dto.CaseType,
                Court = dto.Court,
                FilingDate = dto.FilingDate,
                NextHearingDate = dto.NextHearingDate,
                Status = CaseStatus.Open,
                Description = dto.Description
            };

            _db.Cases.Add(newCase);
            await _db.SaveChangesAsync();

            // Add the creating lawyer to CaseLawyers
            _db.CaseLawyers.Add(new CaseLawyer
            {
                CaseId = newCase.Id,
                LawyerProfileId = lawyerProfile.Id,
                AddedByRole = "Lawyer",
                AddedAt = DateTime.UtcNow,
                IsActive = true
            });

            _db.CaseActivities.Add(new CaseActivity
            {
                CaseId = newCase.Id,
                CreatedByUserId = userId,
                ActivityType = ActivityType.CaseCreated,
                Description = $"Case '{dto.CaseTitle}' was created.",
                ActivityDate = DateTime.UtcNow,
                CreatedByRole = "Lawyer",
                CreatedByName = lawyerProfile.User.FullName
            });
            await _db.SaveChangesAsync();

            var result = await GetCaseByIdAsync(userId, "Lawyer", newCase.Id);
            return (true, "Case created successfully.", result);
        }
    }

    public async Task<List<CaseLawyerDto>> GetCaseLawyersAsync(int userId, string role, int caseId)
    {
        // Verify access
        bool hasAccess = false;
        if (role == "Lawyer")
        {
            var lp = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            hasAccess = lp != null && await _db.Cases.AnyAsync(c =>
                c.Id == caseId && (c.LawyerProfileId == lp.Id || c.CaseLawyers.Any(cl => cl.LawyerProfileId == lp.Id)));
        }
        else if (role == "Client")
        {
            var cp = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            hasAccess = cp != null && await _db.Cases.AnyAsync(c => c.Id == caseId && c.ClientProfileId == cp.Id);
        }
        if (!hasAccess) return [];

        return await _db.CaseLawyers
            .Include(cl => cl.LawyerProfile).ThenInclude(l => l.User)
            .Include(cl => cl.LawyerProfile).ThenInclude(l => l.Category)
            .Where(cl => cl.CaseId == caseId && cl.IsActive)
            .Select(cl => new CaseLawyerDto
            {
                LawyerProfileId = cl.LawyerProfileId,
                LawyerFullName = cl.LawyerProfile.User.FirstName + " " + cl.LawyerProfile.User.LastName,
                ProfilePictureUrl = cl.LawyerProfile.User.ProfilePictureUrl,
                City = cl.LawyerProfile.City,
                CategoryName = cl.LawyerProfile.Category != null ? cl.LawyerProfile.Category.Name : null,
                AddedAt = cl.AddedAt,
                AddedByRole = cl.AddedByRole
            }).ToListAsync();
    }

    public async Task<(bool Success, string Message)> AddLawyerToCaseAsync(int clientUserId, int caseId, int lawyerProfileId)
    {
        var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return (false, "Client profile not found.");

        var caseEntity = await _db.Cases.FirstOrDefaultAsync(c => c.Id == caseId && c.ClientProfileId == clientProfile.Id);
        if (caseEntity == null) return (false, "Case not found.");

        // Verify the client has a relationship with this lawyer
        var hasRelationship = await _db.LawyerClients.AnyAsync(lc =>
            lc.LawyerProfileId == lawyerProfileId && lc.ClientProfileId == clientProfile.Id && lc.IsActive && !lc.IsDeleted);
        if (!hasRelationship) return (false, "You can only add lawyers from your accepted lawyer list.");

        var existing = await _db.CaseLawyers.IgnoreQueryFilters()
            .FirstOrDefaultAsync(cl => cl.CaseId == caseId && cl.LawyerProfileId == lawyerProfileId);
        if (existing != null)
        {
            if (existing.IsActive) return (false, "This lawyer is already assigned to the case.");
            existing.IsActive = true;
            existing.AddedAt = DateTime.UtcNow;
        }
        else
        {
            _db.CaseLawyers.Add(new CaseLawyer
            {
                CaseId = caseId,
                LawyerProfileId = lawyerProfileId,
                AddedByRole = "Client",
                AddedAt = DateTime.UtcNow,
                IsActive = true
            });
            // Set as primary if no primary yet
            if (caseEntity.LawyerProfileId == null)
                caseEntity.LawyerProfileId = lawyerProfileId;
        }

        await _db.SaveChangesAsync();
        return (true, "Lawyer added to case.");
    }

    public async Task<(bool Success, string Message)> RemoveLawyerFromCaseAsync(int clientUserId, int caseId, int lawyerProfileId)
    {
        var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return (false, "Client profile not found.");

        var caseEntity = await _db.Cases.FirstOrDefaultAsync(c => c.Id == caseId && c.ClientProfileId == clientProfile.Id);
        if (caseEntity == null) return (false, "Case not found.");

        var caseLawyer = await _db.CaseLawyers.FirstOrDefaultAsync(cl => cl.CaseId == caseId && cl.LawyerProfileId == lawyerProfileId);
        if (caseLawyer == null) return (false, "Lawyer not assigned to this case.");

        caseLawyer.IsActive = false;
        if (caseEntity.LawyerProfileId == lawyerProfileId)
        {
            var nextLawyer = await _db.CaseLawyers
                .Where(cl => cl.CaseId == caseId && cl.LawyerProfileId != lawyerProfileId && cl.IsActive)
                .FirstOrDefaultAsync();
            caseEntity.LawyerProfileId = nextLawyer?.LawyerProfileId;
        }

        await _db.SaveChangesAsync();
        return (true, "Lawyer removed from case.");
    }

    public async Task<List<HiredLawyerDto>> GetMyLawyersAsync(int clientUserId)
    {
        var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return [];

        return await _db.LawyerClients
            .Include(lc => lc.LawyerProfile).ThenInclude(l => l.User)
            .Include(lc => lc.LawyerProfile).ThenInclude(l => l.Category)
            .Where(lc => lc.ClientProfileId == clientProfile.Id && lc.IsActive && !lc.IsDeleted)
            .Select(lc => new HiredLawyerDto
            {
                LawyerProfileId = lc.LawyerProfileId,
                LawyerClientId = lc.Id,
                LawyerFullName = lc.LawyerProfile.User.FirstName + " " + lc.LawyerProfile.User.LastName,
                ProfilePictureUrl = lc.LawyerProfile.User.ProfilePictureUrl,
                City = lc.LawyerProfile.City,
                Court = lc.LawyerProfile.Court,
                CategoryName = lc.LawyerProfile.Category != null ? lc.LawyerProfile.Category.Name : null
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> UpdateCaseAsync(int lawyerUserId, int caseId, UpdateCaseDto dto)
    {
        var (caseEntity, lawyerProfile) = await GetLawyerCaseAsync(lawyerUserId, caseId);
        if (caseEntity == null) return (false, "Case not found.");
        if (caseEntity.Status == CaseStatus.Closed) return (false, "Cannot edit a closed case.");

        var oldStatus = caseEntity.Status;
        caseEntity.CaseTitle = dto.CaseTitle;
        caseEntity.CaseNumber = dto.CaseNumber;
        caseEntity.CaseType = dto.CaseType;
        caseEntity.Court = dto.Court;
        caseEntity.FilingDate = dto.FilingDate;
        caseEntity.NextHearingDate = dto.NextHearingDate;
        caseEntity.Status = dto.Status;
        caseEntity.Description = dto.Description;
        caseEntity.Outcome = dto.Outcome;
        caseEntity.ModifiedDate = DateTime.UtcNow;

        if (oldStatus != dto.Status)
        {
            _db.CaseActivities.Add(new CaseActivity
            {
                CaseId = caseId,
                CreatedByUserId = lawyerUserId,
                ActivityType = ActivityType.StatusChange,
                Description = $"Status changed from '{oldStatus}' to '{dto.Status}'.",
                ActivityDate = DateTime.UtcNow,
                CreatedByRole = "Lawyer",
                CreatedByName = lawyerProfile!.User.FullName
            });
        }

        await _db.SaveChangesAsync();
        return (true, "Case updated successfully.");
    }

    public async Task<(bool Success, string Message)> CloseCaseAsync(int lawyerUserId, int caseId, CloseCaseDto dto)
    {
        var (caseEntity, lawyerProfile) = await GetLawyerCaseAsync(lawyerUserId, caseId);
        if (caseEntity == null) return (false, "Case not found.");
        if (caseEntity.Status == CaseStatus.Closed) return (false, "Case is already closed.");

        caseEntity.Status = CaseStatus.Closed;
        caseEntity.Outcome = dto.Outcome;
        caseEntity.ModifiedDate = DateTime.UtcNow;

        _db.CaseActivities.Add(new CaseActivity
        {
            CaseId = caseId,
            CreatedByUserId = lawyerUserId,
            ActivityType = ActivityType.CaseClosed,
            Description = $"Case closed. Outcome: {dto.Outcome}",
            ActivityDate = DateTime.UtcNow,
            CreatedByRole = "Lawyer",
            CreatedByName = lawyerProfile!.User.FullName
        });

        await _db.SaveChangesAsync();
        return (true, "Case closed successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateStatusAsync(int lawyerUserId, int caseId, CaseStatus status)
    {
        var (caseEntity, lawyerProfile) = await GetLawyerCaseAsync(lawyerUserId, caseId);
        if (caseEntity == null) return (false, "Case not found.");

        var oldStatus = caseEntity.Status;
        caseEntity.Status = status;
        caseEntity.ModifiedDate = DateTime.UtcNow;

        _db.CaseActivities.Add(new CaseActivity
        {
            CaseId = caseId,
            CreatedByUserId = lawyerUserId,
            ActivityType = ActivityType.StatusChange,
            Description = $"Status changed from '{oldStatus}' to '{status}'.",
            ActivityDate = DateTime.UtcNow,
            CreatedByRole = "Lawyer",
            CreatedByName = lawyerProfile!.User.FullName
        });

        await _db.SaveChangesAsync();
        return (true, "Status updated.");
    }

    public async Task<(bool Success, string Message)> SoftDeleteAsync(int lawyerUserId, int caseId)
    {
        var (caseEntity, _) = await GetLawyerCaseAsync(lawyerUserId, caseId);
        if (caseEntity == null) return (false, "Case not found.");

        caseEntity.IsDeleted = true;
        caseEntity.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, "Case deleted.");
    }

    // ── Private helpers ─────────────────────────────────────────────────────────────

    private async Task<(Case? CaseEntity, LawyerProfile? LawyerProfile)> GetLawyerCaseAsync(int lawyerUserId, int caseId)
    {
        var lawyerProfile = await _db.LawyerProfiles
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return (null, null);

        var caseEntity = await _db.Cases.FirstOrDefaultAsync(c =>
            c.Id == caseId &&
            (c.LawyerProfileId == lawyerProfile.Id ||
             c.CaseLawyers.Any(cl => cl.LawyerProfileId == lawyerProfile.Id && cl.IsActive)));

        return (caseEntity, lawyerProfile);
    }

    private static CaseSummaryDto MapToSummary(Case c) => new()
    {
        Id = c.Id,
        CaseTitle = c.CaseTitle,
        CaseNumber = c.CaseNumber,
        CaseType = c.CaseType,
        Court = c.Court,
        Status = c.Status,
        FilingDate = c.FilingDate,
        NextHearingDate = c.NextHearingDate,
        LawyerName = c.LawyerProfile?.User?.FullName ?? (c.CaseLawyers.Count > 0 ? "Multiple Lawyers" : "No Lawyer"),
        ClientName = c.ClientProfile?.User?.FullName ?? "",
        CreatedDate = c.CreatedDate,
        ModifiedDate = c.ModifiedDate,
        DocumentCount = c.Documents?.Count(d => !d.IsDeleted) ?? 0,
        ActivityCount = c.Activities?.Count ?? 0,
        LawyerCount = c.CaseLawyers?.Count(cl => cl.IsActive) ?? 0
    };

    private static CaseDto MapToDto(Case c)
    {
        var dto = new CaseDto
        {
            Id = c.Id,
            LawyerProfileId = c.LawyerProfileId,
            ClientProfileId = c.ClientProfileId,
            AppointmentId = c.AppointmentId,
            CaseTitle = c.CaseTitle,
            CaseNumber = c.CaseNumber,
            CaseType = c.CaseType,
            Court = c.Court,
            Status = c.Status,
            FilingDate = c.FilingDate,
            NextHearingDate = c.NextHearingDate,
            Description = c.Description,
            Outcome = c.Outcome,
            LawyerName = c.LawyerProfile?.User?.FullName ?? (c.CaseLawyers.Count > 0 ? "Multiple Lawyers" : "No Lawyer Assigned"),
            ClientName = c.ClientProfile?.User?.FullName ?? "",
            LawyerProfilePicture = c.LawyerProfile?.User?.ProfilePictureUrl,
            ClientProfilePicture = c.ClientProfile?.User?.ProfilePictureUrl,
            CreatedDate = c.CreatedDate,
            ModifiedDate = c.ModifiedDate,
            DocumentCount = c.Documents?.Count(d => !d.IsDeleted) ?? 0,
            ActivityCount = c.Activities?.Count ?? 0,
            LawyerCount = c.CaseLawyers?.Count(cl => cl.IsActive) ?? 0,
            AssignedLawyers = c.CaseLawyers?.Where(cl => cl.IsActive).Select(cl => new CaseLawyerDto
            {
                LawyerProfileId = cl.LawyerProfileId,
                LawyerFullName = cl.LawyerProfile?.User?.FirstName + " " + cl.LawyerProfile?.User?.LastName,
                ProfilePictureUrl = cl.LawyerProfile?.User?.ProfilePictureUrl,
                City = cl.LawyerProfile?.City,
                CategoryName = cl.LawyerProfile?.Category?.Name,
                AddedAt = cl.AddedAt,
                AddedByRole = cl.AddedByRole
            }).ToList() ?? [],
            RecentActivities = c.Activities?.Select(a => new CaseActivityDto
            {
                Id = a.Id,
                CaseId = a.CaseId,
                ActivityType = a.ActivityType,
                Description = a.Description,
                ActivityDate = a.ActivityDate,
                CreatedByName = a.CreatedByName,
                CreatedByRole = a.CreatedByRole
            }).ToList() ?? [],
            Documents = c.Documents?.Where(d => !d.IsDeleted).Select(d => new CaseDocumentDto
            {
                Id = d.Id,
                CaseId = d.CaseId,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                FileSize = d.FileSize,
                ContentType = d.ContentType,
                IsPrivate = d.IsPrivate,
                SharedWithAllLawyers = d.SharedWithAllLawyers,
                SharedWithLawyerIds = d.LawyerShares?.Select(s => s.LawyerProfileId).ToList() ?? [],
                UploadedDate = d.UploadedDate,
                UploadedByName = d.UploadedBy?.FirstName + " " + d.UploadedBy?.LastName,
                UploadedByRole = d.UploadedByRole
            }).ToList() ?? []
        };
        return dto;
    }
}