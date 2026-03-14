using LegalConnect.Client.Models.TimeSlots;

namespace LegalConnect.Client.Services;

public interface ILawyerAvailabilityService
{
    // Configuration
    Task<TimeSlotConfigurationModel?> GetConfigurationAsync();
    Task<bool> UpdateConfigurationAsync(UpdateTimeSlotConfigurationModel model);

    // Working Hours
    Task<List<WorkingHoursModel>> GetWorkingHoursAsync();
    Task<bool> UpdateWorkingHoursAsync(int dayOfWeek, UpdateWorkingHoursModel model);

    // Blackout Blocks
    Task<List<BlackoutBlockModel>> GetBlackoutBlocksAsync();
    Task<bool> CreateBlackoutBlockAsync(CreateBlackoutBlockModel model);
    Task<bool> DeleteBlackoutBlockAsync(int id);

    // Personal Holidays
    Task<List<PersonalHolidayModel>> GetPersonalHolidaysAsync();
    Task<bool> CreatePersonalHolidayAsync(CreatePersonalHolidayModel model);
    Task<bool> DeletePersonalHolidayAsync(int id);

    // Holiday Preferences (master holidays)
    Task<List<HolidayPreferenceModel>> GetHolidayPreferencesAsync();
    Task<bool> SetHolidayPreferenceAsync(int masterHolidayId, bool isEnabled);

    // Admin: Master Holidays
    Task<List<MasterHolidayModel>> GetMasterHolidaysAsync();
    Task<bool> CreateMasterHolidayAsync(CreateMasterHolidayModel model);
    Task<bool> UpdateMasterHolidayAsync(int id, UpdateMasterHolidayModel model);
    Task<bool> DeleteMasterHolidayAsync(int id);
}
