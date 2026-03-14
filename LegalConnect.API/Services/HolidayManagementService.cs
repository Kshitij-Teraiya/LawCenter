using LegalConnect.API.Data;
using LegalConnect.API.DTOs.TimeSlots;
using LegalConnect.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public class HolidayManagementService : IHolidayManagementService
{
    private readonly AppDbContext _db;
    private static readonly string[] ValidPatterns = ["None", "Weekly", "MonthlyDate", "Yearly"];

    public HolidayManagementService(AppDbContext db) => _db = db;

    // ── Admin: Master Holidays ────────────────────────────────────────────────

    public async Task<List<MasterHolidayDto>> GetMasterHolidaysAsync()
    {
        var holidays = await _db.MasterHolidays
            .Include(h => h.LawyerPreferences)
            .OrderBy(h => h.HolidayDate)
            .ToListAsync();

        return holidays.Select(h => new MasterHolidayDto
        {
            Id = h.Id,
            HolidayName = h.HolidayName,
            HolidayDate = h.HolidayDate,
            Description = h.Description,
            AppliesYearly = h.AppliesYearly,
            LawyersObservingCount = h.LawyerPreferences.Count(p => p.IsEnabled),
            CreatedAt = h.CreatedAt
        }).ToList();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateMasterHolidayAsync(CreateMasterHolidayDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.HolidayName))
            return (false, "Holiday name is required.", null);

        var holiday = new MasterHoliday
        {
            HolidayName = dto.HolidayName.Trim(),
            HolidayDate = dto.HolidayDate.Date,
            Description = dto.Description,
            AppliesYearly = dto.AppliesYearly,
            CreatedAt = DateTime.UtcNow
        };

        _db.MasterHolidays.Add(holiday);
        await _db.SaveChangesAsync();
        return (true, "Master holiday created.", holiday.Id);
    }

    public async Task<(bool Success, string Message)> UpdateMasterHolidayAsync(int id, UpdateMasterHolidayDto dto)
    {
        var holiday = await _db.MasterHolidays.FindAsync(id);
        if (holiday == null) return (false, "Holiday not found.");

        if (string.IsNullOrWhiteSpace(dto.HolidayName))
            return (false, "Holiday name is required.");

        holiday.HolidayName = dto.HolidayName.Trim();
        holiday.HolidayDate = dto.HolidayDate.Date;
        holiday.Description = dto.Description;
        holiday.AppliesYearly = dto.AppliesYearly;

        await _db.SaveChangesAsync();
        return (true, "Master holiday updated.");
    }

    public async Task<(bool Success, string Message)> DeleteMasterHolidayAsync(int id)
    {
        var holiday = await _db.MasterHolidays.FindAsync(id);
        if (holiday == null) return (false, "Holiday not found.");

        _db.MasterHolidays.Remove(holiday);
        await _db.SaveChangesAsync();
        return (true, "Master holiday deleted.");
    }

    // ── Lawyer: Holiday Preferences ──────────────────────────────────────────

    public async Task<List<HolidayPreferenceDto>> GetLawyerHolidayPreferencesAsync(int lawyerProfileId)
    {
        var allHolidays = await _db.MasterHolidays.ToListAsync();
        var preferences = await _db.LawyerHolidayPreferences
            .Where(p => p.LawyerProfileId == lawyerProfileId)
            .ToListAsync();

        return allHolidays.Select(h =>
        {
            var pref = preferences.FirstOrDefault(p => p.MasterHolidayId == h.Id);
            return new HolidayPreferenceDto
            {
                MasterHolidayId = h.Id,
                HolidayName = h.HolidayName,
                HolidayDate = h.HolidayDate,
                Description = h.Description,
                AppliesYearly = h.AppliesYearly,
                IsEnabled = pref?.IsEnabled ?? false
            };
        }).OrderBy(p => p.HolidayDate).ToList();
    }

    public async Task<(bool Success, string Message)> SetHolidayPreferenceAsync(int lawyerProfileId, int masterHolidayId, bool isEnabled)
    {
        var holiday = await _db.MasterHolidays.FindAsync(masterHolidayId);
        if (holiday == null) return (false, "Master holiday not found.");

        var pref = await _db.LawyerHolidayPreferences
            .FirstOrDefaultAsync(p => p.LawyerProfileId == lawyerProfileId && p.MasterHolidayId == masterHolidayId);

        if (pref == null)
        {
            pref = new LawyerHolidayPreference
            {
                LawyerProfileId = lawyerProfileId,
                MasterHolidayId = masterHolidayId,
                IsEnabled = isEnabled,
                CreatedAt = DateTime.UtcNow
            };
            _db.LawyerHolidayPreferences.Add(pref);
        }
        else
        {
            pref.IsEnabled = isEnabled;
        }

        await _db.SaveChangesAsync();
        return (true, isEnabled ? "Holiday enabled." : "Holiday disabled.");
    }

    // ── Lawyer: Personal Holidays ─────────────────────────────────────────────

    public async Task<List<PersonalHolidayDto>> GetPersonalHolidaysAsync(int lawyerProfileId)
    {
        var holidays = await _db.LawyerPersonalHolidays
            .Where(h => h.LawyerProfileId == lawyerProfileId)
            .OrderBy(h => h.HolidayDate)
            .ToListAsync();

        return holidays.Select(h => new PersonalHolidayDto
        {
            Id = h.Id,
            LawyerProfileId = h.LawyerProfileId,
            HolidayDate = h.HolidayDate,
            Reason = h.Reason,
            RecurringPattern = h.RecurringPattern,
            CreatedAt = h.CreatedAt,
            UpdatedAt = h.UpdatedAt
        }).ToList();
    }

    public async Task<(bool Success, string Message, int? Id)> CreatePersonalHolidayAsync(int lawyerProfileId, CreatePersonalHolidayDto dto)
    {
        if (!ValidPatterns.Contains(dto.RecurringPattern))
            return (false, "Invalid recurring pattern. Use None, Weekly, MonthlyDate, or Yearly.", null);

        var holiday = new LawyerPersonalHoliday
        {
            LawyerProfileId = lawyerProfileId,
            HolidayDate = dto.HolidayDate.Date,
            Reason = dto.Reason,
            RecurringPattern = dto.RecurringPattern,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.LawyerPersonalHolidays.Add(holiday);
        await _db.SaveChangesAsync();
        return (true, "Personal holiday created.", holiday.Id);
    }

    public async Task<(bool Success, string Message)> DeletePersonalHolidayAsync(int lawyerProfileId, int holidayId)
    {
        var holiday = await _db.LawyerPersonalHolidays
            .FirstOrDefaultAsync(h => h.Id == holidayId && h.LawyerProfileId == lawyerProfileId);

        if (holiday == null) return (false, "Personal holiday not found.");

        _db.LawyerPersonalHolidays.Remove(holiday);
        await _db.SaveChangesAsync();
        return (true, "Personal holiday deleted.");
    }

    // ── Helper: Is Holiday? ────────────────────────────────────────────────────

    public async Task<bool> IsHolidayAsync(int lawyerProfileId, DateTime date)
    {
        // Check personal holidays
        var personalHolidays = await _db.LawyerPersonalHolidays
            .Where(h => h.LawyerProfileId == lawyerProfileId)
            .ToListAsync();

        foreach (var h in personalHolidays)
        {
            if (MatchesRecurring(h.HolidayDate, date, h.RecurringPattern))
                return true;
        }

        // Check enabled master holidays
        var enabledPrefs = await _db.LawyerHolidayPreferences
            .Include(p => p.MasterHoliday)
            .Where(p => p.LawyerProfileId == lawyerProfileId && p.IsEnabled && p.MasterHoliday != null)
            .ToListAsync();

        foreach (var pref in enabledPrefs)
        {
            var holiday = pref.MasterHoliday!;
            var pattern = holiday.AppliesYearly ? "Yearly" : "None";
            if (MatchesRecurring(holiday.HolidayDate, date, pattern))
                return true;
        }

        return false;
    }

    private static bool MatchesRecurring(DateTime baseDate, DateTime checkDate, string pattern)
    {
        return pattern switch
        {
            "None" => baseDate.Date == checkDate.Date,
            "Weekly" => baseDate.DayOfWeek == checkDate.DayOfWeek,
            "MonthlyDate" => baseDate.Day == checkDate.Day,
            "Yearly" => baseDate.Month == checkDate.Month && baseDate.Day == checkDate.Day,
            _ => false
        };
    }
}
