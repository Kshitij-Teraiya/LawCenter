using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Dues;
using LegalConnect.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface ISystemSettingsService
{
    Task<string?> GetValueAsync(string key);
    Task<List<SystemSettingDto>> GetAllAsync();
    Task UpsertAsync(string key, string value, int adminUserId);
}

public class SystemSettingsService : ISystemSettingsService
{
    private readonly AppDbContext _db;

    public SystemSettingsService(AppDbContext db) => _db = db;

    public async Task<string?> GetValueAsync(string key)
    {
        var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }

    public async Task<List<SystemSettingDto>> GetAllAsync()
    {
        return await _db.SystemSettings
            .OrderBy(s => s.Key)
            .Select(s => new SystemSettingDto
            {
                Key         = s.Key,
                Value       = s.Value,
                Description = s.Description,
                UpdatedAt   = s.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task UpsertAsync(string key, string value, int adminUserId)
    {
        var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null)
        {
            setting = new SystemSetting { Key = key };
            _db.SystemSettings.Add(setting);
        }

        setting.Value         = value;
        setting.UpdatedAt     = DateTime.UtcNow;
        setting.UpdatedByUserId = adminUserId;

        await _db.SaveChangesAsync();
    }
}
