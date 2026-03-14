using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Appointment;
using LegalConnect.API.Entities;
using LegalConnect.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface IAppointmentService
{
    Task<List<TimeSlotDto>> GetAvailableSlotsAsync(int lawyerProfileId, DateTime date);
    Task<(bool Success, string Message, AppointmentDto? Data)> BookAppointmentAsync(int clientUserId, BookAppointmentDto dto);
    Task<PagedResult<AppointmentDto>> GetMyAppointmentsAsync(int clientUserId, int page, int pageSize);
    Task<(bool Success, string Message)> CancelAppointmentAsync(int userId, string userRole, int appointmentId, CancelAppointmentDto dto);
    Task<List<AppointmentDto>> GetLawyerAppointmentsAsync(int lawyerUserId, DateTime? from, DateTime? to);
    Task<(bool Success, string Message)> ConfirmAppointmentAsync(int lawyerUserId, int appointmentId);
    Task<(bool Success, string Message)> CompleteAppointmentAsync(int lawyerUserId, int appointmentId);
    Task<AppointmentDto?> GetAppointmentByIdAsync(int userId, string userRole, int appointmentId);
    Task<(bool Success, string Message)> RescheduleAppointmentAsync(int userId, string userRole, int appointmentId, RescheduleAppointmentDto dto);
}

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _db;
    private readonly IAppointmentSlotService _slotService;

    public AppointmentService(AppDbContext db, IAppointmentSlotService slotService)
    {
        _db = db;
        _slotService = slotService;
    }

    public Task<List<TimeSlotDto>> GetAvailableSlotsAsync(int lawyerProfileId, DateTime date)
        => _slotService.GetAvailableSlotsAsync(lawyerProfileId, date);

    public async Task<(bool Success, string Message, AppointmentDto? Data)> BookAppointmentAsync(int clientUserId, BookAppointmentDto dto)
    {
        var clientProfile = await _db.ClientProfiles
            .FirstOrDefaultAsync(c => c.UserId == clientUserId);

        if (clientProfile == null)
            return (false, "Client profile not found.", null);

        var lawyerProfile = await _db.LawyerProfiles
            .FirstOrDefaultAsync(l => l.Id == dto.LawyerId && l.IsVerified && l.IsAvailable);

        if (lawyerProfile == null)
            return (false, "Lawyer not found or unavailable.", null);

        // Check slot availability
        var slotTaken = await _db.Appointments.AnyAsync(a =>
            a.LawyerProfileId == dto.LawyerId
            && a.AppointmentDate.Date == dto.AppointmentDate.Date
            && a.StartTime == dto.StartTime
            && (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed));

        if (slotTaken)
            return (false, "This time slot is no longer available.", null);

        // Prevent booking in the past
        var appointmentDateTime = dto.AppointmentDate.Date + dto.StartTime;
        if (appointmentDateTime <= DateTime.UtcNow)
            return (false, "Cannot book appointments in the past.", null);

        var commission = await _db.CommissionSettings.FirstOrDefaultAsync();
        var commissionPct = commission?.DefaultCommissionPercentage ?? 10;
        var platformCommission = Math.Round(lawyerProfile.ConsultationFee * commissionPct / 100, 2);

        var sessionDuration = TimeSpan.FromMinutes(await _slotService.GetSessionDurationAsync(dto.LawyerId));

        var appointment = new Appointment
        {
            LawyerProfileId = dto.LawyerId,
            ClientProfileId = clientProfile.Id,
            AppointmentDate = dto.AppointmentDate.Date,
            StartTime = dto.StartTime,
            EndTime = dto.StartTime + sessionDuration,
            Status = AppointmentStatus.Pending,
            ConsultationFee = lawyerProfile.ConsultationFee,
            PlatformCommission = platformCommission,
            TotalAmount = lawyerProfile.ConsultationFee + platformCommission,
            Notes = dto.Notes
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        // Reload with navigation
        await _db.Entry(appointment).Reference(a => a.LawyerProfile).LoadAsync();
        await _db.Entry(appointment.LawyerProfile).Reference(l => l.User).LoadAsync();
        await _db.Entry(appointment).Reference(a => a.ClientProfile).LoadAsync();
        await _db.Entry(appointment.ClientProfile).Reference(c => c.User).LoadAsync();

        return (true, "Appointment booked successfully.", MapToDto(appointment));
    }

    public async Task<PagedResult<AppointmentDto>> GetMyAppointmentsAsync(int clientUserId, int page, int pageSize)
    {
        var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null)
            return new PagedResult<AppointmentDto> { PageNumber = page, PageSize = pageSize };

        var query = _db.Appointments
            .Include(a => a.LawyerProfile).ThenInclude(l => l.User)
            .Include(a => a.ClientProfile).ThenInclude(c => c.User)
            .Where(a => a.ClientProfileId == clientProfile.Id)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.StartTime);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<AppointmentDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<(bool Success, string Message)> CancelAppointmentAsync(int userId, string userRole, int appointmentId, CancelAppointmentDto dto)
    {
        Appointment? appointment;

        if (userRole == "Client")
        {
            var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (clientProfile == null) return (false, "Profile not found.");
            appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId && a.ClientProfileId == clientProfile.Id);
        }
        else
        {
            var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            if (lawyerProfile == null) return (false, "Profile not found.");
            appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId && a.LawyerProfileId == lawyerProfile.Id);
        }

        if (appointment == null)
            return (false, "Appointment not found.");

        if (appointment.Status == AppointmentStatus.Completed)
            return (false, "Cannot cancel a completed appointment.");

        if (appointment.Status == AppointmentStatus.Cancelled)
            return (false, "Appointment is already cancelled.");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = dto.Reason;
        await _db.SaveChangesAsync();

        return (true, "Appointment cancelled.");
    }

    public async Task<List<AppointmentDto>> GetLawyerAppointmentsAsync(int lawyerUserId, DateTime? from, DateTime? to)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return [];

        var query = _db.Appointments
            .Include(a => a.LawyerProfile).ThenInclude(l => l.User)
            .Include(a => a.ClientProfile).ThenInclude(c => c.User)
            .Where(a => a.LawyerProfileId == lawyerProfile.Id)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(a => a.AppointmentDate >= from.Value.Date);

        if (to.HasValue)
            query = query.Where(a => a.AppointmentDate <= to.Value.Date);

        return await query
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> ConfirmAppointmentAsync(int lawyerUserId, int appointmentId)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return (false, "Profile not found.");

        var appointment = await _db.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.LawyerProfileId == lawyerProfile.Id);

        if (appointment == null) return (false, "Appointment not found.");
        if (appointment.Status != AppointmentStatus.Pending)
            return (false, "Only pending appointments can be confirmed.");

        appointment.Status = AppointmentStatus.Confirmed;
        await _db.SaveChangesAsync();
        return (true, "Appointment confirmed.");
    }

    public async Task<(bool Success, string Message)> CompleteAppointmentAsync(int lawyerUserId, int appointmentId)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return (false, "Profile not found.");

        var appointment = await _db.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.LawyerProfileId == lawyerProfile.Id);

        if (appointment == null) return (false, "Appointment not found.");
        if (appointment.Status != AppointmentStatus.Confirmed)
            return (false, "Only confirmed appointments can be completed.");

        appointment.Status = AppointmentStatus.Completed;
        await _db.SaveChangesAsync();
        return (true, "Appointment marked as completed.");
    }

    public async Task<AppointmentDto?> GetAppointmentByIdAsync(int userId, string userRole, int appointmentId)
    {
        Appointment? appointment = null;

        if (userRole == "Client")
        {
            var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (clientProfile != null)
                appointment = await _db.Appointments
                    .Include(a => a.LawyerProfile).ThenInclude(l => l.User)
                    .Include(a => a.ClientProfile).ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId && a.ClientProfileId == clientProfile.Id);
        }
        else if (userRole == "Lawyer")
        {
            var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            if (lawyerProfile != null)
                appointment = await _db.Appointments
                    .Include(a => a.LawyerProfile).ThenInclude(l => l.User)
                    .Include(a => a.ClientProfile).ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId && a.LawyerProfileId == lawyerProfile.Id);
        }
        else if (userRole == "Admin")
        {
            appointment = await _db.Appointments
                .Include(a => a.LawyerProfile).ThenInclude(l => l.User)
                .Include(a => a.ClientProfile).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }

        return appointment == null ? null : MapToDto(appointment);
    }

    public async Task<(bool Success, string Message)> RescheduleAppointmentAsync(int userId, string userRole, int appointmentId, RescheduleAppointmentDto dto)
    {
        Appointment? appointment = null;

        if (userRole == "Client")
        {
            var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (clientProfile == null) return (false, "Profile not found.");
            appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId && a.ClientProfileId == clientProfile.Id);
        }
        else if (userRole == "Lawyer")
        {
            var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            if (lawyerProfile == null) return (false, "Profile not found.");
            appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId && a.LawyerProfileId == lawyerProfile.Id);
        }

        if (appointment == null) return (false, "Appointment not found.");

        if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Confirmed)
            return (false, "Only pending or confirmed appointments can be rescheduled.");

        if (dto.NewDate.Date < DateTime.UtcNow.Date)
            return (false, "Cannot reschedule to a past date.");

        // Check new slot availability (excluding this appointment)
        var slotTaken = await _db.Appointments.AnyAsync(a =>
            a.Id != appointmentId
            && a.LawyerProfileId == appointment.LawyerProfileId
            && a.AppointmentDate.Date == dto.NewDate.Date
            && a.StartTime == dto.NewStartTime
            && (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed));

        if (slotTaken) return (false, "The selected time slot is not available.");

        var sessionDuration = TimeSpan.FromMinutes(await _slotService.GetSessionDurationAsync(appointment.LawyerProfileId));
        appointment.AppointmentDate = dto.NewDate.Date;
        appointment.StartTime = dto.NewStartTime;
        appointment.EndTime = dto.NewStartTime + sessionDuration;

        await _db.SaveChangesAsync();
        return (true, "Appointment rescheduled.");
    }

    private static AppointmentDto MapToDto(Appointment a) => new()
    {
        Id = a.Id,
        LawyerId = a.LawyerProfileId,
        LawyerName = a.LawyerProfile?.User?.FullName ?? "",
        LawyerProfilePicture = a.LawyerProfile?.User?.ProfilePictureUrl,
        ClientId = a.ClientProfileId,
        ClientName = a.ClientProfile?.User?.FullName ?? "",
        AppointmentDate = a.AppointmentDate,
        StartTime = a.StartTime,
        EndTime = a.EndTime,
        Status = a.Status,
        ConsultationFee = a.ConsultationFee,
        PlatformCommission = a.PlatformCommission,
        TotalAmount = a.TotalAmount,
        Notes = a.Notes,
        CreatedAt = a.CreatedAt
    };
}
