using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Admin;

namespace LegalConnect.Client.Services;

public interface IAdminStaffService
{
    Task<ApiResponse<List<AdminStaffDto>>?> GetAllAsync();
    Task<ApiResponse<AdminStaffDto>?> GetByIdAsync(int id);
    Task<ApiResponse<AdminStaffDto>?> CreateAsync(CreateAdminStaffModel dto);
    Task<(bool Success, string? Error)> UpdateRolesAsync(int id, UpdateAdminStaffRolesModel dto);
    Task<(bool Success, string? Error)> ToggleActiveAsync(int id);
    Task<(bool Success, string? Error)> ResetPasswordAsync(int id, ResetAdminStaffPasswordModel dto);
    Task<ApiResponse<List<AdminStaffRoleInfoDto>>?> GetAvailableRolesAsync();
}

public class AdminStaffService : IAdminStaffService
{
    private readonly IHttpClientFactory _httpFactory;

    public AdminStaffService(IHttpClientFactory httpFactory) => _httpFactory = httpFactory;

    public async Task<ApiResponse<List<AdminStaffDto>>?> GetAllAsync()
    {
        var client = _httpFactory.CreateClient("secured");
        return await client.GetFromJsonAsync<ApiResponse<List<AdminStaffDto>>>("admin/staff");
    }

    public async Task<ApiResponse<AdminStaffDto>?> GetByIdAsync(int id)
    {
        var client = _httpFactory.CreateClient("secured");
        return await client.GetFromJsonAsync<ApiResponse<AdminStaffDto>>($"admin/staff/{id}");
    }

    public async Task<ApiResponse<AdminStaffDto>?> CreateAsync(CreateAdminStaffModel dto)
    {
        try
        {
            var client = _httpFactory.CreateClient("secured");
            var response = await client.PostAsJsonAsync("admin/staff", dto);
            return await response.Content.ReadFromJsonAsync<ApiResponse<AdminStaffDto>>();
        }
        catch (Exception ex)
        {
            return new ApiResponse<AdminStaffDto> { Success = false, Message = ex.Message };
        }
    }

    public async Task<(bool Success, string? Error)> UpdateRolesAsync(int id, UpdateAdminStaffRolesModel dto)
    {
        try
        {
            var client = _httpFactory.CreateClient("secured");
            var response = await client.PutAsJsonAsync($"admin/staff/{id}/roles", dto);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (false, err?.Message ?? "Request failed.");
            }
            return (true, null);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Success, string? Error)> ToggleActiveAsync(int id)
    {
        try
        {
            var client = _httpFactory.CreateClient("secured");
            var response = await client.PutAsJsonAsync($"admin/staff/{id}/toggle-active", new { });
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (false, err?.Message ?? "Request failed.");
            }
            return (true, null);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(int id, ResetAdminStaffPasswordModel dto)
    {
        try
        {
            var client = _httpFactory.CreateClient("secured");
            var response = await client.PutAsJsonAsync($"admin/staff/{id}/reset-password", dto);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (false, err?.Message ?? "Request failed.");
            }
            return (true, null);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<ApiResponse<List<AdminStaffRoleInfoDto>>?> GetAvailableRolesAsync()
    {
        var client = _httpFactory.CreateClient("secured");
        return await client.GetFromJsonAsync<ApiResponse<List<AdminStaffRoleInfoDto>>>("admin/staff/roles");
    }
}
