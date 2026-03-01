using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Lawyer;
using LegalConnect.API.Entities;
using LegalConnect.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface ILawyerService
{
    Task<PagedResult<LawyerSummaryDto>> GetLawyersAsync(LawyerFilterDto filter);
    Task<LawyerDto?> GetLawyerByIdAsync(int lawyerProfileId);
    Task<List<string>> GetCitiesAsync();
    Task<List<string>> GetCourtsAsync();
    Task<LawyerDto?> GetMyProfileAsync(int userId);
    Task<(bool Success, string Message)> UpdateMyProfileAsync(int userId, UpdateLawyerProfileDto dto);
    Task<(bool Success, string Message)> AddExperienceAsync(int userId, AddExperienceDto dto);
    Task<(bool Success, string Message)> DeleteExperienceAsync(int userId, int experienceId);
    Task<(bool Success, string Message)> AddCaseResultAsync(int userId, AddCaseResultDto dto);
    Task<(bool Success, string Message)> DeleteCaseResultAsync(int userId, int caseResultId);
    Task<(bool Success, string Message)> SetServiceChargeAsync(int userId, SetServiceChargeDto dto);
}

public class LawyerService : ILawyerService
{
    private readonly AppDbContext _db;

    public LawyerService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<LawyerSummaryDto>> GetLawyersAsync(LawyerFilterDto filter)
    {
        var query = _db.LawyerProfiles
            .Include(l => l.User)
            .Include(l => l.Category)
            .Include(l => l.Reviews)
            .Where(l => l.IsVerified)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(l =>
                l.User.FirstName.ToLower().Contains(term) ||
                l.User.LastName.ToLower().Contains(term) ||
                l.City.ToLower().Contains(term) ||
                l.Court.ToLower().Contains(term) ||
                l.Category.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(l => l.City.ToLower() == filter.City.ToLower());

        if (filter.CategoryId.HasValue)
            query = query.Where(l => l.CategoryId == filter.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Court))
            query = query.Where(l => l.Court.ToLower().Contains(filter.Court.ToLower()));

        if (filter.MinExperience.HasValue)
            query = query.Where(l => l.YearsOfExperience >= filter.MinExperience.Value);

        if (filter.MaxExperience.HasValue)
            query = query.Where(l => l.YearsOfExperience <= filter.MaxExperience.Value);

        if (filter.MinFee.HasValue)
            query = query.Where(l => l.ConsultationFee >= filter.MinFee.Value);

        if (filter.MaxFee.HasValue)
            query = query.Where(l => l.ConsultationFee <= filter.MaxFee.Value);

        if (filter.IsAvailable.HasValue)
            query = query.Where(l => l.IsAvailable == filter.IsAvailable.Value);

        // Get all to calculate average ratings (EF Core limitation with computed properties)
        var allLawyers = await query.ToListAsync();

        if (filter.MinRating.HasValue)
            allLawyers = allLawyers.Where(l => l.AverageRating >= filter.MinRating.Value).ToList();

        // Sort
        allLawyers = filter.SortBy.ToLower() switch
        {
            "fee" => filter.SortDescending
                ? allLawyers.OrderByDescending(l => l.ConsultationFee).ToList()
                : allLawyers.OrderBy(l => l.ConsultationFee).ToList(),
            "experience" => filter.SortDescending
                ? allLawyers.OrderByDescending(l => l.YearsOfExperience).ToList()
                : allLawyers.OrderBy(l => l.YearsOfExperience).ToList(),
            "name" => filter.SortDescending
                ? allLawyers.OrderByDescending(l => l.User.FullName).ToList()
                : allLawyers.OrderBy(l => l.User.FullName).ToList(),
            _ => filter.SortDescending
                ? allLawyers.OrderByDescending(l => l.AverageRating).ToList()
                : allLawyers.OrderBy(l => l.AverageRating).ToList()
        };

        var totalCount = allLawyers.Count;
        var items = allLawyers
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(l => MapToSummary(l))
            .ToList();

        return new PagedResult<LawyerSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<LawyerDto?> GetLawyerByIdAsync(int lawyerProfileId)
    {
        var lawyer = await _db.LawyerProfiles
            .Include(l => l.User)
            .Include(l => l.Category)
            .Include(l => l.Experiences)
            .Include(l => l.CaseResults)
            .Include(l => l.Reviews).ThenInclude(r => r.ClientProfile).ThenInclude(cp => cp.User)
            .Include(l => l.Certifications)
            .Include(l => l.Publications)
            .FirstOrDefaultAsync(l => l.Id == lawyerProfileId && l.IsVerified);

        return lawyer == null ? null : MapToDto(lawyer);
    }

    public async Task<List<string>> GetCitiesAsync()
    {
        return await _db.LawyerProfiles
            .Where(l => l.IsVerified)
            .Select(l => l.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<List<string>> GetCourtsAsync()
    {
        return await _db.LawyerProfiles
            .Where(l => l.IsVerified)
            .Select(l => l.Court)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<LawyerDto?> GetMyProfileAsync(int userId)
    {
        var lawyer = await _db.LawyerProfiles
            .Include(l => l.User)
            .Include(l => l.Category)
            .Include(l => l.Experiences)
            .Include(l => l.CaseResults)
            .Include(l => l.Reviews).ThenInclude(r => r.ClientProfile).ThenInclude(cp => cp.User)
            .Include(l => l.Certifications)
            .Include(l => l.Publications)
            .FirstOrDefaultAsync(l => l.UserId == userId);

        return lawyer == null ? null : MapToDto(lawyer);
    }

    public async Task<(bool Success, string Message)> UpdateMyProfileAsync(int userId, UpdateLawyerProfileDto dto)
    {
        var lawyer = await _db.LawyerProfiles
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.UserId == userId);

        if (lawyer == null)
            return (false, "Lawyer profile not found.");

        lawyer.User.FirstName = dto.FirstName;
        lawyer.User.LastName = dto.LastName;
        lawyer.User.PhoneNumber = dto.PhoneNumber;
        lawyer.City = dto.City;
        lawyer.Court = dto.Court;
        lawyer.Bio = dto.Bio;
        lawyer.ConsultationFee = dto.ConsultationFee;
        lawyer.IsAvailable = dto.IsAvailable;

        await _db.SaveChangesAsync();
        return (true, "Profile updated successfully.");
    }

    public async Task<(bool Success, string Message)> AddExperienceAsync(int userId, AddExperienceDto dto)
    {
        var lawyer = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
        if (lawyer == null)
            return (false, "Lawyer profile not found.");

        var experience = new Experience
        {
            LawyerProfileId = lawyer.Id,
            Title = dto.Title,
            Organization = dto.Organization,
            StartDate = dto.StartDate,
            EndDate = dto.IsCurrent ? null : dto.EndDate,
            IsCurrent = dto.IsCurrent,
            Description = dto.Description
        };

        _db.Experiences.Add(experience);
        await _db.SaveChangesAsync();
        return (true, "Experience added successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteExperienceAsync(int userId, int experienceId)
    {
        var lawyer = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
        if (lawyer == null)
            return (false, "Lawyer profile not found.");

        var experience = await _db.Experiences
            .FirstOrDefaultAsync(e => e.Id == experienceId && e.LawyerProfileId == lawyer.Id);

        if (experience == null)
            return (false, "Experience not found.");

        _db.Experiences.Remove(experience);
        await _db.SaveChangesAsync();
        return (true, "Experience deleted.");
    }

    public async Task<(bool Success, string Message)> AddCaseResultAsync(int userId, AddCaseResultDto dto)
    {
        var lawyer = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
        if (lawyer == null)
            return (false, "Lawyer profile not found.");

        var caseResult = new CaseResult
        {
            LawyerProfileId = lawyer.Id,
            CaseTitle = dto.CaseTitle,
            Court = dto.Court,
            Outcome = dto.Outcome,
            Year = dto.Year,
            Description = dto.Description
        };

        _db.CaseResults.Add(caseResult);
        await _db.SaveChangesAsync();
        return (true, "Case result added successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteCaseResultAsync(int userId, int caseResultId)
    {
        var lawyer = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
        if (lawyer == null)
            return (false, "Lawyer profile not found.");

        var caseResult = await _db.CaseResults
            .FirstOrDefaultAsync(cr => cr.Id == caseResultId && cr.LawyerProfileId == lawyer.Id);

        if (caseResult == null)
            return (false, "Case result not found.");

        _db.CaseResults.Remove(caseResult);
        await _db.SaveChangesAsync();
        return (true, "Case result deleted.");
    }

    public async Task<(bool Success, string Message)> SetServiceChargeAsync(int userId, SetServiceChargeDto dto)
    {
        var lawyer = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
        if (lawyer == null)
            return (false, "Lawyer profile not found.");

        lawyer.ConsultationFee = dto.ConsultationFee;
        await _db.SaveChangesAsync();
        return (true, "Service charge updated.");
    }

    private static LawyerSummaryDto MapToSummary(LawyerProfile l) => new()
    {
        Id = l.Id,
        FullName = l.User.FullName,
        City = l.City,
        Court = l.Court,
        YearsOfExperience = l.YearsOfExperience,
        ConsultationFee = l.ConsultationFee,
        AverageRating = l.AverageRating,
        TotalReviews = l.TotalReviews,
        IsVerified = l.IsVerified,
        ProfilePictureUrl = l.User.ProfilePictureUrl,
        CategoryName = l.Category?.Name
    };

    private static LawyerDto MapToDto(LawyerProfile l) => new()
    {
        Id = l.Id,
        FullName = l.User.FullName,
        Email = l.User.Email ?? "",
        PhoneNumber = l.User.PhoneNumber ?? "",
        City = l.City,
        Court = l.Court,
        BarCouncilNumber = l.BarCouncilNumber,
        YearsOfExperience = l.YearsOfExperience,
        ConsultationFee = l.ConsultationFee,
        AverageRating = l.AverageRating,
        TotalReviews = l.TotalReviews,
        IsVerified = l.IsVerified,
        IsAvailable = l.IsAvailable,
        ProfilePictureUrl = l.User.ProfilePictureUrl,
        Bio = l.Bio,
        ProfileCompletionPercentage = l.ProfileCompletionPercentage,
        Category = l.Category == null ? null : new CategoryDto
        {
            Id = l.Category.Id,
            Name = l.Category.Name,
            IconClass = l.Category.IconClass,
            Description = l.Category.Description,
            LawyerCount = l.Category.Lawyers.Count(x => x.IsVerified)
        },
        Experiences = l.Experiences.Select(e => new ExperienceDto
        {
            Id = e.Id,
            Title = e.Title,
            Organization = e.Organization,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            IsCurrent = e.IsCurrent,
            Description = e.Description
        }).ToList(),
        CaseResults = l.CaseResults.Select(cr => new CaseResultDto
        {
            Id = cr.Id,
            CaseTitle = cr.CaseTitle,
            Court = cr.Court,
            Outcome = cr.Outcome,
            Year = cr.Year,
            Description = cr.Description
        }).ToList(),
        Reviews = l.Reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            ClientName = r.ClientProfile?.User?.FullName ?? "Anonymous",
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        }).ToList(),
        Certifications = l.Certifications.Select(c => new CertificationDto
        {
            Id = c.Id,
            Title = c.Title,
            IssuingOrganization = c.IssuingOrganization,
            Year = c.Year,
            CertificateUrl = c.CertificateUrl
        }).ToList(),
        Publications = l.Publications.Select(p => new PublicationDto
        {
            Id = p.Id,
            Title = p.Title,
            Publisher = p.Publisher,
            Year = p.Year,
            Url = p.Url
        }).ToList()
    };
}
