using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Cases;
using LegalConnect.API.Entities;
using LegalConnect.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface ILawyerClientService
{
    Task<PagedResult<LawyerClientDto>> GetMyClientsAsync(int lawyerUserId, LawyerClientFilterDto filter);
    Task<LawyerClientDto?> GetClientByIdAsync(int lawyerUserId, int lawyerClientId);
    Task<List<EligibleClientDto>> GetEligibleClientsAsync(int lawyerUserId);
    Task<(bool Success, string Message, LawyerClientDto? Data)> AddClientAsync(int lawyerUserId, AddLawyerClientDto dto);
    Task<(bool Success, string Message)> UpdateNotesAsync(int lawyerUserId, int lawyerClientId, UpdateLawyerClientNotesDto dto);
    Task<(bool Success, string Message)> RemoveClientAsync(int lawyerUserId, int lawyerClientId);
}

public class LawyerClientService : ILawyerClientService
{
    private readonly AppDbContext _db;

    public LawyerClientService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<LawyerClientDto>> GetMyClientsAsync(int lawyerUserId, LawyerClientFilterDto filter)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null)
            return new PagedResult<LawyerClientDto> { PageNumber = filter.PageNumber, PageSize = filter.PageSize };

        var query = _db.LawyerClients
            .Include(lc => lc.ClientProfile).ThenInclude(cp => cp.User)
            .Include(lc => lc.Cases)
            .Where(lc => lc.LawyerProfileId == lawyerProfile.Id && !lc.IsDeleted)
            .AsQueryable();

        if (filter.IsActive.HasValue)
            query = query.Where(lc => lc.IsActive == filter.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(lc =>
                lc.ClientProfile.User.FirstName.ToLower().Contains(term) ||
                lc.ClientProfile.User.LastName.ToLower().Contains(term) ||
                lc.ClientProfile.User.PhoneNumber!.Contains(term) ||
                lc.ClientProfile.User.Email!.ToLower().Contains(term));
        }

        var allClients = await query.ToListAsync();

        // Sort
        allClients = filter.SortBy.ToLower() switch
        {
            "name" => filter.SortDescending
                ? allClients.OrderByDescending(lc => lc.ClientProfile.User.FullName).ToList()
                : allClients.OrderBy(lc => lc.ClientProfile.User.FullName).ToList(),
            "cases" => filter.SortDescending
                ? allClients.OrderByDescending(lc => lc.Cases.Count).ToList()
                : allClients.OrderBy(lc => lc.Cases.Count).ToList(),
            _ => filter.SortDescending
                ? allClients.OrderByDescending(lc => lc.AddedDate).ToList()
                : allClients.OrderBy(lc => lc.AddedDate).ToList()
        };

        var totalCount = allClients.Count;
        var items = allClients
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(MapToDto)
            .ToList();

        return new PagedResult<LawyerClientDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<LawyerClientDto?> GetClientByIdAsync(int lawyerUserId, int lawyerClientId)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return null;

        var lawyerClient = await _db.LawyerClients
            .Include(lc => lc.ClientProfile).ThenInclude(cp => cp.User)
            .Include(lc => lc.Cases)
            .FirstOrDefaultAsync(lc =>
                lc.Id == lawyerClientId &&
                lc.LawyerProfileId == lawyerProfile.Id &&
                !lc.IsDeleted);

        return lawyerClient == null ? null : MapToDto(lawyerClient);
    }

    public async Task<List<EligibleClientDto>> GetEligibleClientsAsync(int lawyerUserId)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return [];

        // Clients from completed appointments who haven't been added yet
        var existingClientIds = await _db.LawyerClients
            .Where(lc => lc.LawyerProfileId == lawyerProfile.Id && !lc.IsDeleted)
            .Select(lc => lc.ClientProfileId)
            .ToListAsync();

        var completedAppointments = await _db.Appointments
            .Include(a => a.ClientProfile).ThenInclude(cp => cp.User)
            .Where(a => a.LawyerProfileId == lawyerProfile.Id
                && a.Status == AppointmentStatus.Completed)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        // Group by client, take most recent appointment
        return completedAppointments
            .GroupBy(a => a.ClientProfileId)
            .Select(g =>
            {
                var latest = g.OrderByDescending(a => a.AppointmentDate).First();
                return new EligibleClientDto
                {
                    AppointmentId = latest.Id,
                    ClientProfileId = latest.ClientProfileId,
                    ClientFullName = latest.ClientProfile.User.FullName,
                    ClientEmail = latest.ClientProfile.User.Email ?? "",
                    ClientPhone = latest.ClientProfile.User.PhoneNumber,
                    ProfilePictureUrl = latest.ClientProfile.User.ProfilePictureUrl,
                    AppointmentDate = latest.AppointmentDate,
                    AlreadyAdded = existingClientIds.Contains(latest.ClientProfileId)
                };
            })
            .ToList();
    }

    public async Task<(bool Success, string Message, LawyerClientDto? Data)> AddClientAsync(int lawyerUserId, AddLawyerClientDto dto)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null)
            return (false, "Lawyer profile not found.", null);

        // Verify client exists
        var clientProfile = await _db.ClientProfiles
            .Include(cp => cp.User)
            .FirstOrDefaultAsync(cp => cp.Id == dto.ClientProfileId);
        if (clientProfile == null)
            return (false, "Client not found.", null);

        // Check if already added (including soft-deleted)
        var existing = await _db.LawyerClients
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(lc =>
                lc.LawyerProfileId == lawyerProfile.Id &&
                lc.ClientProfileId == dto.ClientProfileId);

        if (existing != null && !existing.IsDeleted)
            return (false, "Client is already in your client list.", null);

        // Verify the appointment exists and is completed (if provided)
        if (dto.FirstAppointmentId.HasValue)
        {
            var appt = await _db.Appointments.FirstOrDefaultAsync(a =>
                a.Id == dto.FirstAppointmentId.Value &&
                a.LawyerProfileId == lawyerProfile.Id &&
                a.ClientProfileId == dto.ClientProfileId &&
                a.Status == AppointmentStatus.Completed);
            if (appt == null)
                return (false, "Appointment not found or not completed.", null);
        }
        else
        {
            // Auto-find first completed appointment
            var appt = await _db.Appointments
                .Where(a => a.LawyerProfileId == lawyerProfile.Id &&
                            a.ClientProfileId == dto.ClientProfileId &&
                            a.Status == AppointmentStatus.Completed)
                .OrderBy(a => a.AppointmentDate)
                .FirstOrDefaultAsync();
            if (appt == null)
                return (false, "No completed appointment found with this client.", null);
            dto.FirstAppointmentId = appt.Id;
        }

        LawyerClient lawyerClient;
        if (existing != null && existing.IsDeleted)
        {
            // Reactivate
            existing.IsDeleted = false;
            existing.IsActive = true;
            existing.Notes = dto.Notes;
            existing.AddedDate = DateTime.UtcNow;
            existing.ModifiedAt = DateTime.UtcNow;
            lawyerClient = existing;
        }
        else
        {
            lawyerClient = new LawyerClient
            {
                LawyerProfileId = lawyerProfile.Id,
                ClientProfileId = dto.ClientProfileId,
                FirstAppointmentId = dto.FirstAppointmentId,
                Notes = dto.Notes
            };
            _db.LawyerClients.Add(lawyerClient);
        }

        await _db.SaveChangesAsync();

        // Reload with navigations
        await _db.Entry(lawyerClient).Reference(lc => lc.ClientProfile).LoadAsync();
        await _db.Entry(lawyerClient.ClientProfile).Reference(cp => cp.User).LoadAsync();

        return (true, "Client added successfully.", MapToDto(lawyerClient));
    }

    public async Task<(bool Success, string Message)> UpdateNotesAsync(int lawyerUserId, int lawyerClientId, UpdateLawyerClientNotesDto dto)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return (false, "Profile not found.");

        var lawyerClient = await _db.LawyerClients
            .FirstOrDefaultAsync(lc =>
                lc.Id == lawyerClientId &&
                lc.LawyerProfileId == lawyerProfile.Id &&
                !lc.IsDeleted);

        if (lawyerClient == null) return (false, "Client relationship not found.");

        lawyerClient.Notes = dto.Notes;
        lawyerClient.IsActive = dto.IsActive;
        lawyerClient.ModifiedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, "Client notes updated.");
    }

    public async Task<(bool Success, string Message)> RemoveClientAsync(int lawyerUserId, int lawyerClientId)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return (false, "Profile not found.");

        var lawyerClient = await _db.LawyerClients
            .FirstOrDefaultAsync(lc =>
                lc.Id == lawyerClientId &&
                lc.LawyerProfileId == lawyerProfile.Id &&
                !lc.IsDeleted);

        if (lawyerClient == null) return (false, "Client relationship not found.");

        // Soft delete
        lawyerClient.IsDeleted = true;
        lawyerClient.IsActive = false;
        lawyerClient.ModifiedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, "Client removed from your list.");
    }

    private static LawyerClientDto MapToDto(LawyerClient lc) => new()
    {
        Id = lc.Id,
        LawyerProfileId = lc.LawyerProfileId,
        ClientProfileId = lc.ClientProfileId,
        FirstAppointmentId = lc.FirstAppointmentId,
        ClientFullName = lc.ClientProfile?.User?.FullName ?? "",
        ClientEmail = lc.ClientProfile?.User?.Email ?? "",
        ClientPhone = lc.ClientProfile?.User?.PhoneNumber,
        ClientCity = lc.ClientProfile?.City,
        ProfilePictureUrl = lc.ClientProfile?.User?.ProfilePictureUrl,
        AddedDate = lc.AddedDate,
        Notes = lc.Notes,
        IsActive = lc.IsActive,
        TotalCases = lc.Cases?.Count ?? 0,
        OpenCases = lc.Cases?.Count(c => c.Status != CaseStatus.Closed) ?? 0,
        LastActivityDate = lc.ModifiedAt
    };
}
