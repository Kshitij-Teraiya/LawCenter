using LegalConnect.API.DTOs.Appointment;

namespace LegalConnect.API.Services;

public interface IAppointmentSlotService
{
    Task<List<TimeSlotDto>> GetAvailableSlotsAsync(int lawyerProfileId, DateTime date);
    Task<int> GetSessionDurationAsync(int lawyerProfileId);
}
