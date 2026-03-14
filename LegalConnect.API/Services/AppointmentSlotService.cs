using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Appointment;
using LegalConnect.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public class AppointmentSlotService : IAppointmentSlotService
{
    private readonly AppDbContext _db;
    private readonly IHolidayManagementService _holidayService;
    private readonly ILawyerBlackoutBlockService _blackoutService;

    public AppointmentSlotService(
        AppDbContext db,
        IHolidayManagementService holidayService,
        ILawyerBlackoutBlockService blackoutService)
    {
        _db = db;
        _holidayService = holidayService;
        _blackoutService = blackoutService;
    }

    public async Task<int> GetSessionDurationAsync(int lawyerProfileId)
    {
        var config = await _db.LawyerTimeSlotConfigurations
            .FirstOrDefaultAsync(c => c.LawyerProfileId == lawyerProfileId);
        return config?.SessionDurationMinutes ?? 60;
    }

    public async Task<List<TimeSlotDto>> GetAvailableSlotsAsync(int lawyerProfileId, DateTime date)
    {
        // 1. Past date check
        if (date.Date < DateTime.UtcNow.Date)
            return [];

        // 2. Load config (session duration + buffer)
        var config = await _db.LawyerTimeSlotConfigurations
            .FirstOrDefaultAsync(c => c.LawyerProfileId == lawyerProfileId);

        var sessionMinutes = config?.SessionDurationMinutes ?? 60;
        var bufferMinutes = config?.BufferTimeMinutes ?? 0;
        var slotDuration = TimeSpan.FromMinutes(sessionMinutes);
        var bufferDuration = TimeSpan.FromMinutes(bufferMinutes);

        // 3. Load working hours for day of week
        var dayOfWeek = (int)date.DayOfWeek;
        var workingHours = await _db.LawyerWorkingHours
            .FirstOrDefaultAsync(w => w.LawyerProfileId == lawyerProfileId && w.DayOfWeek == dayOfWeek);

        TimeSpan workStart, workEnd;
        if (workingHours == null || !workingHours.IsWorking)
        {
            // Fallback: if no custom hours, use 9-18 Mon-Fri defaults; weekends off
            if (dayOfWeek == 0 || dayOfWeek == 6)
                return [];
            workStart = TimeSpan.FromHours(9);
            workEnd = TimeSpan.FromHours(18);
        }
        else
        {
            if (!workingHours.IsWorking) return [];
            workStart = workingHours.StartTime;
            workEnd = workingHours.EndTime;
        }

        // 4. Check holidays
        if (await _holidayService.IsHolidayAsync(lawyerProfileId, date))
            return [];

        // 5. Load booked appointments (Pending/Confirmed)
        var bookedAppointments = await _db.Appointments
            .Where(a => a.LawyerProfileId == lawyerProfileId
                && a.AppointmentDate.Date == date.Date
                && (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed))
            .Select(a => new { a.StartTime, a.EndTime })
            .ToListAsync();

        // 6. Load blackout blocks for this day
        var blackoutBlocks = await _db.LawyerBlackoutBlocks
            .Where(b => b.LawyerProfileId == lawyerProfileId && b.DayOfWeek == dayOfWeek)
            .ToListAsync();

        // 7. Generate slots
        // Slots advance by (sessionDuration + bufferDuration) so each slot is naturally
        // spaced to respect the lawyer's buffer gap — e.g. 60-min session + 15-min buffer
        // produces slots at 9:00, 10:15, 11:30 … instead of 9:00, 10:00, 11:00.
        // Buffer=0 keeps the original behaviour (9:00, 10:00, 11:00 …).
        var stepDuration = slotDuration + bufferDuration;

        var slots = new List<TimeSlotDto>();
        var current = workStart;

        while (current + slotDuration <= workEnd)
        {
            var slotEnd = current + slotDuration;
            var isAvailable = true;

            // Check booked appointments
            foreach (var appt in bookedAppointments)
            {
                if (current < appt.EndTime && slotEnd > appt.StartTime)
                {
                    isAvailable = false;
                    break;
                }
            }

            // Check blackout blocks
            if (isAvailable)
            {
                foreach (var block in blackoutBlocks)
                {
                    if (current < block.EndTime && slotEnd > block.StartTime)
                    {
                        isAvailable = false;
                        break;
                    }
                }
            }

            slots.Add(new TimeSlotDto
            {
                StartTime = current,
                EndTime = slotEnd,
                IsAvailable = isAvailable
            });

            current += stepDuration;   // advance by session + buffer
        }

        return slots.Where(s => s.IsAvailable).ToList();
    }
}
