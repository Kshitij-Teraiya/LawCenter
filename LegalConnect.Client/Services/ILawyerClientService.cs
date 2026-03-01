using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Cases;

namespace LegalConnect.Client.Services;

public interface ILawyerClientService
{
    Task<ApiResponse<PagedResult<LawyerClientDto>>?> GetMyClientsAsync(LawyerClientFilterDto filter);
    Task<ApiResponse<LawyerClientDto>?> GetClientByIdAsync(int id);
    Task<ApiResponse<List<EligibleClientDto>>?> GetEligibleClientsAsync();
    Task<ApiResponse<LawyerClientDto>?> AddClientAsync(AddLawyerClientDto dto);
    Task<ApiResponse?> UpdateNotesAsync(int id, UpdateLawyerClientNotesDto dto);
    Task<ApiResponse?> RemoveClientAsync(int id);
}

public class LawyerClientService : ILawyerClientService
{
    private readonly HttpClient _http;

    public LawyerClientService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("secured");
    }

    public async Task<ApiResponse<PagedResult<LawyerClientDto>>?> GetMyClientsAsync(LawyerClientFilterDto filter)
    {
        var qs = filter.ToQueryString();
        return await _http.GetFromJsonAsync<ApiResponse<PagedResult<LawyerClientDto>>>($"lawyer-clients?{qs}");
    }

    public async Task<ApiResponse<LawyerClientDto>?> GetClientByIdAsync(int id)
        => await _http.GetFromJsonAsync<ApiResponse<LawyerClientDto>>($"lawyer-clients/{id}");

    public async Task<ApiResponse<List<EligibleClientDto>>?> GetEligibleClientsAsync()
        => await _http.GetFromJsonAsync<ApiResponse<List<EligibleClientDto>>>("lawyer-clients/eligible");

    public async Task<ApiResponse<LawyerClientDto>?> AddClientAsync(AddLawyerClientDto dto)
    {
        var resp = await _http.PostAsJsonAsync("lawyer-clients", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<LawyerClientDto>>();
    }

    public async Task<ApiResponse?> UpdateNotesAsync(int id, UpdateLawyerClientNotesDto dto)
    {
        var resp = await _http.PutAsJsonAsync($"lawyer-clients/{id}/notes", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse?> RemoveClientAsync(int id)
    {
        var resp = await _http.DeleteAsync($"lawyer-clients/{id}");
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
