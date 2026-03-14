using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Dues;

namespace LegalConnect.Client.Services;

public interface IDuesClientService
{
    Task<ApiResponse<DuesSummaryModel>?> GetMyDuesAsync();
    Task<ApiResponse<LawyerDuesSummaryModel>?> GetLawyerDuesAsync(int lawyerProfileId);
    Task<ApiResponse<PagedResult<LawyerDuesSummaryModel>>?> GetAllLawyerDuesAsync(int page = 1, int pageSize = 20);
}

public class DuesClientService : IDuesClientService
{
    private readonly HttpClient _http;

    public DuesClientService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("secured");
    }

    public async Task<ApiResponse<DuesSummaryModel>?> GetMyDuesAsync()
        => await _http.GetFromJsonAsync<ApiResponse<DuesSummaryModel>>("dues/my");

    public async Task<ApiResponse<LawyerDuesSummaryModel>?> GetLawyerDuesAsync(int lawyerProfileId)
        => await _http.GetFromJsonAsync<ApiResponse<LawyerDuesSummaryModel>>($"dues/lawyer/{lawyerProfileId}");

    public async Task<ApiResponse<PagedResult<LawyerDuesSummaryModel>>?> GetAllLawyerDuesAsync(
        int page = 1, int pageSize = 20)
        => await _http.GetFromJsonAsync<ApiResponse<PagedResult<LawyerDuesSummaryModel>>>($"dues/all?page={page}&pageSize={pageSize}");
}
