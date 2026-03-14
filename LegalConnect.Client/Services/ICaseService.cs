using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Cases;

namespace LegalConnect.Client.Services;

public interface ICaseService
{
    Task<ApiResponse<PagedResult<CaseSummaryDto>>?> GetCasesAsync(CaseFilterDto filter);
    Task<ApiResponse<CaseDto>?> GetCaseByIdAsync(int id);
    Task<ApiResponse<CaseDto>?> CreateCaseAsync(CreateCaseDto dto);
    Task<ApiResponse?> UpdateCaseAsync(int id, UpdateCaseDto dto);
    Task<ApiResponse?> UpdateStatusAsync(int id, CaseStatus status);
    Task<ApiResponse?> CloseCaseAsync(int id, CloseCaseDto dto);
    Task<ApiResponse?> DeleteCaseAsync(int id);
    Task<ApiResponse<List<CaseActivityDto>>?> GetActivitiesAsync(int caseId);
    Task<ApiResponse<CaseActivityDto>?> AddActivityAsync(int caseId, AddCaseActivityDto dto);
    Task<ApiResponse<List<CaseDocumentDto>>?> GetDocumentsAsync(int caseId);
    Task<ApiResponse?> DeleteDocumentAsync(int documentId);
    Task<ApiResponse<List<HiredLawyerDto>>?> GetMyLawyersAsync();
    Task<ApiResponse<List<CaseLawyerDto>>?> GetCaseLawyersAsync(int caseId);
    Task<ApiResponse?> AddLawyerToCaseAsync(int caseId, int lawyerProfileId);
    Task<ApiResponse?> RemoveLawyerFromCaseAsync(int caseId, int lawyerProfileId);
    Task<ApiResponse?> LinkDocumentToActivityAsync(int caseId, int activityId, int documentId);
    Task<ApiResponse<bool>?> ToggleAvailableForDealAsync(int documentId);
    Task<ApiResponse<bool>?> TogglePrivateAsync(int documentId);
}

public class CaseService : ICaseService
{
    private readonly HttpClient _http;

    public CaseService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("secured");
    }

    public async Task<ApiResponse<PagedResult<CaseSummaryDto>>?> GetCasesAsync(CaseFilterDto filter)
        => await _http.GetFromJsonAsync<ApiResponse<PagedResult<CaseSummaryDto>>>($"cases?{filter.ToQueryString()}");

    public async Task<ApiResponse<CaseDto>?> GetCaseByIdAsync(int id)
        => await _http.GetFromJsonAsync<ApiResponse<CaseDto>>($"cases/{id}");

    public async Task<ApiResponse<CaseDto>?> CreateCaseAsync(CreateCaseDto dto)
    {
        var resp = await _http.PostAsJsonAsync("cases", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<CaseDto>>();
    }

    public async Task<ApiResponse?> UpdateCaseAsync(int id, UpdateCaseDto dto)
    {
        var resp = await _http.PutAsJsonAsync($"cases/{id}", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse?> UpdateStatusAsync(int id, CaseStatus status)
    {
        var resp = await _http.PutAsJsonAsync($"cases/{id}/status", new { Status = status });
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse?> CloseCaseAsync(int id, CloseCaseDto dto)
    {
        var resp = await _http.PutAsJsonAsync($"cases/{id}/close", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse?> DeleteCaseAsync(int id)
    {
        var resp = await _http.DeleteAsync($"cases/{id}");
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse<List<CaseActivityDto>>?> GetActivitiesAsync(int caseId)
        => await _http.GetFromJsonAsync<ApiResponse<List<CaseActivityDto>>>($"cases/{caseId}/activities");

    public async Task<ApiResponse<CaseActivityDto>?> AddActivityAsync(int caseId, AddCaseActivityDto dto)
    {
        var resp = await _http.PostAsJsonAsync($"cases/{caseId}/activities", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<CaseActivityDto>>();
    }

    public async Task<ApiResponse<List<CaseDocumentDto>>?> GetDocumentsAsync(int caseId)
        => await _http.GetFromJsonAsync<ApiResponse<List<CaseDocumentDto>>>($"cases/{caseId}/documents");

    public async Task<ApiResponse?> DeleteDocumentAsync(int documentId)
    {
        var resp = await _http.DeleteAsync($"case-documents/{documentId}");
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse<List<HiredLawyerDto>>?> GetMyLawyersAsync()
        => await _http.GetFromJsonAsync<ApiResponse<List<HiredLawyerDto>>>("cases/my-lawyers");

    public async Task<ApiResponse<List<CaseLawyerDto>>?> GetCaseLawyersAsync(int caseId)
        => await _http.GetFromJsonAsync<ApiResponse<List<CaseLawyerDto>>>($"cases/{caseId}/lawyers");

    public async Task<ApiResponse?> AddLawyerToCaseAsync(int caseId, int lawyerProfileId)
    {
        var resp = await _http.PostAsJsonAsync($"cases/{caseId}/lawyers", new { LawyerProfileId = lawyerProfileId });
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse?> RemoveLawyerFromCaseAsync(int caseId, int lawyerProfileId)
    {
        var resp = await _http.DeleteAsync($"cases/{caseId}/lawyers/{lawyerProfileId}");
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse?> LinkDocumentToActivityAsync(int caseId, int activityId, int documentId)
    {
        var resp = await _http.PostAsync($"cases/{caseId}/activities/{activityId}/link-document/{documentId}", null);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse<bool>?> ToggleAvailableForDealAsync(int documentId)
    {
        var resp = await _http.PutAsync($"case-documents/{documentId}/toggle-deal-share", null);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    public async Task<ApiResponse<bool>?> TogglePrivateAsync(int documentId)
    {
        var resp = await _http.PutAsync($"case-documents/{documentId}/toggle-private", null);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }
}
