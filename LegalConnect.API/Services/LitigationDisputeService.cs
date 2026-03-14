using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Dues;
using LegalConnect.API.Entities;
using LegalConnect.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface ILitigationDisputeService
{
    Task<(bool Success, string Message, LitigationDisputeDto? Data)> RaiseDisputeAsync(int clientUserId, RaiseDisputeDto dto);
    Task<(bool Success, string Message)> AdminApproveAsync(int adminUserId, int disputeId, bool approve);
    Task<(bool Success, string Message)> LawyerApproveAsync(int lawyerUserId, int disputeId, bool approve);
    Task<PagedResult<LitigationDisputeDto>> GetDisputesAsync(int userId, string role, DisputeFilterDto filter);
    Task<LitigationDisputeDto?> GetByIdAsync(int userId, string role, int disputeId);
}

public class LitigationDisputeService : ILitigationDisputeService
{
    private readonly AppDbContext _db;
    private readonly IDuesService _duesService;

    public LitigationDisputeService(AppDbContext db, IDuesService duesService)
    {
        _db         = db;
        _duesService = duesService;
    }

    public async Task<(bool Success, string Message, LitigationDisputeDto? Data)> RaiseDisputeAsync(
        int clientUserId, RaiseDisputeDto dto)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Deal).ThenInclude(d => d.ClientProfile)
            .FirstOrDefaultAsync(i => i.Id == dto.InvoiceId);

        if (invoice == null) return (false, "Invoice not found.", null);
        if (invoice.Deal.ClientProfile.UserId != clientUserId)
            return (false, "Access denied.", null);
        if (invoice.Status != InvoiceStatus.Accepted && invoice.Status != InvoiceStatus.Paid)
            return (false, "Disputes can only be raised on accepted or paid invoices.", null);

        var existing = await _db.LitigationDisputes
            .FirstOrDefaultAsync(d => d.InvoiceId == dto.InvoiceId
                && d.Status != DisputeStatus.Rejected);
        if (existing != null)
            return (false, "A dispute already exists for this invoice.", null);

        if (dto.DisputedAmount <= 0 || dto.DisputedAmount > invoice.TotalAmount)
            return (false, $"Disputed amount must be between 0 and ₹{invoice.TotalAmount:N2}.", null);

        var dispute = new LitigationDispute
        {
            InvoiceId      = dto.InvoiceId,
            ClientUserId   = clientUserId,
            DisputeType    = dto.DisputeType,
            DisputedAmount = dto.DisputedAmount,
            Reason         = dto.Reason,
            Status         = DisputeStatus.Pending,
            CreatedAt      = DateTime.UtcNow
        };
        _db.LitigationDisputes.Add(dispute);
        await _db.SaveChangesAsync();

        return (true, "Dispute raised successfully.", await ToDto(dispute));
    }

    public async Task<(bool Success, string Message)> AdminApproveAsync(
        int adminUserId, int disputeId, bool approve)
    {
        var dispute = await _db.LitigationDisputes
            .Include(d => d.Invoice).ThenInclude(i => i.Deal).ThenInclude(d => d.LawyerProfile)
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null) return (false, "Dispute not found.");
        if (dispute.AdminApproved) return (false, "Already approved by admin.");

        if (!approve)
        {
            dispute.Status = DisputeStatus.Rejected;
            await _db.SaveChangesAsync();
            return (true, "Dispute rejected.");
        }

        dispute.AdminApproved  = true;
        dispute.AdminApprovedAt = DateTime.UtcNow;
        dispute.AdminUserId    = adminUserId;
        dispute.Status = dispute.LawyerApproved ? DisputeStatus.BothApproved : DisputeStatus.AdminApproved;
        await _db.SaveChangesAsync();

        if (dispute.Status == DisputeStatus.BothApproved)
            await CreateDuesEntryAsync(dispute, adminUserId);

        return (true, "Dispute approved by admin.");
    }

    public async Task<(bool Success, string Message)> LawyerApproveAsync(
        int lawyerUserId, int disputeId, bool approve)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return (false, "Lawyer profile not found.");

        var dispute = await _db.LitigationDisputes
            .Include(d => d.Invoice).ThenInclude(i => i.Deal)
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null) return (false, "Dispute not found.");
        if (dispute.Invoice.Deal.LawyerProfileId != lawyerProfile.Id)
            return (false, "Access denied.");
        if (dispute.LawyerApproved) return (false, "Already approved by lawyer.");

        if (!approve)
        {
            dispute.Status = DisputeStatus.Rejected;
            await _db.SaveChangesAsync();
            return (true, "Dispute rejected.");
        }

        dispute.LawyerApproved  = true;
        dispute.LawyerApprovedAt = DateTime.UtcNow;
        dispute.Status = dispute.AdminApproved ? DisputeStatus.BothApproved : DisputeStatus.LawyerApproved;
        await _db.SaveChangesAsync();

        if (dispute.Status == DisputeStatus.BothApproved)
            await CreateDuesEntryAsync(dispute, lawyerUserId);

        return (true, "Dispute approved by lawyer.");
    }

    public async Task<PagedResult<LitigationDisputeDto>> GetDisputesAsync(
        int userId, string role, DisputeFilterDto filter)
    {
        var query = _db.LitigationDisputes
            .Include(d => d.Invoice).ThenInclude(i => i.Deal)
                .ThenInclude(deal => deal.LawyerProfile).ThenInclude(l => l.User)
            .AsQueryable();

        if (role == "Client")
            query = query.Where(d => d.ClientUserId == userId);
        else if (role == "Lawyer")
        {
            var lp = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            if (lp != null)
                query = query.Where(d => d.Invoice.Deal.LawyerProfileId == lp.Id);
            else
                query = query.Where(_ => false);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(d => d.Status == filter.Status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = new List<LitigationDisputeDto>();
        foreach (var d in items)
            dtos.Add(await ToDto(d));

        return new PagedResult<LitigationDisputeDto>
        {
            Items      = dtos,
            TotalCount = total,
            PageNumber = filter.Page,
            PageSize   = filter.PageSize
        };
    }

    public async Task<LitigationDisputeDto?> GetByIdAsync(int userId, string role, int disputeId)
    {
        var dispute = await _db.LitigationDisputes
            .Include(d => d.Invoice).ThenInclude(i => i.Deal)
                .ThenInclude(deal => deal.LawyerProfile).ThenInclude(l => l.User)
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null) return null;
        if (role is "Admin" or "AdminStaff") return await ToDto(dispute);

        if (role == "Client" && dispute.ClientUserId != userId) return null;
        if (role == "Lawyer")
        {
            var lp = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            if (lp == null || dispute.Invoice.Deal.LawyerProfileId != lp.Id) return null;
        }

        return await ToDto(dispute);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task CreateDuesEntryAsync(LitigationDispute dispute, int createdByUserId)
    {
        var existing = await _db.DuesEntries
            .AnyAsync(e => e.LitigationDisputeId == dispute.Id);
        if (existing) return;

        var lawyerProfileId = dispute.Invoice.Deal.LawyerProfileId;
        await _duesService.AddEntryAsync(new CreateDuesEntryInternal
        {
            LawyerProfileId     = lawyerProfileId,
            EntryType           = DuesEntryType.DisputeDebit,
            Amount              = dispute.DisputedAmount,
            Description         = $"Dispute #{dispute.Id}: {dispute.DisputeType} on Invoice {dispute.Invoice.InvoiceNumber}",
            CreatedByUserId     = createdByUserId,
            InvoiceId           = dispute.InvoiceId,
            LitigationDisputeId = dispute.Id
        });
    }

    private async Task<LitigationDisputeDto> ToDto(LitigationDispute d)
    {
        string lawyerName = string.Empty;
        string clientName = string.Empty;

        if (d.Invoice?.Deal?.LawyerProfile?.User != null)
            lawyerName = $"{d.Invoice.Deal.LawyerProfile.User.FirstName} {d.Invoice.Deal.LawyerProfile.User.LastName}";

        var clientUser = await _db.Users.FindAsync(d.ClientUserId);
        if (clientUser != null)
            clientName = $"{clientUser.FirstName} {clientUser.LastName}";

        return new LitigationDisputeDto
        {
            Id              = d.Id,
            InvoiceId       = d.InvoiceId,
            InvoiceNumber   = d.Invoice?.InvoiceNumber ?? string.Empty,
            DisputeType     = d.DisputeType,
            DisputedAmount  = d.DisputedAmount,
            Reason          = d.Reason,
            Status          = d.Status,
            AdminApproved   = d.AdminApproved,
            AdminApprovedAt = d.AdminApprovedAt,
            LawyerApproved  = d.LawyerApproved,
            LawyerApprovedAt = d.LawyerApprovedAt,
            CreatedAt       = d.CreatedAt,
            LawyerName      = lawyerName,
            ClientName      = clientName
        };
    }
}
