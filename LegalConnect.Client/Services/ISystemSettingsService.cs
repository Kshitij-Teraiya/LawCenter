using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Dues;

namespace LegalConnect.Client.Services;

public interface ISystemSettingsClientService
{
    Task<ApiResponse<SystemSettingModel>?> GetByKeyAsync(string key);
    Task<ApiResponse<List<SystemSettingModel>>?> GetAllAsync();
    Task<ApiResponse?> UpdateAsync(string key, string value);
}

public class SystemSettingsClientService : ISystemSettingsClientService
{
    private readonly HttpClient _httpSecured;
    private readonly HttpClient _httpPublic;

    public SystemSettingsClientService(IHttpClientFactory httpClientFactory)
    {
        _httpSecured = httpClientFactory.CreateClient("secured");
        _httpPublic  = httpClientFactory.CreateClient("public");
    }

    public async Task<ApiResponse<SystemSettingModel>?> GetByKeyAsync(string key)
        => await _httpPublic.GetFromJsonAsync<ApiResponse<SystemSettingModel>>($"system-settings/{key}");

    public async Task<ApiResponse<List<SystemSettingModel>>?> GetAllAsync()
        => await _httpSecured.GetFromJsonAsync<ApiResponse<List<SystemSettingModel>>>("system-settings");

    public async Task<ApiResponse?> UpdateAsync(string key, string value)
    {
        var resp = await _httpSecured.PutAsJsonAsync($"system-settings/{key}", new { value });
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
