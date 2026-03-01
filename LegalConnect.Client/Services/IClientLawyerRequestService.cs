using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Cases;

namespace LegalConnect.Client.Services;

public interface IClientLawyerRequestService
{
    Task<ApiResponse?> SendRequestAsync(CreateClientLawyerRequestDto dto);
    Task<ApiResponse<List<ClientLawyerRequestDto>>?> GetMyRequestsAsync();
    Task<ApiResponse<List<ClientLawyerRequestDto>>?> GetIncomingRequestsAsync();
    Task<ApiResponse?> AcceptRequestAsync(int requestId, RespondToRequestDto dto);
    Task<ApiResponse?> RejectRequestAsync(int requestId, RespondToRequestDto dto);
    Task<ApiResponse?> CancelRequestAsync(int requestId);
}

public class ClientLawyerRequestService : IClientLawyerRequestService
{
    private readonly HttpClient _http;

    public ClientLawyerRequestService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("secured");
    }

    public async Task<ApiResponse?> SendRequestAsync(CreateClientLawyerRequestDto dto)
    {
        var resp = await _http.PostAsJsonAsync("lawyer-requests", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse<List<ClientLawyerRequestDto>>?> GetMyRequestsAsync()
        => await _http.GetFromJsonAsync<ApiResponse<List<ClientLawyerRequestDto>>>("lawyer-requests/my");

    public async Task<ApiResponse<List<ClientLawyerRequestDto>>?> GetIncomingRequestsAsync()
        => await _http.GetFromJsonAsync<ApiResponse<List<ClientLawyerRequestDto>>>("lawyer-requests/incoming");

    public async Task<ApiResponse?> AcceptRequestAsync(int requestId, RespondToRequestDto dto)
    {
        var resp = await _http.PutAsJsonAsync($"lawyer-requests/{requestId}/accept", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse?> RejectRequestAsync(int requestId, RespondToRequestDto dto)
    {
        var resp = await _http.PutAsJsonAsync($"lawyer-requests/{requestId}/reject", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse?> CancelRequestAsync(int requestId)
    {
        var resp = await _http.DeleteAsync($"lawyer-requests/{requestId}");
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
