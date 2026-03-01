using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Client;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface IClientProfileService
{
    Task<ClientProfileDto?> GetMyProfileAsync(int userId);
    Task<(bool Success, string Message)> UpdateMyProfileAsync(int userId, UpdateClientProfileDto dto);
}

public class ClientProfileService : IClientProfileService
{
    private readonly AppDbContext _db;

    public ClientProfileService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ClientProfileDto?> GetMyProfileAsync(int userId)
    {
        var profile = await _db.ClientProfiles
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (profile == null) return null;

        return new ClientProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FirstName = profile.User.FirstName,
            LastName = profile.User.LastName,
            Email = profile.User.Email ?? string.Empty,
            PhoneNumber = profile.User.PhoneNumber,
            City = profile.City,
            ProfilePictureUrl = profile.User.ProfilePictureUrl,
            CreatedAt = profile.CreatedAt,
            TotalCases = await _db.Cases.CountAsync(c => c.ClientProfileId == profile.Id),
            TotalAppointments = await _db.Appointments.CountAsync(a => a.ClientProfileId == profile.Id)
        };
    }

    public async Task<(bool Success, string Message)> UpdateMyProfileAsync(int userId, UpdateClientProfileDto dto)
    {
        var profile = await _db.ClientProfiles
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (profile == null) return (false, "Client profile not found.");

        profile.User.FirstName = dto.FirstName;
        profile.User.LastName = dto.LastName;
        profile.User.PhoneNumber = dto.PhoneNumber;
        profile.City = dto.City;

        await _db.SaveChangesAsync();
        return (true, "Profile updated successfully.");
    }
}
