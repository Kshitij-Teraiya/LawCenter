using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Support;

namespace LegalConnect.Client.Services;

public interface ISupportService
{
    Task<ApiResponse<List<SupportTicketDto>>?> GetMyTicketsAsync();
    Task<ApiResponse<List<SupportTicketDto>>?> GetAllTicketsAsync(string? status = null, string? category = null);
    Task<ApiResponse<SupportTicketDto>?> GetTicketByIdAsync(int id);
    Task<ApiResponse<SupportTicketDto>?> CreateTicketAsync(CreateSupportTicketDto dto);
    Task<ApiResponse?> UpdateTicketStatusAsync(int id, UpdateTicketStatusDto dto);
    Task<ApiResponse<List<SupportMessageDto>>?> GetMessagesAsync(int ticketId);
    Task<ApiResponse<SupportMessageDto>?> SendMessageAsync(int ticketId, SendSupportMessageDto dto);
}

public class SupportService : ISupportService
{
    private readonly HttpClient _http;

    public SupportService(IHttpClientFactory httpFactory)
        => _http = httpFactory.CreateClient("secured");

    public async Task<ApiResponse<List<SupportTicketDto>>?> GetMyTicketsAsync()
        => await _http.GetFromJsonAsync<ApiResponse<List<SupportTicketDto>>>("support/my");

    public async Task<ApiResponse<List<SupportTicketDto>>?> GetAllTicketsAsync(string? status = null, string? category = null)
    {
        var query = "support/all";
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(status)) parts.Add($"status={Uri.EscapeDataString(status)}");
        if (!string.IsNullOrEmpty(category)) parts.Add($"category={Uri.EscapeDataString(category)}");
        if (parts.Count > 0) query += "?" + string.Join("&", parts);
        return await _http.GetFromJsonAsync<ApiResponse<List<SupportTicketDto>>>(query);
    }

    public async Task<ApiResponse<SupportTicketDto>?> GetTicketByIdAsync(int id)
        => await _http.GetFromJsonAsync<ApiResponse<SupportTicketDto>>($"support/{id}");

    public async Task<ApiResponse<SupportTicketDto>?> CreateTicketAsync(CreateSupportTicketDto dto)
    {
        var resp = await _http.PostAsJsonAsync("support", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<SupportTicketDto>>();
    }

    public async Task<ApiResponse?> UpdateTicketStatusAsync(int id, UpdateTicketStatusDto dto)
    {
        var resp = await _http.PutAsJsonAsync($"support/{id}/status", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse<List<SupportMessageDto>>?> GetMessagesAsync(int ticketId)
        => await _http.GetFromJsonAsync<ApiResponse<List<SupportMessageDto>>>($"support/{ticketId}/messages");

    public async Task<ApiResponse<SupportMessageDto>?> SendMessageAsync(int ticketId, SendSupportMessageDto dto)
    {
        var resp = await _http.PostAsJsonAsync($"support/{ticketId}/messages", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<SupportMessageDto>>();
    }
}
