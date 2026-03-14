using LegalConnect.API.DTOs.TimeSlots;

namespace LegalConnect.API.Services;

public interface ILawyerTimeSlotConfigurationService
{
    Task<TimeSlotConfigurationDto?> GetConfigurationAsync(int lawyerProfileId);
    Task<(bool Success, string Message)> UpdateConfigurationAsync(int lawyerProfileId, int sessionDurationMinutes, int bufferTimeMinutes);
    Task<(bool Success, string Message)> CreateDefaultConfigurationAsync(int lawyerProfileId);
}
