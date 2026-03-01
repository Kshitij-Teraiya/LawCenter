using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Client;

namespace LegalConnect.Client.Services;

public interface IClientProfileService
{
    Task<ApiResponse<ClientProfileDto>?> GetMyProfileAsync();
    Task<ApiResponse?> UpdateMyProfileAsync(UpdateClientProfileDto dto);
}

public class ClientProfileService : IClientProfileService
{
    private readonly HttpClient _http;

    public ClientProfileService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("secured");
    }

    public async Task<ApiResponse<ClientProfileDto>?> GetMyProfileAsync()
        => await _http.GetFromJsonAsync<ApiResponse<ClientProfileDto>>("client-profiles/me");

    public async Task<ApiResponse?> UpdateMyProfileAsync(UpdateClientProfileDto dto)
    {
        var resp = await _http.PutAsJsonAsync("client-profiles/me", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
