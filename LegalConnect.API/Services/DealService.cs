using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Deals;
using LegalConnect.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface IDealService
{
    // HireRequest
    Task<(bool Success, string Message, HireRequestDto? Data)> CreateHireRequestAsync(int clientUserId, CreateHireRequestDto dto);
    Task<List<HireRequestDto>> GetMyHireRequestsAsync(int clientUserId);
    Task<List<HireRequestDto>> GetIncomingHireRequestsAsync(int lawyerUserId);
    Task<HireRequestDetailDto?> GetHireRequestByIdAsync(int userId, string role, int hireRequestId);
    Task<(bool Success, string Message)> RejectHireRequestAsync(int lawyerUserId, int hireRequestId);
    Task<(bool Success, string Message)> CancelHireRequestAsync(int clientUserId, int hireRequestId);

    // Deal
    Task<(bool Success, string Message, DealDto? Data)> AcceptHireRequestAsync(int lawyerUserId, int hireRequestId);

    // Messages
    Task<List<HireRequestMessageDto>> GetMessagesAsync(int userId, int hireRequestId);
    Task<(bool Success, string Message, HireRequestMessageDto? Data)> SendMessageAsync(int userId, string role, string fullName, string? picture, int hireRequestId, SendMessageDto dto);
    Task<(bool Success, string Message)> MarkMessagesReadAsync(int userId, int hireRequestId);

    // Proposals (on Deal)
    Task<(bool Success, string Message, ProposalDto? Data)> CreateProposalAsync(int lawyerUserId, int dealId, CreateProposalDto dto);
    Task<(bool Success, string Message)> AcceptProposalAsync(int clientUserId, int proposalId, string? note);
    Task<(bool Success, string Message)> RejectProposalAsync(int clientUserId, int proposalId, string? note);

    // Invoices (on Deal)
    Task<(bool Success, string Message, InvoiceDto? Data)> GenerateInvoiceAsync(int lawyerUserId, int proposalId, CreateInvoiceDto dto);
    Task<(bool Success, string Message)> AcceptInvoiceAsync(int clientUserId, int invoiceId);
    Task<(bool Success, string Message)> MarkInvoicePaidAsync(int lawyerUserId, int invoiceId);
    Task<(bool Success, string Message)> RejectInvoiceAsync(int clientUserId, int invoiceId);
}

public class DealService : IDealService
{
    private readonly AppDbContext _db;

    public DealService(AppDbContext db)
    {
        _db = db;
    }

    // ── HireRequest ──────────────────────────────────────────────────

    public async Task<(bool Success, string Message, HireRequestDto? Data)> CreateHireRequestAsync(int clientUserId, CreateHireRequestDto dto)
    {
        var clientProfile = await _db.ClientProfiles.Include(c => c.User).FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return (false, "Client profile not found.", null);

        var lawyerProfile = await _db.LawyerProfiles.Include(l => l.User).Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == dto.LawyerProfileId && l.IsVerified);
        if (lawyerProfile == null) return (false, "Lawyer not found or not verified.", null);

        // Check for existing active hire request (same client + same lawyer, not rejected/cancelled/converted)
        var existing = await _db.HireRequests.IgnoreQueryFilters()
            .FirstOrDefaultAsync(h => h.LawyerProfileId == dto.LawyerProfileId
                && h.ClientProfileId == clientProfile.Id && !h.IsDeleted
                && h.Status != HireRequestStatus.Rejected
                && h.Status != HireRequestStatus.ConvertedToCase);
        if (existing != null) return (false, "You already have an active hire request with this lawyer.", null);

        var hireRequest = new HireRequest
        {
            ClientProfileId = clientProfile.Id,
            LawyerProfileId = dto.LawyerProfileId,
            Status = HireRequestStatus.Inquiry,
            Description = dto.Description,
            CaseType = dto.CaseType,
            Court = dto.Court,
            Message = dto.Message,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.HireRequests.Add(hireRequest);
        await _db.SaveChangesAsync();

        return (true, "Hire request sent successfully.", MapToDto(hireRequest, clientProfile, lawyerProfile, 0, 0));
    }

    public async Task<List<HireRequestDto>> GetMyHireRequestsAsync(int clientUserId)
    {
        var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return [];

        return await _db.HireRequests
            .Where(h => h.ClientProfileId == clientProfile.Id)
            .Include(h => h.ClientProfile).ThenInclude(c => c.User)
            .Include(h => h.LawyerProfile).ThenInclude(l => l.User)
            .Include(h => h.LawyerProfile).ThenInclude(l => l.Category)
            .Include(h => h.Messages)
            .Include(h => h.Deal)
            .OrderByDescending(h => h.UpdatedAt)
            .Select(h => new HireRequestDto
            {
                Id = h.Id,
                ClientProfileId = h.ClientProfileId,
                ClientFullName = h.ClientProfile.User.FirstName + " " + h.ClientProfile.User.LastName,
                ClientPicture = h.ClientProfile.User.ProfilePictureUrl,
                LawyerProfileId = h.LawyerProfileId,
                LawyerFullName = h.LawyerProfile.User.FirstName + " " + h.LawyerProfile.User.LastName,
                LawyerPicture = h.LawyerProfile.User.ProfilePictureUrl,
                LawyerCategoryName = h.LawyerProfile.Category != null ? h.LawyerProfile.Category.Name : null,
                Status = h.Status.ToString(),
                Description = h.Description,
                CaseType = h.CaseType,
                Court = h.Court,
                Message = h.Message,
                MessageCount = h.Messages.Count,
                UnreadCount = h.Messages.Count(m => !m.IsRead && m.SenderUserId != clientProfile.UserId),
                HasDeal = h.Deal != null,
                DealId = h.Deal == null ? (int?)null : h.Deal.Id,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt
            }).ToListAsync();
    }

    public async Task<List<HireRequestDto>> GetIncomingHireRequestsAsync(int lawyerUserId)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return [];

        return await _db.HireRequests
            .Where(h => h.LawyerProfileId == lawyerProfile.Id)
            .Include(h => h.ClientProfile).ThenInclude(c => c.User)
            .Include(h => h.LawyerProfile).ThenInclude(l => l.User)
            .Include(h => h.LawyerProfile).ThenInclude(l => l.Category)
            .Include(h => h.Messages)
            .Include(h => h.Deal)
            .OrderByDescending(h => h.UpdatedAt)
            .Select(h => new HireRequestDto
            {
                Id = h.Id,
                ClientProfileId = h.ClientProfileId,
                ClientFullName = h.ClientProfile.User.FirstName + " " + h.ClientProfile.User.LastName,
                ClientPicture = h.ClientProfile.User.ProfilePictureUrl,
                LawyerProfileId = h.LawyerProfileId,
                LawyerFullName = h.LawyerProfile.User.FirstName + " " + h.LawyerProfile.User.LastName,
                LawyerPicture = h.LawyerProfile.User.ProfilePictureUrl,
                LawyerCategoryName = h.LawyerProfile.Category != null ? h.LawyerProfile.Category.Name : null,
                Status = h.Status.ToString(),
                Description = h.Description,
                CaseType = h.CaseType,
                Court = h.Court,
                Message = h.Message,
                MessageCount = h.Messages.Count,
                UnreadCount = h.Messages.Count(m => !m.IsRead && m.SenderUserId != lawyerProfile.UserId),
                HasDeal = h.Deal != null,
                DealId = h.Deal == null ? (int?)null : h.Deal.Id,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt
            }).ToListAsync();
    }

    public async Task<HireRequestDetailDto?> GetHireRequestByIdAsync(int userId, string role, int hireRequestId)
    {
        var hr = await _db.HireRequests
            .Where(h => h.Id == hireRequestId)
            .Include(h => h.ClientProfile).ThenInclude(c => c.User)
            .Include(h => h.LawyerProfile).ThenInclude(l => l.User)
            .Include(h => h.LawyerProfile).ThenInclude(l => l.Category)
            .Include(h => h.Messages)
            .Include(h => h.Deal).ThenInclude(d => d!.Proposals).ThenInclude(p => p.Invoice)
            .Include(h => h.Deal).ThenInclude(d => d!.Invoices)
            .FirstOrDefaultAsync();

        if (hr == null) return null;

        // Verify access
        if (!await HasAccessAsync(userId, role, hr)) return null;

        var result = new HireRequestDetailDto
        {
            Id = hr.Id,
            ClientProfileId = hr.ClientProfileId,
            ClientFullName = hr.ClientProfile.User.FirstName + " " + hr.ClientProfile.User.LastName,
            ClientPicture = hr.ClientProfile.User.ProfilePictureUrl,
            LawyerProfileId = hr.LawyerProfileId,
            LawyerFullName = hr.LawyerProfile.User.FirstName + " " + hr.LawyerProfile.User.LastName,
            LawyerPicture = hr.LawyerProfile.User.ProfilePictureUrl,
            LawyerCategoryName = hr.LawyerProfile.Category?.Name,
            Status = hr.Status.ToString(),
            Description = hr.Description,
            CaseType = hr.CaseType,
            Court = hr.Court,
            Message = hr.Message,
            MessageCount = hr.Messages.Count,
            UnreadCount = hr.Messages.Count(m => !m.IsRead && m.SenderUserId != userId),
            HasDeal = hr.Deal != null,
            DealId = hr.Deal?.Id,
            CreatedAt = hr.CreatedAt,
            UpdatedAt = hr.UpdatedAt
        };

        if (hr.Deal != null)
        {
            result.Deal = new DealDto
            {
                Id = hr.Deal.Id,
                HireRequestId = hr.Deal.HireRequestId,
                Status = hr.Deal.Status.ToString(),
                CreatedAt = hr.Deal.CreatedAt,
                UpdatedAt = hr.Deal.UpdatedAt,
                Proposals = hr.Deal.Proposals.OrderByDescending(p => p.CreatedAt).Select(p => new ProposalDto
                {
                    Id = p.Id,
                    DealId = p.DealId,
                    Title = p.Title,
                    Description = p.Description,
                    Amount = p.Amount,
                    IsNegotiable = p.IsNegotiable,
                    Status = p.Status.ToString(),
                    ClientNote = p.ClientNote,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Invoice = p.Invoice != null ? new InvoiceDto
                    {
                        Id = p.Invoice.Id,
                        ProposalId = p.Invoice.ProposalId,
                        DealId = p.Invoice.DealId,
                        InvoiceNumber = p.Invoice.InvoiceNumber,
                        Amount = p.Invoice.Amount,
                        Description = p.Invoice.Description,
                        DueDate = p.Invoice.DueDate,
                        Status = p.Invoice.Status.ToString(),
                        CreatedAt = p.Invoice.CreatedAt,
                        UpdatedAt = p.Invoice.UpdatedAt
                    } : null
                }).ToList(),
                Invoices = hr.Deal.Invoices.OrderByDescending(i => i.CreatedAt).Select(i => new InvoiceDto
                {
                    Id = i.Id,
                    ProposalId = i.ProposalId,
                    DealId = i.DealId,
                    InvoiceNumber = i.InvoiceNumber,
                    Amount = i.Amount,
                    Description = i.Description,
                    DueDate = i.DueDate,
                    Status = i.Status.ToString(),
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                }).ToList()
            };
        }

        return result;
    }

    public async Task<(bool Success, string Message)> RejectHireRequestAsync(int lawyerUserId, int hireRequestId)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return (false, "Lawyer profile not found.");

        var hr = await _db.HireRequests.FirstOrDefaultAsync(h => h.Id == hireRequestId && h.LawyerProfileId == lawyerProfile.Id);
        if (hr == null) return (false, "Hire request not found.");
        if (hr.Status != HireRequestStatus.Inquiry) return (false, "Only inquiry hire requests can be rejected.");

        hr.Status = HireRequestStatus.Rejected;
        hr.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, "Hire request rejected.");
    }

    public async Task<(bool Success, string Message)> CancelHireRequestAsync(int clientUserId, int hireRequestId)
    {
        var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return (false, "Client profile not found.");

        var hr = await _db.HireRequests.FirstOrDefaultAsync(h => h.Id == hireRequestId && h.ClientProfileId == clientProfile.Id);
        if (hr == null) return (false, "Hire request not found.");
        if (hr.Status == HireRequestStatus.ConvertedToCase) return (false, "Converted hire requests cannot be cancelled.");

        hr.IsDeleted = true;
        hr.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, "Hire request cancelled.");
    }

    // ── Deal (Accept HireRequest → Create Deal) ─────────────────────

    public async Task<(bool Success, string Message, DealDto? Data)> AcceptHireRequestAsync(int lawyerUserId, int hireRequestId)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return (false, "Lawyer profile not found.", null);

        var hr = await _db.HireRequests.Include(h => h.Deal).FirstOrDefaultAsync(h => h.Id == hireRequestId && h.LawyerProfileId == lawyerProfile.Id);
        if (hr == null) return (false, "Hire request not found.", null);
        if (hr.Status != HireRequestStatus.Inquiry) return (false, "Only inquiry hire requests can be accepted.", null);
        if (hr.Deal != null) return (false, "A deal already exists for this hire request.", null);

        hr.Status = HireRequestStatus.AcceptedByLawyer;
        hr.UpdatedAt = DateTime.UtcNow;

        var deal = new Deal
        {
            HireRequestId = hireRequestId,
            ClientProfileId = hr.ClientProfileId,
            LawyerProfileId = hr.LawyerProfileId,
            Status = DealStatus.Negotiation,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Deals.Add(deal);
        await _db.SaveChangesAsync();

        return (true, "Hire request accepted. Deal created.", new DealDto
        {
            Id = deal.Id,
            HireRequestId = deal.HireRequestId,
            Status = deal.Status.ToString(),
            CreatedAt = deal.CreatedAt,
            UpdatedAt = deal.UpdatedAt
        });
    }

    // ── Messages ─────────────────────────────────────────────────────

    public async Task<List<HireRequestMessageDto>> GetMessagesAsync(int userId, int hireRequestId)
    {
        var hr = await _db.HireRequests.FirstOrDefaultAsync(h => h.Id == hireRequestId);
        if (hr == null) return [];

        return await _db.HireRequestMessages
            .Where(m => m.HireRequestId == hireRequestId)
            .Include(m => m.Sender)
            .OrderBy(m => m.SentAt)
            .Select(m => new HireRequestMessageDto
            {
                Id = m.Id,
                HireRequestId = m.HireRequestId,
                SenderUserId = m.SenderUserId,
                SenderRole = m.SenderRole,
                SenderName = m.Sender.FirstName + " " + m.Sender.LastName,
                SenderPicture = m.Sender.ProfilePictureUrl,
                Content = m.Content,
                SentAt = m.SentAt,
                IsRead = m.IsRead
            }).ToListAsync();
    }

    public async Task<(bool Success, string Message, HireRequestMessageDto? Data)> SendMessageAsync(
        int userId, string role, string fullName, string? picture, int hireRequestId, SendMessageDto dto)
    {
        var hr = await _db.HireRequests.FirstOrDefaultAsync(h => h.Id == hireRequestId);
        if (hr == null) return (false, "Hire request not found.", null);
        if (hr.Status == HireRequestStatus.Rejected)
            return (false, "Cannot send messages on a rejected hire request.", null);

        var msg = new HireRequestMessage
        {
            HireRequestId = hireRequestId,
            SenderUserId = userId,
            SenderRole = role,
            Content = dto.Content,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };
        _db.HireRequestMessages.Add(msg);
        hr.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return (true, "Message sent.", new HireRequestMessageDto
        {
            Id = msg.Id,
            HireRequestId = msg.HireRequestId,
            SenderUserId = msg.SenderUserId,
            SenderRole = msg.SenderRole,
            SenderName = fullName,
            SenderPicture = picture,
            Content = msg.Content,
            SentAt = msg.SentAt,
            IsRead = msg.IsRead
        });
    }

    public async Task<(bool Success, string Message)> MarkMessagesReadAsync(int userId, int hireRequestId)
    {
        var unread = await _db.HireRequestMessages
            .Where(m => m.HireRequestId == hireRequestId && m.SenderUserId != userId && !m.IsRead)
            .ToListAsync();

        foreach (var m in unread) m.IsRead = true;
        await _db.SaveChangesAsync();
        return (true, $"Marked {unread.Count} messages as read.");
    }

    // ── Proposals (on Deal) ──────────────────────────────────────────

    public async Task<(bool Success, string Message, ProposalDto? Data)> CreateProposalAsync(int lawyerUserId, int dealId, CreateProposalDto dto)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return (false, "Lawyer profile not found.", null);

        var deal = await _db.Deals.Include(d => d.HireRequest).FirstOrDefaultAsync(d => d.Id == dealId && d.LawyerProfileId == lawyerProfile.Id);
        if (deal == null) return (false, "Deal not found.", null);
        if (deal.Status != DealStatus.Negotiation && deal.Status != DealStatus.ProposalSent)
            return (false, "Proposals can only be sent during negotiation.", null);

        var proposal = new Proposal
        {
            DealId = dealId,
            Title = dto.Title,
            Description = dto.Description,
            Amount = dto.Amount,
            IsNegotiable = dto.IsNegotiable,
            Status = ProposalStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Proposals.Add(proposal);

        if (deal.Status == DealStatus.Negotiation)
        {
            deal.Status = DealStatus.ProposalSent;
            deal.UpdatedAt = DateTime.UtcNow;
        }

        // Update HireRequest status to DealInProgress
        if (deal.HireRequest.Status == HireRequestStatus.AcceptedByLawyer)
        {
            deal.HireRequest.Status = HireRequestStatus.DealInProgress;
            deal.HireRequest.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return (true, "Proposal sent.", new ProposalDto
        {
            Id = proposal.Id,
            DealId = proposal.DealId,
            Title = proposal.Title,
            Description = proposal.Description,
            Amount = proposal.Amount,
            IsNegotiable = proposal.IsNegotiable,
            Status = proposal.Status.ToString(),
            CreatedAt = proposal.CreatedAt,
            UpdatedAt = proposal.UpdatedAt
        });
    }

    public async Task<(bool Success, string Message)> AcceptProposalAsync(int clientUserId, int proposalId, string? note)
    {
        var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return (false, "Client profile not found.");

        var proposal = await _db.Proposals.Include(p => p.Deal).ThenInclude(d => d.HireRequest).FirstOrDefaultAsync(p => p.Id == proposalId);
        if (proposal == null) return (false, "Proposal not found.");
        if (proposal.Deal.ClientProfileId != clientProfile.Id) return (false, "Access denied.");
        if (proposal.Status != ProposalStatus.Pending) return (false, "Proposal is no longer pending.");

        proposal.Status = ProposalStatus.Accepted;
        proposal.ClientNote = note;
        proposal.UpdatedAt = DateTime.UtcNow;

        // Reject other pending proposals for the same deal
        var otherPending = await _db.Proposals
            .Where(p => p.DealId == proposal.DealId && p.Id != proposalId && p.Status == ProposalStatus.Pending)
            .ToListAsync();
        foreach (var p in otherPending)
        {
            p.Status = ProposalStatus.Rejected;
            p.UpdatedAt = DateTime.UtcNow;
        }

        proposal.Deal.Status = DealStatus.ProposalAccepted;
        proposal.Deal.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, "Proposal accepted.");
    }

    public async Task<(bool Success, string Message)> RejectProposalAsync(int clientUserId, int proposalId, string? note)
    {
        var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return (false, "Client profile not found.");

        var proposal = await _db.Proposals.Include(p => p.Deal).FirstOrDefaultAsync(p => p.Id == proposalId);
        if (proposal == null) return (false, "Proposal not found.");
        if (proposal.Deal.ClientProfileId != clientProfile.Id) return (false, "Access denied.");
        if (proposal.Status != ProposalStatus.Pending) return (false, "Proposal is no longer pending.");

        proposal.Status = ProposalStatus.Rejected;
        proposal.ClientNote = note;
        proposal.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, "Proposal rejected.");
    }

    // ── Invoices (on Deal) ───────────────────────────────────────────

    public async Task<(bool Success, string Message, InvoiceDto? Data)> GenerateInvoiceAsync(int lawyerUserId, int proposalId, CreateInvoiceDto dto)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return (false, "Lawyer profile not found.", null);

        var proposal = await _db.Proposals.Include(p => p.Deal).Include(p => p.Invoice).FirstOrDefaultAsync(p => p.Id == proposalId);
        if (proposal == null) return (false, "Proposal not found.", null);
        if (proposal.Deal.LawyerProfileId != lawyerProfile.Id) return (false, "Access denied.", null);
        if (proposal.Status != ProposalStatus.Accepted) return (false, "Only accepted proposals can generate invoices.", null);
        if (proposal.Invoice != null) return (false, "Invoice already generated for this proposal.", null);

        // Generate invoice number
        var year = DateTime.UtcNow.Year;
        var count = await _db.Invoices.CountAsync(i => i.CreatedAt.Year == year);
        var invoiceNumber = $"INV-{year}-{(count + 1):D4}";

        var invoice = new Invoice
        {
            ProposalId = proposalId,
            DealId = proposal.DealId,
            InvoiceNumber = invoiceNumber,
            Amount = proposal.Amount,
            Description = dto.Description,
            DueDate = dto.DueDate,
            Status = InvoiceStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Invoices.Add(invoice);

        proposal.Deal.Status = DealStatus.InvoiceGenerated;
        proposal.Deal.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return (true, "Invoice generated.", new InvoiceDto
        {
            Id = invoice.Id,
            ProposalId = invoice.ProposalId,
            DealId = invoice.DealId,
            InvoiceNumber = invoice.InvoiceNumber,
            Amount = invoice.Amount,
            Description = invoice.Description,
            DueDate = invoice.DueDate,
            Status = invoice.Status.ToString(),
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt
        });
    }

    public async Task<(bool Success, string Message)> AcceptInvoiceAsync(int clientUserId, int invoiceId)
    {
        var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return (false, "Client profile not found.");

        var invoice = await _db.Invoices.Include(i => i.Deal).ThenInclude(d => d.HireRequest).FirstOrDefaultAsync(i => i.Id == invoiceId);
        if (invoice == null) return (false, "Invoice not found.");
        if (invoice.Deal.ClientProfileId != clientProfile.Id) return (false, "Access denied.");
        if (invoice.Status != InvoiceStatus.Pending) return (false, "Invoice is not pending.");

        invoice.Status = InvoiceStatus.Accepted;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await ConvertDealToCaseAsync(invoice.DealId);
        return (true, "Invoice accepted.");
    }

    public async Task<(bool Success, string Message)> MarkInvoicePaidAsync(int lawyerUserId, int invoiceId)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return (false, "Lawyer profile not found.");

        var invoice = await _db.Invoices.Include(i => i.Deal).ThenInclude(d => d.HireRequest).FirstOrDefaultAsync(i => i.Id == invoiceId);
        if (invoice == null) return (false, "Invoice not found.");
        if (invoice.Deal.LawyerProfileId != lawyerProfile.Id) return (false, "Access denied.");
        if (invoice.Status != InvoiceStatus.Accepted && invoice.Status != InvoiceStatus.Pending)
            return (false, "Invoice cannot be marked as paid in its current state.");

        invoice.Status = InvoiceStatus.Paid;
        invoice.UpdatedAt = DateTime.UtcNow;

        invoice.Deal.Status = DealStatus.Paid;
        invoice.Deal.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await ConvertDealToCaseAsync(invoice.DealId);
        return (true, "Invoice marked as paid.");
    }

    public async Task<(bool Success, string Message)> RejectInvoiceAsync(int clientUserId, int invoiceId)
    {
        var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return (false, "Client profile not found.");

        var invoice = await _db.Invoices.Include(i => i.Deal).FirstOrDefaultAsync(i => i.Id == invoiceId);
        if (invoice == null) return (false, "Invoice not found.");
        if (invoice.Deal.ClientProfileId != clientProfile.Id) return (false, "Access denied.");
        if (invoice.Status != InvoiceStatus.Pending) return (false, "Invoice is not pending.");

        invoice.Status = InvoiceStatus.Rejected;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, "Invoice rejected.");
    }

    // ── Private Helpers ──────────────────────────────────────────────

    private async Task<bool> HasAccessAsync(int userId, string role, HireRequest hr)
    {
        if (role == "Client")
        {
            var cp = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            return cp != null && hr.ClientProfileId == cp.Id;
        }
        if (role == "Lawyer")
        {
            var lp = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            return lp != null && hr.LawyerProfileId == lp.Id;
        }
        return role == "Admin";
    }

    /// <summary>
    /// When an invoice is accepted or paid, auto-create a Case from the HireRequest info,
    /// add the lawyer to the case, and create the LawyerClient relationship.
    /// </summary>
    private async Task ConvertDealToCaseAsync(int dealId)
    {
        var deal = await _db.Deals.Include(d => d.HireRequest).FirstOrDefaultAsync(d => d.Id == dealId);
        if (deal == null || deal.HireRequest.Status == HireRequestStatus.ConvertedToCase) return;

        var hr = deal.HireRequest;

        // Auto-create Case from HireRequest details
        var newCase = new Case
        {
            ClientProfileId = hr.ClientProfileId,
            LawyerProfileId = hr.LawyerProfileId,
            CaseTitle = $"{hr.CaseType} - Deal #{deal.Id}",
            CaseType = hr.CaseType,
            Court = hr.Court,
            Description = hr.Description,
            Status = CaseStatus.Open,
            DealId = deal.Id,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };
        _db.Cases.Add(newCase);
        await _db.SaveChangesAsync();

        // Add lawyer as CaseLawyer
        _db.CaseLawyers.Add(new CaseLawyer
        {
            CaseId = newCase.Id,
            LawyerProfileId = hr.LawyerProfileId,
            AddedByRole = "System",
            AddedAt = DateTime.UtcNow,
            IsActive = true
        });

        // Create LawyerClient relationship if not exists
        var existingLc = await _db.LawyerClients.IgnoreQueryFilters()
            .FirstOrDefaultAsync(lc => lc.LawyerProfileId == hr.LawyerProfileId && lc.ClientProfileId == hr.ClientProfileId);
        if (existingLc == null)
        {
            _db.LawyerClients.Add(new LawyerClient
            {
                LawyerProfileId = hr.LawyerProfileId,
                ClientProfileId = hr.ClientProfileId,
                IsActive = true,
                AddedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            });
        }
        else if (existingLc.IsDeleted || !existingLc.IsActive)
        {
            existingLc.IsDeleted = false;
            existingLc.IsActive = true;
            existingLc.ModifiedAt = DateTime.UtcNow;
        }

        // Update statuses
        hr.Status = HireRequestStatus.ConvertedToCase;
        hr.UpdatedAt = DateTime.UtcNow;
        deal.Status = DealStatus.Completed;
        deal.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    private static HireRequestDto MapToDto(HireRequest hr, ClientProfile client, LawyerProfile lawyer, int msgCount, int unreadCount) => new()
    {
        Id = hr.Id,
        ClientProfileId = hr.ClientProfileId,
        ClientFullName = client.User.FirstName + " " + client.User.LastName,
        ClientPicture = client.User.ProfilePictureUrl,
        LawyerProfileId = hr.LawyerProfileId,
        LawyerFullName = lawyer.User.FirstName + " " + lawyer.User.LastName,
        LawyerPicture = lawyer.User.ProfilePictureUrl,
        LawyerCategoryName = lawyer.Category?.Name,
        Status = hr.Status.ToString(),
        Description = hr.Description,
        CaseType = hr.CaseType,
        Court = hr.Court,
        Message = hr.Message,
        MessageCount = msgCount,
        UnreadCount = unreadCount,
        CreatedAt = hr.CreatedAt,
        UpdatedAt = hr.UpdatedAt
    };
}
