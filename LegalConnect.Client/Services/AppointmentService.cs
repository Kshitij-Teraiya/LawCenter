using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Appointment;

namespace LegalConnect.Client.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IHttpClientFactory _httpFactory;

    public AppointmentService(IHttpClientFactory httpFactory)
        => _httpFactory = httpFactory;

    public async Task<List<TimeSlotDto>> GetAvailableSlotsAsync(int lawyerId, DateTime date)
    {
        var client = _httpFactory.CreateClient("public");
        var result = await client.GetFromJsonAsync<ApiResponse<List<TimeSlotDto>>>(
            $"appointments/slots?lawyerId={lawyerId}&date={date:yyyy-MM-dd}");
        return result?.Data ?? [];
    }

    public async Task<(bool Success, string? Error, AppointmentDto? Appointment)>
        BookAppointmentAsync(BookAppointmentDto dto)
    {
        try
        {
            var client   = _httpFactory.CreateClient("secured");
            var response = await client.PostAsJsonAsync("appointments", dto);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (false, err?.Message ?? "Booking failed.", null);
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AppointmentDto>>();
            return (true, null, result?.Data);
        }
        catch (Exception ex) { return (false, ex.Message, null); }
    }

    public async Task<PagedResult<AppointmentDto>?> GetMyAppointmentsAsync(
        int page = 1, int pageSize = 10)
    {
        var client = _httpFactory.CreateClient("secured");
        var result = await client.GetFromJsonAsync<ApiResponse<PagedResult<AppointmentDto>>>(
            $"appointments/my?page={page}&pageSize={pageSize}");
        return result?.Data;
    }

    public async Task<(bool Success, string? Error)> CancelAppointmentAsync(
        int id, CancelAppointmentDto dto)
    {
        try
        {
            var client   = _httpFactory.CreateClient("secured");
            var response = await client.PutAsJsonAsync($"appointments/{id}/cancel", dto);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (false, err?.Message ?? "Cancel failed.");
            }
            return (true, null);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<List<AppointmentDto>> GetLawyerAppointmentsAsync(
        DateTime? from = null, DateTime? to = null)
    {
        var query = $"appointments/lawyer?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
        var client = _httpFactory.CreateClient("secured");
        var result = await client.GetFromJsonAsync<ApiResponse<List<AppointmentDto>>>(query);
        return result?.Data ?? [];
    }

    public async Task<(bool Success, string? Error)> ConfirmAppointmentAsync(int id)
        => await PutAsync($"appointments/{id}/confirm", new { });

    public async Task<(bool Success, string? Error)> CompleteAppointmentAsync(int id)
        => await PutAsync($"appointments/{id}/complete", new { });

    public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
    {
        var client = _httpFactory.CreateClient("secured");
        var result = await client.GetFromJsonAsync<ApiResponse<AppointmentDto>>($"appointments/{id}");
        return result?.Data;
    }

    private async Task<(bool, string?)> PutAsync<T>(string url, T payload)
    {
        try
        {
            var client = _httpFactory.CreateClient("secured");
            var response = await client.PutAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (false, err?.Message ?? "Request failed.");
            }
            return (true, null);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }
}
