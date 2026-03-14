using LegalConnect.API.Data;
using LegalConnect.API.DTOs.TimeSlots;
using LegalConnect.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public class LawyerTimeSlotConfigurationService : ILawyerTimeSlotConfigurationService
{
    private readonly AppDbContext _db;
    private static readonly int[] ValidDurations = [15, 30, 45, 60];

    public LawyerTimeSlotConfigurationService(AppDbContext db) => _db = db;

    public async Task<TimeSlotConfigurationDto?> GetConfigurationAsync(int lawyerProfileId)
    {
        var config = await _db.LawyerTimeSlotConfigurations
            .FirstOrDefaultAsync(c => c.LawyerProfileId == lawyerProfileId);

        if (config == null) return null;

        return new TimeSlotConfigurationDto
        {
            LawyerProfileId = config.LawyerProfileId,
            SessionDurationMinutes = config.SessionDurationMinutes,
            BufferTimeMinutes = config.BufferTimeMinutes,
            CreatedAt = config.CreatedAt,
            UpdatedAt = config.UpdatedAt
        };
    }

    public async Task<(bool Success, string Message)> UpdateConfigurationAsync(
        int lawyerProfileId, int sessionDurationMinutes, int bufferTimeMinutes)
    {
        if (!ValidDurations.Contains(sessionDurationMinutes))
            return (false, "Session duration must be 15, 30, 45, or 60 minutes.");

        if (bufferTimeMinutes < 0 || bufferTimeMinutes > 60)
            return (false, "Buffer time must be between 0 and 60 minutes.");

        var config = await _db.LawyerTimeSlotConfigurations
            .FirstOrDefaultAsync(c => c.LawyerProfileId == lawyerProfileId);

        if (config == null)
        {
            config = new LawyerTimeSlotConfiguration
            {
                LawyerProfileId = lawyerProfileId,
                SessionDurationMinutes = sessionDurationMinutes,
                BufferTimeMinutes = bufferTimeMinutes,
                CreatedAt = DateTime.UtcNow
            };
            _db.LawyerTimeSlotConfigurations.Add(config);
        }
        else
        {
            config.SessionDurationMinutes = sessionDurationMinutes;
            config.BufferTimeMinutes = bufferTimeMinutes;
            config.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return (true, "Configuration updated successfully.");
    }

    public async Task<(bool Success, string Message)> CreateDefaultConfigurationAsync(int lawyerProfileId)
    {
        var exists = await _db.LawyerTimeSlotConfigurations
            .AnyAsync(c => c.LawyerProfileId == lawyerProfileId);

        if (exists) return (true, "Configuration already exists.");

        _db.LawyerTimeSlotConfigurations.Add(new LawyerTimeSlotConfiguration
        {
            LawyerProfileId = lawyerProfileId,
            SessionDurationMinutes = 60,
            BufferTimeMinutes = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return (true, "Default configuration created.");
    }
}
