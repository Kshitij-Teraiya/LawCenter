using LegalConnect.API.DTOs.TimeSlots;

namespace LegalConnect.API.Services;

public interface IHolidayManagementService
{
    // Admin - Master Holidays
    Task<List<MasterHolidayDto>> GetMasterHolidaysAsync();
    Task<(bool Success, string Message, int? Id)> CreateMasterHolidayAsync(CreateMasterHolidayDto dto);
    Task<(bool Success, string Message)> UpdateMasterHolidayAsync(int id, UpdateMasterHolidayDto dto);
    Task<(bool Success, string Message)> DeleteMasterHolidayAsync(int id);

    // Lawyer - Holiday Preferences (master holidays)
    Task<List<HolidayPreferenceDto>> GetLawyerHolidayPreferencesAsync(int lawyerProfileId);
    Task<(bool Success, string Message)> SetHolidayPreferenceAsync(int lawyerProfileId, int masterHolidayId, bool isEnabled);

    // Lawyer - Personal Holidays
    Task<List<PersonalHolidayDto>> GetPersonalHolidaysAsync(int lawyerProfileId);
    Task<(bool Success, string Message, int? Id)> CreatePersonalHolidayAsync(int lawyerProfileId, CreatePersonalHolidayDto dto);
    Task<(bool Success, string Message)> DeletePersonalHolidayAsync(int lawyerProfileId, int holidayId);

    // Helper
    Task<bool> IsHolidayAsync(int lawyerProfileId, DateTime date);
}
