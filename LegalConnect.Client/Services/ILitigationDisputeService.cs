using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Dues;

namespace LegalConnect.Client.Services;

public interface ILitigationDisputeService
{
    Task<ApiResponse<PagedResult<LitigationDisputeModel>>?> GetDisputesAsync(string? status = null, int page = 1, int pageSize = 20);
    Task<ApiResponse<LitigationDisputeModel>?> GetByIdAsync(int id);
    Task<ApiResponse<LitigationDisputeModel>?> RaiseDisputeAsync(RaiseDisputeModel model);
    Task<ApiResponse?> AdminApproveAsync(int id, ApproveDisputeModel model);
    Task<ApiResponse?> LawyerApproveAsync(int id, ApproveDisputeModel model);
}

public class LitigationDisputeClientService : ILitigationDisputeService
{
    private readonly HttpClient _http;

    public LitigationDisputeClientService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("secured");
    }

    public async Task<ApiResponse<PagedResult<LitigationDisputeModel>>?> GetDisputesAsync(
        string? status = null, int page = 1, int pageSize = 20)
    {
        var qs = $"litigation-disputes?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(status)) qs += $"&status={Uri.EscapeDataString(status)}";
        return await _http.GetFromJsonAsync<ApiResponse<PagedResult<LitigationDisputeModel>>>(qs);
    }

    public async Task<ApiResponse<LitigationDisputeModel>?> GetByIdAsync(int id)
        => await _http.GetFromJsonAsync<ApiResponse<LitigationDisputeModel>>($"litigation-disputes/{id}");

    public async Task<ApiResponse<LitigationDisputeModel>?> RaiseDisputeAsync(RaiseDisputeModel model)
    {
        var resp = await _http.PostAsJsonAsync("litigation-disputes", model);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<LitigationDisputeModel>>();
    }

    public async Task<ApiResponse?> AdminApproveAsync(int id, ApproveDisputeModel model)
    {
        var resp = await _http.PutAsJsonAsync($"litigation-disputes/{id}/admin-approve", model);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse?> LawyerApproveAsync(int id, ApproveDisputeModel model)
    {
        var resp = await _http.PutAsJsonAsync($"litigation-disputes/{id}/lawyer-approve", model);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
