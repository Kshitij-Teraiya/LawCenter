using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Appointment;

namespace LegalConnect.Client.Services;

public interface IAppointmentService
{
    Task<List<TimeSlotDto>> GetAvailableSlotsAsync(int lawyerId, DateTime date);
    Task<(bool Success, string? Error, AppointmentDto? Appointment)> BookAppointmentAsync(BookAppointmentDto dto);

    // Client
    Task<PagedResult<AppointmentDto>?> GetMyAppointmentsAsync(int page = 1, int pageSize = 10);
    Task<(bool Success, string? Error)> CancelAppointmentAsync(int id, CancelAppointmentDto dto);

    // Lawyer
    Task<List<AppointmentDto>> GetLawyerAppointmentsAsync(DateTime? from = null, DateTime? to = null);
    Task<(bool Success, string? Error)> ConfirmAppointmentAsync(int id);
    Task<(bool Success, string? Error)> CompleteAppointmentAsync(int id);

    // Shared
    Task<AppointmentDto?> GetAppointmentByIdAsync(int id);
}
