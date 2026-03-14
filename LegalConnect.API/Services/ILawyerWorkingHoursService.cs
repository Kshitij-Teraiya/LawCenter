using LegalConnect.API.DTOs.TimeSlots;

namespace LegalConnect.API.Services;

public interface ILawyerWorkingHoursService
{
    Task<List<WorkingHoursDto>> GetWorkingHoursAsync(int lawyerProfileId);
    Task<WorkingHoursDto?> GetWorkingHoursByDayAsync(int lawyerProfileId, int dayOfWeek);
    Task<(bool Success, string Message)> UpdateWorkingHoursAsync(int lawyerProfileId, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, bool isWorking);
    Task<(bool Success, string Message)> SetDefaultHoursAsync(int lawyerProfileId);
}
