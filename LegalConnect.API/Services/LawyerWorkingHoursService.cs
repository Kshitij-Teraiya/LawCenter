using LegalConnect.API.Data;
using LegalConnect.API.DTOs.TimeSlots;
using LegalConnect.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public class LawyerWorkingHoursService : ILawyerWorkingHoursService
{
    private readonly AppDbContext _db;

    public LawyerWorkingHoursService(AppDbContext db) => _db = db;

    private static readonly TimeSpan DefaultStart = new(9, 0, 0);
    private static readonly TimeSpan DefaultEnd = new(18, 0, 0);

    public async Task<List<WorkingHoursDto>> GetWorkingHoursAsync(int lawyerProfileId)
    {
        var hours = await _db.LawyerWorkingHours
            .Where(w => w.LawyerProfileId == lawyerProfileId)
            .OrderBy(w => w.DayOfWeek)
            .ToListAsync();

        // Always return all 7 days; use saved row if it exists, otherwise return defaults
        var result = new List<WorkingHoursDto>();
        for (int day = 0; day <= 6; day++)
        {
            var saved = hours.FirstOrDefault(w => w.DayOfWeek == day);
            bool isWeekday = day >= 1 && day <= 5;
            result.Add(saved != null
                ? new WorkingHoursDto
                {
                    Id = saved.Id,
                    LawyerProfileId = saved.LawyerProfileId,
                    DayOfWeek = saved.DayOfWeek,
                    StartTime = saved.StartTime,
                    EndTime = saved.EndTime,
                    IsWorking = saved.IsWorking,
                    CreatedAt = saved.CreatedAt,
                    UpdatedAt = saved.UpdatedAt
                }
                : new WorkingHoursDto
                {
                    Id = 0,
                    LawyerProfileId = lawyerProfileId,
                    DayOfWeek = day,
                    StartTime = DefaultStart,
                    EndTime = DefaultEnd,
                    IsWorking = isWeekday,   // Mon-Fri on, Sat-Sun off
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
        }
        return result;
    }

    public async Task<WorkingHoursDto?> GetWorkingHoursByDayAsync(int lawyerProfileId, int dayOfWeek)
    {
        var wh = await _db.LawyerWorkingHours
            .FirstOrDefaultAsync(w => w.LawyerProfileId == lawyerProfileId && w.DayOfWeek == dayOfWeek);

        if (wh == null) return null;

        return new WorkingHoursDto
        {
            Id = wh.Id,
            LawyerProfileId = wh.LawyerProfileId,
            DayOfWeek = wh.DayOfWeek,
            StartTime = wh.StartTime,
            EndTime = wh.EndTime,
            IsWorking = wh.IsWorking,
            CreatedAt = wh.CreatedAt,
            UpdatedAt = wh.UpdatedAt
        };
    }

    public async Task<(bool Success, string Message)> UpdateWorkingHoursAsync(
        int lawyerProfileId, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, bool isWorking)
    {
        if (dayOfWeek < 0 || dayOfWeek > 6)
            return (false, "Invalid day of week. Use 0 (Sunday) to 6 (Saturday).");

        if (isWorking && startTime >= endTime)
            return (false, "Start time must be before end time.");

        var wh = await _db.LawyerWorkingHours
            .FirstOrDefaultAsync(w => w.LawyerProfileId == lawyerProfileId && w.DayOfWeek == dayOfWeek);

        if (wh == null)
        {
            wh = new LawyerWorkingHours
            {
                LawyerProfileId = lawyerProfileId,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime,
                IsWorking = isWorking,
                CreatedAt = DateTime.UtcNow
            };
            _db.LawyerWorkingHours.Add(wh);
        }
        else
        {
            wh.StartTime = startTime;
            wh.EndTime = endTime;
            wh.IsWorking = isWorking;
            wh.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return (true, "Working hours updated successfully.");
    }

    public async Task<(bool Success, string Message)> SetDefaultHoursAsync(int lawyerProfileId)
    {
        // Default: Mon-Fri 9:00 AM – 6:00 PM, Sat-Sun off
        var workStart = new TimeSpan(9, 0, 0);
        var workEnd = new TimeSpan(18, 0, 0);

        var existingHours = await _db.LawyerWorkingHours
            .Where(w => w.LawyerProfileId == lawyerProfileId)
            .ToListAsync();

        _db.LawyerWorkingHours.RemoveRange(existingHours);

        for (int day = 0; day <= 6; day++)
        {
            bool isWorkday = day >= 1 && day <= 5; // Mon=1 to Fri=5
            _db.LawyerWorkingHours.Add(new LawyerWorkingHours
            {
                LawyerProfileId = lawyerProfileId,
                DayOfWeek = day,
                StartTime = workStart,
                EndTime = workEnd,
                IsWorking = isWorkday,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        return (true, "Working hours reset to default (Mon-Fri, 9:00 AM – 6:00 PM).");
    }
}
