using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Admin;
using LegalConnect.Client.Models.Lawyer;

namespace LegalConnect.Client.Services;

public interface IAdminService
{
    Task<List<PendingLawyerDto>> GetPendingLawyersAsync();
    Task<(bool Success, string? Error)> ApproveLawyerAsync(int lawyerId);
    Task<(bool Success, string? Error)> RejectLawyerAsync(int lawyerId, string reason);

    Task<CommissionSettingDto?> GetCommissionSettingAsync();
    Task<(bool Success, string? Error)> SetCommissionAsync(SetCommissionDto dto);

    Task<List<CategoryDto>> GetAllCategoriesAsync();
    Task<(bool Success, string? Error)> CreateCategoryAsync(CreateCategoryDto dto);
    Task<(bool Success, string? Error)> UpdateCategoryAsync(UpdateCategoryDto dto);
    Task<(bool Success, string? Error)> DeleteCategoryAsync(int id);

    Task<RevenueStatsDto?> GetRevenueStatsAsync();

    Task<List<AdminLawyerDto>> GetAllLawyersAsync();
    Task<List<AdminClientDto>> GetAllClientsAsync();
    Task<(bool Success, string? Error)> ToggleUserActiveAsync(int userId);
}
