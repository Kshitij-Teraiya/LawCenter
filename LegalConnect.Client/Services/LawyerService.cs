using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Lawyer;

namespace LegalConnect.Client.Services;

public class LawyerService : ILawyerService
{
    private readonly IHttpClientFactory _httpFactory;

    public LawyerService(IHttpClientFactory httpFactory)
        => _httpFactory = httpFactory;

    // ── Public ───────────────────────────────────────────────────────────────

    public async Task<PagedResult<LawyerSummaryDto>?> GetLawyersAsync(LawyerFilterDto filter)
    {
        var client = _httpFactory.CreateClient("public");
        var result = await client.GetFromJsonAsync<ApiResponse<PagedResult<LawyerSummaryDto>>>(
            $"lawyers?{filter.ToQueryString()}");
        return result?.Data;
    }

    public async Task<LawyerDto?> GetLawyerByIdAsync(int id)
    {
        var client = _httpFactory.CreateClient("public");
        var result = await client.GetFromJsonAsync<ApiResponse<LawyerDto>>($"lawyers/{id}");
        return result?.Data;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        var client = _httpFactory.CreateClient("public");
        var result = await client.GetFromJsonAsync<ApiResponse<List<CategoryDto>>>("categories");
        return result?.Data ?? [];
    }

    public async Task<List<string>> GetCitiesAsync()
    {
        var client = _httpFactory.CreateClient("public");
        var result = await client.GetFromJsonAsync<ApiResponse<List<string>>>("lawyers/cities");
        return result?.Data ?? [];
    }

    public async Task<List<string>> GetCourtsAsync()
    {
        var client = _httpFactory.CreateClient("public");
        var result = await client.GetFromJsonAsync<ApiResponse<List<string>>>("lawyers/courts");
        return result?.Data ?? [];
    }

    // ── Lawyer Self-Service (requires auth) ──────────────────────────────────

    public async Task<LawyerDto?> GetMyProfileAsync()
    {
        var client = _httpFactory.CreateClient("secured");
        var result = await client.GetFromJsonAsync<ApiResponse<LawyerDto>>("lawyers/me");
        return result?.Data;
    }

    public async Task<(bool Success, string? Error)> UpdateMyProfileAsync(UpdateLawyerProfileDto dto)
        => await PutAsync("lawyers/me", dto);

    public async Task<(bool Success, string? Error)> AddExperienceAsync(AddExperienceDto dto)
        => await PostAsync("lawyers/me/experiences", dto);

    public async Task<(bool Success, string? Error)> DeleteExperienceAsync(int experienceId)
        => await DeleteAsync($"lawyers/me/experiences/{experienceId}");

    public async Task<(bool Success, string? Error)> AddCaseResultAsync(AddCaseResultDto dto)
        => await PostAsync("lawyers/me/case-results", dto);

    public async Task<(bool Success, string? Error)> DeleteCaseResultAsync(int caseResultId)
        => await DeleteAsync($"lawyers/me/case-results/{caseResultId}");

    public async Task<(bool Success, string? Error)> SetServiceChargeAsync(SetServiceChargeDto dto)
        => await PutAsync("lawyers/me/service-charge", dto);

    // ── Helpers ──────────────────────────────────────────────────────────────

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

    private async Task<(bool, string?)> DeleteAsync(string url)
    {
        try
        {
            var client = _httpFactory.CreateClient("secured");
            var response = await client.DeleteAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (false, err?.Message ?? "Delete failed.");
            }
            return (true, null);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }
}
