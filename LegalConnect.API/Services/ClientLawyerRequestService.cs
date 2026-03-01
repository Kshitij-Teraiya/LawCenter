using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Cases;
using LegalConnect.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface IClientLawyerRequestService
{
    Task<(bool Success, string Message, ClientLawyerRequestDto? Data)> SendRequestAsync(int clientUserId, CreateClientLawyerRequestDto dto);
    Task<List<ClientLawyerRequestDto>> GetMyRequestsAsync(int clientUserId);
    Task<List<ClientLawyerRequestDto>> GetIncomingRequestsAsync(int lawyerUserId);
    Task<(bool Success, string Message)> AcceptRequestAsync(int lawyerUserId, int requestId, string? note);
    Task<(bool Success, string Message)> RejectRequestAsync(int lawyerUserId, int requestId, string? note);
    Task<(bool Success, string Message)> CancelRequestAsync(int clientUserId, int requestId);
}

public class ClientLawyerRequestService : IClientLawyerRequestService
{
    private readonly AppDbContext _db;
    public ClientLawyerRequestService(AppDbContext db) { _db = db; }

    public async Task<(bool Success, string Message, ClientLawyerRequestDto? Data)> SendRequestAsync(int clientUserId, CreateClientLawyerRequestDto dto)
    {
        var clientProfile = await _db.ClientProfiles.Include(c => c.User).FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return (false, "Client profile not found.", null);

        var lawyerProfile = await _db.LawyerProfiles.Include(l => l.User).Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == dto.LawyerProfileId && l.IsVerified);
        if (lawyerProfile == null) return (false, "Lawyer not found or not verified.", null);

        var existingRequest = await _db.ClientLawyerRequests.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.ClientProfileId == clientProfile.Id && r.LawyerProfileId == dto.LawyerProfileId && !r.IsDeleted);

        if (existingRequest != null)
        {
            if (existingRequest.Status == ClientLawyerRequestStatus.Pending)
                return (false, "You already have a pending request to this lawyer.", null);
            if (existingRequest.Status == ClientLawyerRequestStatus.Accepted)
                return (false, "This lawyer has already accepted you as a client.", null);
            existingRequest.Status = ClientLawyerRequestStatus.Pending;
            existingRequest.Message = dto.Message;
            existingRequest.LawyerNote = null;
            existingRequest.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, "Request re-sent successfully.", MapToDto(existingRequest, clientProfile, lawyerProfile));
        }

        var request = new ClientLawyerRequest
        {
            ClientProfileId = clientProfile.Id,
            LawyerProfileId = dto.LawyerProfileId,
            Status = ClientLawyerRequestStatus.Pending,
            Message = dto.Message,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.ClientLawyerRequests.Add(request);
        await _db.SaveChangesAsync();
        return (true, "Request sent successfully.", MapToDto(request, clientProfile, lawyerProfile));
    }
    public async Task<List<ClientLawyerRequestDto>> GetMyRequestsAsync(int clientUserId)
    {
        var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return [];
        return await _db.ClientLawyerRequests
            .Include(r => r.ClientProfile).ThenInclude(c => c.User)
            .Include(r => r.LawyerProfile).ThenInclude(l => l.User)
            .Include(r => r.LawyerProfile).ThenInclude(l => l.Category)
            .Where(r => r.ClientProfileId == clientProfile.Id)
            .OrderByDescending(r => r.UpdatedAt)
            .Select(r => new ClientLawyerRequestDto
            {
                Id = r.Id,
                ClientProfileId = r.ClientProfileId,
                LawyerProfileId = r.LawyerProfileId,
                Status = r.Status.ToString(),
                ClientFullName = r.ClientProfile.User.FirstName + " " + r.ClientProfile.User.LastName,
                ClientEmail = r.ClientProfile.User.Email,
                ClientPicture = r.ClientProfile.User.ProfilePictureUrl,
                LawyerFullName = r.LawyerProfile.User.FirstName + " " + r.LawyerProfile.User.LastName,
                LawyerPicture = r.LawyerProfile.User.ProfilePictureUrl,
                LawyerCity = r.LawyerProfile.City,
                LawyerCategoryName = r.LawyerProfile.Category != null ? r.LawyerProfile.Category.Name : null,
                Message = r.Message,
                LawyerNote = r.LawyerNote,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToListAsync();
    }

    public async Task<List<ClientLawyerRequestDto>> GetIncomingRequestsAsync(int lawyerUserId)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return [];
        return await _db.ClientLawyerRequests
            .Include(r => r.ClientProfile).ThenInclude(c => c.User)
            .Include(r => r.LawyerProfile).ThenInclude(l => l.User)
            .Include(r => r.LawyerProfile).ThenInclude(l => l.Category)
            .Where(r => r.LawyerProfileId == lawyerProfile.Id)
            .OrderByDescending(r => r.UpdatedAt)
            .Select(r => new ClientLawyerRequestDto
            {
                Id = r.Id,
                ClientProfileId = r.ClientProfileId,
                LawyerProfileId = r.LawyerProfileId,
                Status = r.Status.ToString(),
                ClientFullName = r.ClientProfile.User.FirstName + " " + r.ClientProfile.User.LastName,
                ClientEmail = r.ClientProfile.User.Email,
                ClientPicture = r.ClientProfile.User.ProfilePictureUrl,
                LawyerFullName = r.LawyerProfile.User.FirstName + " " + r.LawyerProfile.User.LastName,
                LawyerPicture = r.LawyerProfile.User.ProfilePictureUrl,
                LawyerCity = r.LawyerProfile.City,
                LawyerCategoryName = r.LawyerProfile.Category != null ? r.LawyerProfile.Category.Name : null,
                Message = r.Message,
                LawyerNote = r.LawyerNote,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToListAsync();
    }
    public async Task<(bool Success, string Message)> AcceptRequestAsync(int lawyerUserId, int requestId, string? note)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return (false, "Lawyer profile not found.");
        var request = await _db.ClientLawyerRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.LawyerProfileId == lawyerProfile.Id);
        if (request == null) return (false, "Request not found.");
        if (request.Status != ClientLawyerRequestStatus.Pending) return (false, "Request is no longer pending.");

        request.Status = ClientLawyerRequestStatus.Accepted;
        request.LawyerNote = note;
        request.UpdatedAt = DateTime.UtcNow;

        var existingLawyerClient = await _db.LawyerClients.IgnoreQueryFilters()
            .FirstOrDefaultAsync(lc => lc.LawyerProfileId == lawyerProfile.Id && lc.ClientProfileId == request.ClientProfileId);
        if (existingLawyerClient == null)
        {
            _db.LawyerClients.Add(new LawyerClient
            {
                LawyerProfileId = lawyerProfile.Id,
                ClientProfileId = request.ClientProfileId,
                IsActive = true,
                AddedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            });
        }
        else if (existingLawyerClient.IsDeleted || !existingLawyerClient.IsActive)
        {
            existingLawyerClient.IsDeleted = false;
            existingLawyerClient.IsActive = true;
            existingLawyerClient.ModifiedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return (true, "Request accepted. Client added to your client list.");
    }

    public async Task<(bool Success, string Message)> RejectRequestAsync(int lawyerUserId, int requestId, string? note)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return (false, "Lawyer profile not found.");
        var request = await _db.ClientLawyerRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.LawyerProfileId == lawyerProfile.Id);
        if (request == null) return (false, "Request not found.");
        if (request.Status != ClientLawyerRequestStatus.Pending) return (false, "Request is no longer pending.");
        request.Status = ClientLawyerRequestStatus.Rejected;
        request.LawyerNote = note;
        request.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, "Request rejected.");
    }

    public async Task<(bool Success, string Message)> CancelRequestAsync(int clientUserId, int requestId)
    {
        var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return (false, "Client profile not found.");
        var request = await _db.ClientLawyerRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.ClientProfileId == clientProfile.Id);
        if (request == null) return (false, "Request not found.");
        if (request.Status != ClientLawyerRequestStatus.Pending) return (false, "Only pending requests can be cancelled.");
        request.IsDeleted = true;
        request.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, "Request cancelled.");
    }

    private static ClientLawyerRequestDto MapToDto(ClientLawyerRequest r, ClientProfile client, LawyerProfile lawyer) => new()
    {
        Id = r.Id, ClientProfileId = r.ClientProfileId, LawyerProfileId = r.LawyerProfileId,
        Status = r.Status.ToString(),
        ClientFullName = client.User.FirstName + " " + client.User.LastName,
        ClientEmail = client.User.Email, ClientPicture = client.User.ProfilePictureUrl,
        LawyerFullName = lawyer.User.FirstName + " " + lawyer.User.LastName,
        LawyerPicture = lawyer.User.ProfilePictureUrl, LawyerCity = lawyer.City,
        LawyerCategoryName = lawyer.Category?.Name, Message = r.Message, LawyerNote = r.LawyerNote,
        CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt
    };
}
