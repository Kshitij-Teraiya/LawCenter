using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Lawyer;

namespace LegalConnect.Client.Services;

public interface ILawyerService
{
    Task<PagedResult<LawyerSummaryDto>?> GetLawyersAsync(LawyerFilterDto filter);
    Task<LawyerDto?> GetLawyerByIdAsync(int id);
    Task<List<CategoryDto>> GetCategoriesAsync();
    Task<List<string>> GetCitiesAsync();
    Task<List<string>> GetCourtsAsync();

    // Lawyer self-service
    Task<LawyerDto?> GetMyProfileAsync();
    Task<(bool Success, string? Error)> UpdateMyProfileAsync(UpdateLawyerProfileDto dto);
    Task<(bool Success, string? Error)> AddExperienceAsync(AddExperienceDto dto);
    Task<(bool Success, string? Error)> DeleteExperienceAsync(int experienceId);
    Task<(bool Success, string? Error)> AddCaseResultAsync(AddCaseResultDto dto);
    Task<(bool Success, string? Error)> DeleteCaseResultAsync(int caseResultId);
    Task<(bool Success, string? Error)> SetServiceChargeAsync(SetServiceChargeDto dto);
}
