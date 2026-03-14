using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.TimeSlots;

namespace LegalConnect.Client.Services;

public class LawyerAvailabilityService : ILawyerAvailabilityService
{
    private readonly IHttpClientFactory _httpFactory;
    public LawyerAvailabilityService(IHttpClientFactory httpFactory) => _httpFactory = httpFactory;

    private HttpClient Secured => _httpFactory.CreateClient("secured");

    // ── Configuration ─────────────────────────────────────────────────────────

    public async Task<TimeSlotConfigurationModel?> GetConfigurationAsync()
    {
        var result = await Secured.GetFromJsonAsync<ApiResponse<TimeSlotConfigurationModel>>("availability/configuration");
        return result?.Data;
    }

    public async Task<bool> UpdateConfigurationAsync(UpdateTimeSlotConfigurationModel model)
    {
        var response = await Secured.PostAsJsonAsync("availability/configuration", model);
        return response.IsSuccessStatusCode;
    }

    // ── Working Hours ─────────────────────────────────────────────────────────

    public async Task<List<WorkingHoursModel>> GetWorkingHoursAsync()
    {
        var result = await Secured.GetFromJsonAsync<ApiResponse<List<WorkingHoursModel>>>("availability/working-hours");
        return result?.Data ?? [];
    }

    public async Task<bool> UpdateWorkingHoursAsync(int dayOfWeek, UpdateWorkingHoursModel model)
    {
        var response = await Secured.PutAsJsonAsync($"availability/working-hours/{dayOfWeek}", model);
        return response.IsSuccessStatusCode;
    }

    // ── Blackout Blocks ───────────────────────────────────────────────────────

    public async Task<List<BlackoutBlockModel>> GetBlackoutBlocksAsync()
    {
        var result = await Secured.GetFromJsonAsync<ApiResponse<List<BlackoutBlockModel>>>("availability/blackout-blocks");
        return result?.Data ?? [];
    }

    public async Task<bool> CreateBlackoutBlockAsync(CreateBlackoutBlockModel model)
    {
        var response = await Secured.PostAsJsonAsync("availability/blackout-blocks", model);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteBlackoutBlockAsync(int id)
    {
        var response = await Secured.DeleteAsync($"availability/blackout-blocks/{id}");
        return response.IsSuccessStatusCode;
    }

    // ── Personal Holidays ─────────────────────────────────────────────────────

    public async Task<List<PersonalHolidayModel>> GetPersonalHolidaysAsync()
    {
        var result = await Secured.GetFromJsonAsync<ApiResponse<List<PersonalHolidayModel>>>("availability/holidays/personal");
        return result?.Data ?? [];
    }

    public async Task<bool> CreatePersonalHolidayAsync(CreatePersonalHolidayModel model)
    {
        var response = await Secured.PostAsJsonAsync("availability/holidays/personal", model);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePersonalHolidayAsync(int id)
    {
        var response = await Secured.DeleteAsync($"availability/holidays/personal/{id}");
        return response.IsSuccessStatusCode;
    }

    // ── Holiday Preferences ───────────────────────────────────────────────────

    public async Task<List<HolidayPreferenceModel>> GetHolidayPreferencesAsync()
    {
        var result = await Secured.GetFromJsonAsync<ApiResponse<List<HolidayPreferenceModel>>>("availability/holidays/preferences");
        return result?.Data ?? [];
    }

    public async Task<bool> SetHolidayPreferenceAsync(int masterHolidayId, bool isEnabled)
    {
        var response = await Secured.PutAsJsonAsync($"availability/holidays/preferences/{masterHolidayId}", new { isEnabled });
        return response.IsSuccessStatusCode;
    }

    // ── Admin: Master Holidays ────────────────────────────────────────────────

    public async Task<List<MasterHolidayModel>> GetMasterHolidaysAsync()
    {
        var result = await Secured.GetFromJsonAsync<ApiResponse<List<MasterHolidayModel>>>("availability/holidays/master");
        return result?.Data ?? [];
    }

    public async Task<bool> CreateMasterHolidayAsync(CreateMasterHolidayModel model)
    {
        var response = await Secured.PostAsJsonAsync("availability/holidays/master", model);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateMasterHolidayAsync(int id, UpdateMasterHolidayModel model)
    {
        var response = await Secured.PutAsJsonAsync($"availability/holidays/master/{id}", model);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteMasterHolidayAsync(int id)
    {
        var response = await Secured.DeleteAsync($"availability/holidays/master/{id}");
        return response.IsSuccessStatusCode;
    }
}
