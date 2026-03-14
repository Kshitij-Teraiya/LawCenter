using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Staff;

namespace LegalConnect.Client.Services;

public interface IStaffService
{
    // ── Lawyer: Staff management ──────────────────────────────────────────
    Task<ApiResponse<List<StaffProfileModel>>?> GetMyStaffAsync();
    Task<ApiResponse<StaffProfileModel>?> CreateStaffAsync(CreateStaffModel model);
    Task<ApiResponse<StaffProfileModel>?> GetStaffByIdAsync(int id);
    Task<ApiResponse<string>?> ToggleStaffActiveAsync(int id);

    // ── Case staff assignment ─────────────────────────────────────────────
    Task<ApiResponse<List<CaseStaffModel>>?> GetCaseStaffAsync(int caseId);
    Task<ApiResponse<string>?> AssignStaffToCaseAsync(int caseId, AssignStaffModel model);
    Task<ApiResponse<string>?> RemoveStaffFromCaseAsync(int caseId, int staffId);
    Task<ApiResponse<StaffCasePermissionsModel>?> GetMyPermissionsAsync(int caseId);

    // ── Lawyer: Task management ───────────────────────────────────────────
    Task<ApiResponse<List<StaffTaskModel>>?> GetAllTasksAsync();
    Task<ApiResponse<StaffTaskModel>?> CreateTaskAsync(CreateStaffTaskModel model);
    Task<ApiResponse<StaffTaskModel>?> UpdateTaskAsync(int id, UpdateStaffTaskModel model);
    Task<ApiResponse<string>?> DeleteTaskAsync(int id);

    // ── Staff: own tasks ──────────────────────────────────────────────────
    Task<ApiResponse<List<StaffTaskModel>>?> GetMyTasksAsync();
    Task<ApiResponse<StaffTaskModel>?> UpdateMyTaskStatusAsync(int id, UpdateTaskStatusModel model);
}

public class StaffService : IStaffService
{
    private readonly HttpClient _http;

    public StaffService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("secured");
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    // Safe GET: does not throw on 4xx/5xx — returns null only on network failure
    private async Task<ApiResponse<T>?> GetAsync<T>(string url)
    {
        var response = await _http.GetAsync(url);
        return await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
    }

    // ── Lawyer: Staff management ──────────────────────────────────────────

    public async Task<ApiResponse<List<StaffProfileModel>>?> GetMyStaffAsync()
        => await GetAsync<List<StaffProfileModel>>("staff/my-staff");

    public async Task<ApiResponse<StaffProfileModel>?> CreateStaffAsync(CreateStaffModel model)
    {
        var response = await _http.PostAsJsonAsync("staff/create", model);
        return await response.Content.ReadFromJsonAsync<ApiResponse<StaffProfileModel>>();
    }

    public async Task<ApiResponse<StaffProfileModel>?> GetStaffByIdAsync(int id)
        => await GetAsync<StaffProfileModel>($"staff/{id}");

    public async Task<ApiResponse<string>?> ToggleStaffActiveAsync(int id)
    {
        var response = await _http.PutAsJsonAsync($"staff/{id}/toggle", new { });
        return await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
    }

    // ── Case staff assignment ─────────────────────────────────────────────

    public async Task<ApiResponse<List<CaseStaffModel>>?> GetCaseStaffAsync(int caseId)
        => await GetAsync<List<CaseStaffModel>>($"staff/cases/{caseId}/staff");

    public async Task<ApiResponse<string>?> AssignStaffToCaseAsync(int caseId, AssignStaffModel model)
    {
        var response = await _http.PostAsJsonAsync($"staff/cases/{caseId}/assign", model);
        return await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
    }

    public async Task<ApiResponse<string>?> RemoveStaffFromCaseAsync(int caseId, int staffId)
    {
        var response = await _http.DeleteAsync($"staff/cases/{caseId}/staff/{staffId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
    }

    public async Task<ApiResponse<StaffCasePermissionsModel>?> GetMyPermissionsAsync(int caseId)
        => await GetAsync<StaffCasePermissionsModel>($"staff/cases/{caseId}/my-permissions");

    // ── Lawyer: Task management ───────────────────────────────────────────

    public async Task<ApiResponse<List<StaffTaskModel>>?> GetAllTasksAsync()
        => await GetAsync<List<StaffTaskModel>>("staff/tasks");

    public async Task<ApiResponse<StaffTaskModel>?> CreateTaskAsync(CreateStaffTaskModel model)
    {
        var response = await _http.PostAsJsonAsync("staff/tasks", model);
        return await response.Content.ReadFromJsonAsync<ApiResponse<StaffTaskModel>>();
    }

    public async Task<ApiResponse<StaffTaskModel>?> UpdateTaskAsync(int id, UpdateStaffTaskModel model)
    {
        var response = await _http.PutAsJsonAsync($"staff/tasks/{id}", model);
        return await response.Content.ReadFromJsonAsync<ApiResponse<StaffTaskModel>>();
    }

    public async Task<ApiResponse<string>?> DeleteTaskAsync(int id)
    {
        var response = await _http.DeleteAsync($"staff/tasks/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
    }

    // ── Staff: own tasks ──────────────────────────────────────────────────

    public async Task<ApiResponse<List<StaffTaskModel>>?> GetMyTasksAsync()
        => await GetAsync<List<StaffTaskModel>>("staff/my-tasks");

    public async Task<ApiResponse<StaffTaskModel>?> UpdateMyTaskStatusAsync(int id, UpdateTaskStatusModel model)
    {
        var response = await _http.PutAsJsonAsync($"staff/my-tasks/{id}", model);
        return await response.Content.ReadFromJsonAsync<ApiResponse<StaffTaskModel>>();
    }
}
