using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Admin;
using LegalConnect.Client.Models.Lawyer;

namespace LegalConnect.Client.Services;

public class AdminService : IAdminService
{
    private readonly IHttpClientFactory _httpFactory;

    public AdminService(IHttpClientFactory httpFactory)
        => _httpFactory = httpFactory;

    public async Task<List<PendingLawyerDto>> GetPendingLawyersAsync()
    {
        var client = _httpFactory.CreateClient("secured");
        var result = await client.GetFromJsonAsync<ApiResponse<List<PendingLawyerDto>>>("admin/lawyers/pending");
        return result?.Data ?? [];
    }

    public async Task<(bool Success, string? Error)> ApproveLawyerAsync(int lawyerId)
        => await PutAsync($"admin/lawyers/{lawyerId}/approve", new { });

    public async Task<(bool Success, string? Error)> RejectLawyerAsync(int lawyerId, string reason)
        => await PutAsync($"admin/lawyers/{lawyerId}/reject", new { Reason = reason });

    public async Task<CommissionSettingDto?> GetCommissionSettingAsync()
    {
        var client = _httpFactory.CreateClient("secured");
        var result = await client.GetFromJsonAsync<ApiResponse<CommissionSettingDto>>("admin/commission");
        return result?.Data;
    }

    public async Task<(bool Success, string? Error)> SetCommissionAsync(SetCommissionDto dto)
        => await PutAsync("admin/commission", dto);

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        var client = _httpFactory.CreateClient("secured");
        var result = await client.GetFromJsonAsync<ApiResponse<List<CategoryDto>>>("admin/categories");
        return result?.Data ?? [];
    }

    public async Task<(bool Success, string? Error)> CreateCategoryAsync(CreateCategoryDto dto)
        => await PostAsync("admin/categories", dto);

    public async Task<(bool Success, string? Error)> UpdateCategoryAsync(UpdateCategoryDto dto)
        => await PutAsync($"admin/categories/{dto.Id}", dto);

    public async Task<(bool Success, string? Error)> DeleteCategoryAsync(int id)
    {
        try
        {
            var client = _httpFactory.CreateClient("secured");
            var response = await client.DeleteAsync($"admin/categories/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (false, err?.Message ?? "Delete failed.");
            }
            return (true, null);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<RevenueStatsDto?> GetRevenueStatsAsync()
    {
        var client = _httpFactory.CreateClient("secured");
        var result = await client.GetFromJsonAsync<ApiResponse<RevenueStatsDto>>("admin/revenue");
        return result?.Data;
    }

    private async Task<(bool, string?)> PostAsync<T>(string url, T payload)
    {
        try
        {
            var client = _httpFactory.CreateClient("secured");
            var response = await client.PostAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (false, err?.Message ?? "Request failed.");
            }
            return (true, null);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    private async Task<(bool, string?)> PutAsync<T>(string url, T payload)
    {
        try
        {
            var client = _httpFactory.CreateClient("secured");
            var response = await client.PutAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (false, err?.Message ?? "Request failed.");
            }
            return (true, null);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }
}
