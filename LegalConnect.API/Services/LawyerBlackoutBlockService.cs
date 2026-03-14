using LegalConnect.API.Data;
using LegalConnect.API.DTOs.TimeSlots;
using LegalConnect.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public class LawyerBlackoutBlockService : ILawyerBlackoutBlockService
{
    private readonly AppDbContext _db;
    private static readonly string[] ValidPatterns = ["None", "Weekly", "MonthlyDate", "Yearly"];

    public LawyerBlackoutBlockService(AppDbContext db) => _db = db;

    public async Task<List<BlackoutBlockDto>> GetBlackoutBlocksAsync(int lawyerProfileId)
    {
        var blocks = await _db.LawyerBlackoutBlocks
            .Where(b => b.LawyerProfileId == lawyerProfileId)
            .OrderBy(b => b.DayOfWeek).ThenBy(b => b.StartTime)
            .ToListAsync();

        return blocks.Select(b => new BlackoutBlockDto
        {
            Id = b.Id,
            LawyerProfileId = b.LawyerProfileId,
            DayOfWeek = b.DayOfWeek,
            StartTime = b.StartTime,
            EndTime = b.EndTime,
            Reason = b.Reason,
            RecurringPattern = b.RecurringPattern,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        }).ToList();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateBlackoutBlockAsync(
        int lawyerProfileId, CreateBlackoutBlockDto dto)
    {
        if (dto.DayOfWeek < 0 || dto.DayOfWeek > 6)
            return (false, "Invalid day of week.", null);

        if (dto.StartTime >= dto.EndTime)
            return (false, "Start time must be before end time.", null);

        if (!ValidPatterns.Contains(dto.RecurringPattern))
            return (false, "Invalid recurring pattern. Use None, Weekly, MonthlyDate, or Yearly.", null);

        var block = new LawyerBlackoutBlock
        {
            LawyerProfileId = lawyerProfileId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Reason = dto.Reason,
            RecurringPattern = dto.RecurringPattern,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.LawyerBlackoutBlocks.Add(block);
        await _db.SaveChangesAsync();
        return (true, "Blackout block created.", block.Id);
    }

    public async Task<(bool Success, string Message)> DeleteBlackoutBlockAsync(int lawyerProfileId, int blockId)
    {
        var block = await _db.LawyerBlackoutBlocks
            .FirstOrDefaultAsync(b => b.Id == blockId && b.LawyerProfileId == lawyerProfileId);

        if (block == null) return (false, "Blackout block not found.");

        _db.LawyerBlackoutBlocks.Remove(block);
        await _db.SaveChangesAsync();
        return (true, "Blackout block deleted.");
    }

    public async Task<bool> IsInBlackoutBlockAsync(int lawyerProfileId, DateTime dateTime, TimeSpan duration)
    {
        var dayOfWeek = (int)dateTime.DayOfWeek;
        var slotStart = dateTime.TimeOfDay;
        var slotEnd = slotStart + duration;

        var blocks = await _db.LawyerBlackoutBlocks
            .Where(b => b.LawyerProfileId == lawyerProfileId && b.DayOfWeek == dayOfWeek)
            .ToListAsync();

        foreach (var block in blocks)
        {
            // Check if slot overlaps with blackout block
            if (slotStart < block.EndTime && slotEnd > block.StartTime)
                return true;
        }

        return false;
    }
}
