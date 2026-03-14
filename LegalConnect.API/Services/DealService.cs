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
    Task<List<ClientInvoiceSummaryDto>> GetMyInvoicesAsync(int clientUserId);
    Task<List<HireRequestDto>> GetIncomingHireRequestsAsync(int lawyerUserId);
    Task<HireRequestDetailDto?> GetHireRequestByIdAsync(int userId, string role, int hireRequestId);
    Task<(bool Success, string Message)> RejectHireRequestAsync(int lawyerUserId, int hireRequestId);
    Task<(bool Success, string Message)> CancelHireRequestAsync(int clientUserId, int hireRequestId);
    Task<(bool Success, string Message)> CancelHireRequestByPartyAsync(int userId, string role, int hireRequestId);

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
    Task<(bool Success, string Message, InvoiceDto? Data)> GenerateInvoiceAsync(int lawyerUserId, int dealId, CreateInvoiceDto dto);
    Task<(bool Success, string Message)> AcceptInvoiceAsync(int clientUserId, int invoiceId);
    Task<(bool Success, string Message)> MarkInvoicePaidAsync(int lawyerUserId, int invoiceId);
    Task<(bool Success, string Message)> RejectInvoiceAsync(int clientUserId, int invoiceId);

    // Invoice Settings
    Task<LawyerInvoiceSettingsDto?> GetInvoiceSettingsAsync(int lawyerUserId);
    Task<(bool Success, string Message, LawyerInvoiceSettingsDto? Data)> UpsertInvoiceSettingsAsync(int lawyerUserId, UpsertLawyerInvoiceSettingsDto dto);
    Task<List<LawyerPaidInvoiceDto>> GetLawyerPaidInvoicesAsync(int lawyerUserId);
}

public class DealService : IDealService
{
    private readonly AppDbContext _db;
    private readonly IContractService _contractService;

    public DealService(AppDbContext db, IContractService contractService)
    {
        _db              = db;
        _contractService = contractService;
    }

    // ── HireRequest ──────────────────────────────────────────────────

    public async Task<(bool Success, string Message, HireRequestDto? Data)> CreateHireRequestAsync(int clientUserId, CreateHireRequestDto dto)
    {
        var clientProfile = await _db.ClientProfiles.Include(c => c.User).FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return (false, "Client profile not found.", null);

        var lawyerProfile = await _db.LawyerProfiles.Include(l => l.User).Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == dto.LawyerProfileId && l.IsVerified);
        if (lawyerProfile == null) return (false, "Lawyer not found or not verified.", null);

        var existing = await _db.HireRequests.IgnoreQueryFilters()
            .FirstOrDefaultAsync(h => h.LawyerProfileId == dto.LawyerProfileId
                && h.ClientProfileId == clientProfile.Id && !h.IsDeleted
                && h.Status != HireRequestStatus.Rejected
                && h.Status != HireRequestStatus.ConvertedToCase);
        if (existing != null) return (false, "You already have an active hire request with this lawyer.", null);

        // Validate the linked case if provided
        if (dto.LinkedCaseId.HasValue)
        {
            var linkedCase = await _db.Cases.FirstOrDefaultAsync(c =>
                c.Id == dto.LinkedCaseId.Value && c.ClientProfileId == clientProfile.Id);
            if (linkedCase == null)
                return (false, "Linked case not found or does not belong to you.", null);
            if (linkedCase.Status == CaseStatus.Closed)
                return (false, "Cannot link a hire request to a closed case.", null);
        }

        var hireRequest = new HireRequest
        {
            ClientProfileId = clientProfile.Id,
            LawyerProfileId = dto.LawyerProfileId,
            Status = HireRequestStatus.Inquiry,
            Description = dto.Description,
            CaseType = dto.CaseType,
            Court = dto.Court,
            Message = dto.Message,
            LinkedCaseId = dto.LinkedCaseId,
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
            .Include(h => h.LinkedCase)
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
                LinkedCaseId = h.LinkedCaseId,
                LinkedCaseTitle = h.LinkedCase != null ? h.LinkedCase.CaseTitle : null,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt
            }).ToListAsync();
    }

    public async Task<List<ClientInvoiceSummaryDto>> GetMyInvoicesAsync(int clientUserId)
    {
        var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
        if (clientProfile == null) return [];

        return await _db.Invoices
            .Where(i => i.Deal.ClientProfileId == clientProfile.Id)
            .Include(i => i.Deal).ThenInclude(d => d.LawyerProfile).ThenInclude(l => l.User)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new ClientInvoiceSummaryDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                LawyerName = i.Deal.LawyerProfile.User.FirstName + " " + i.Deal.LawyerProfile.User.LastName,
                TotalAmount = i.TotalAmount,
                Status = i.Status.ToString()
            })
            .ToListAsync();
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
            .Include(h => h.LinkedCase)
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
                LinkedCaseId = h.LinkedCaseId,
                LinkedCaseTitle = h.LinkedCase != null ? h.LinkedCase.CaseTitle : null,
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
            .Include(h => h.Documents)
            .Include(h => h.LinkedCase)
            .Include(h => h.Deal).ThenInclude(d => d!.Proposals).ThenInclude(p => p.Invoices)
            .Include(h => h.Deal).ThenInclude(d => d!.Invoices)
            .FirstOrDefaultAsync();

        if (hr == null) return null;
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
            LinkedCaseId = hr.LinkedCaseId,
            LinkedCaseTitle = hr.LinkedCase?.CaseTitle,
            CreatedAt = hr.CreatedAt,
            UpdatedAt = hr.UpdatedAt,
            Documents = hr.Documents.OrderByDescending(d => d.UploadedAt).Select(d => new HireRequestDocumentDto
            {
                Id            = d.Id,
                HireRequestId = d.HireRequestId,
                FileName      = d.FileName,
                FileSize      = d.FileSize,
                ContentType   = d.ContentType,
                UploadedAt    = d.UploadedAt,
                SourceType    = "HireRequest"
            }).ToList(),
            LinkedCasePreview = hr.LinkedCase == null ? null : new LinkedCasePreviewDto
            {
                Id          = hr.LinkedCase.Id,
                CaseTitle   = hr.LinkedCase.CaseTitle,
                CaseType    = hr.LinkedCase.CaseType,
                Court       = hr.LinkedCase.Court,
                Description = hr.LinkedCase.Description,
                Status      = hr.LinkedCase.Status.ToString(),
                CreatedAt   = hr.LinkedCase.CreatedDate
            }
        };

        // Auto-attach deal-flagged case documents from the linked case
        if (hr.LinkedCaseId.HasValue)
        {
            var caseDocs = await _db.CaseDocuments
                .Where(d => d.CaseId == hr.LinkedCaseId.Value
                         && d.IsAvailableForDeal
                         && !d.IsDeleted
                         && d.UploadedByRole == "Client"
                         && !d.IsPrivate)
                .OrderByDescending(d => d.UploadedDate)
                .Select(d => new HireRequestDocumentDto
                {
                    Id            = d.Id,          // CaseDocument.Id used for download routing
                    HireRequestId = hr.Id,
                    FileName      = d.FileName,
                    FileSize      = d.FileSize,
                    ContentType   = d.ContentType,
                    UploadedAt    = d.UploadedDate,
                    SourceType    = "Case"
                })
                .ToListAsync();
            result.Documents.AddRange(caseDocs);
        }

        if (hr.Deal != null)
        {
            var acceptedProposal = hr.Deal.Proposals.FirstOrDefault(p => p.Status == ProposalStatus.Accepted);
            var acceptedAmount = acceptedProposal?.Amount ?? 0m;
            var activeInvoices = hr.Deal.Invoices.Where(i => i.Status != InvoiceStatus.Rejected).ToList();
            var totalInvoiced = activeInvoices.Sum(i => i.Amount);

            result.Deal = new DealDto
            {
                Id = hr.Deal.Id,
                HireRequestId = hr.Deal.HireRequestId,
                Status = hr.Deal.Status.ToString(),
                CreatedAt = hr.Deal.CreatedAt,
                UpdatedAt = hr.Deal.UpdatedAt,
                AcceptedProposalAmount = acceptedAmount,
                TotalInvoicedAmount = totalInvoiced,
                RemainingAmount = acceptedAmount - totalInvoiced,
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
                    Invoices = p.Invoices.OrderByDescending(i => i.CreatedAt).Select(MapInvoiceDto).ToList()
                }).ToList(),
                Invoices = hr.Deal.Invoices.OrderByDescending(i => i.CreatedAt).Select(MapInvoiceDto).ToList()
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

    public async Task<(bool Success, string Message)> CancelHireRequestByPartyAsync(int userId, string role, int hireRequestId)
    {
        HireRequest? hr;

        if (role == "Client")
        {
            var clientProfile = await _db.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (clientProfile == null) return (false, "Client profile not found.");
            hr = await _db.HireRequests.FirstOrDefaultAsync(h => h.Id == hireRequestId && h.ClientProfileId == clientProfile.Id);
        }
        else // Lawyer
        {
            var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            if (lawyerProfile == null) return (false, "Lawyer profile not found.");
            hr = await _db.HireRequests.FirstOrDefaultAsync(h => h.Id == hireRequestId && h.LawyerProfileId == lawyerProfile.Id);
        }

        if (hr == null) return (false, "Hire request not found or access denied.");
        if (hr.Status == HireRequestStatus.ConvertedToCase)
            return (false, "Cannot cancel a request that has already been converted to a case.");

        hr.IsDeleted = true;
        hr.Status    = HireRequestStatus.Rejected;   // mark as rejected so it disappears cleanly
        hr.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, "Hire request cancelled successfully.");
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

        if (deal.HireRequest.Status == HireRequestStatus.AcceptedByLawyer)
        {
            deal.HireRequest.Status = HireRequestStatus.DealInProgress;
            deal.HireRequest.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        // Generate draft contract PDF (non-fatal)
        try { await _contractService.GenerateAndSaveProposalDraftAsync(proposal.Id); } catch { }

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

        // Generate final signed contract PDF (non-fatal)
        try { await _contractService.GenerateAndSaveFinalProposalAsync(proposalId); } catch { }

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

    /// <summary>
    /// Create a partial/milestone invoice. Multiple invoices allowed per deal;
    /// sum of non-rejected invoices must not exceed the accepted proposal amount.
    /// </summary>
    public async Task<(bool Success, string Message, InvoiceDto? Data)> GenerateInvoiceAsync(
        int lawyerUserId, int dealId, CreateInvoiceDto dto)
    {
        var lawyerProfile = await _db.LawyerProfiles
            .Include(l => l.User)
            .Include(l => l.InvoiceSettings)
            .FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return (false, "Lawyer profile not found.", null);

        var deal = await _db.Deals
            .Include(d => d.HireRequest).ThenInclude(h => h.ClientProfile).ThenInclude(c => c.User)
            .Include(d => d.Proposals)
            .Include(d => d.Invoices)
            .FirstOrDefaultAsync(d => d.Id == dealId && d.LawyerProfileId == lawyerProfile.Id);

        if (deal == null) return (false, "Deal not found.", null);

        var acceptedProposal = deal.Proposals.FirstOrDefault(p => p.Status == ProposalStatus.Accepted);
        if (acceptedProposal == null)
            return (false, "No accepted proposal found. Client must accept a proposal before invoicing.", null);

        var existingTotal = deal.Invoices
            .Where(i => i.Status != InvoiceStatus.Rejected)
            .Sum(i => i.Amount);

        if (existingTotal + dto.Amount > acceptedProposal.Amount)
        {
            var remaining = acceptedProposal.Amount - existingTotal;
            return (false, $"Invoice amount exceeds the remaining balance. Remaining: \u20b9{remaining:N2} (Proposal total: \u20b9{acceptedProposal.Amount:N2}).", null);
        }

        var gstRate = dto.GstRate ?? 0m;
        var gstAmount = Math.Round(dto.Amount * gstRate / 100m, 2);
        var totalAmount = dto.Amount + gstAmount;

        var year = DateTime.UtcNow.Year;
        var count = await _db.Invoices.CountAsync(i => i.CreatedAt.Year == year);
        var invoiceNumber = $"INV-{year}-{(count + 1):D4}";

        var invoice = new Invoice
        {
            ProposalId = acceptedProposal.Id,
            DealId = dealId,
            InvoiceNumber = invoiceNumber,
            ChargeType = dto.ChargeType,
            Amount = dto.Amount,
            GstRate = gstRate > 0 ? gstRate : null,
            GstAmount = gstAmount,
            TotalAmount = totalAmount,
            Description = dto.Description,
            DueDate = dto.DueDate,
            Status = InvoiceStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Invoices.Add(invoice);

        if (deal.Status == DealStatus.ProposalAccepted)
        {
            deal.Status = DealStatus.InvoiceGenerated;
            deal.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        var s = lawyerProfile.InvoiceSettings;
        var client = deal.HireRequest.ClientProfile;

        return (true, "Invoice created.", new InvoiceDto
        {
            Id = invoice.Id,
            ProposalId = invoice.ProposalId,
            DealId = invoice.DealId,
            InvoiceNumber = invoice.InvoiceNumber,
            ChargeType = invoice.ChargeType,
            Amount = invoice.Amount,
            GstRate = invoice.GstRate,
            GstAmount = invoice.GstAmount,
            TotalAmount = invoice.TotalAmount,
            Description = invoice.Description,
            DueDate = invoice.DueDate,
            Status = invoice.Status.ToString(),
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt,
            LawyerFirmName = s?.FirmName,
            LawyerFullName = lawyerProfile.User.FirstName + " " + lawyerProfile.User.LastName,
            ClientFullName = client.User.FirstName + " " + client.User.LastName,
            LawyerGSTNumber = s?.GSTNumber,
            LawyerAddress = s != null
                ? string.Join(", ", new[] { s.FirmAddress, s.City, s.State, s.Country, s.PostalCode }
                    .Where(x => !string.IsNullOrWhiteSpace(x)))
                : null,
            LawyerPhone = s?.Phone,
            LawyerEmail = s?.Email,
            LawyerFirmLogoPath = s?.FirmLogoPath,
            LawyerBankDetails = s?.BankDetails,
            LawyerNotesForInvoice = s?.NotesForInvoice,
            LawyerTermsAndConditions = s?.TermsAndConditions,
            LawyerAuthorizedSignImagePath = s?.AuthorizedSignImagePath
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

    public async Task<List<LawyerPaidInvoiceDto>> GetLawyerPaidInvoicesAsync(int lawyerUserId)
    {
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return [];

        return await _db.Invoices
            .Include(i => i.Deal).ThenInclude(d => d.ClientProfile).ThenInclude(cp => cp.User)
            .Where(i => i.Deal.LawyerProfileId == lawyerProfile.Id && i.Status == InvoiceStatus.Paid)
            .OrderByDescending(i => i.UpdatedAt)
            .Select(i => new LawyerPaidInvoiceDto
            {
                Id            = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                ClientName    = i.Deal.ClientProfile.User.FirstName + " " + i.Deal.ClientProfile.User.LastName,
                ChargeType    = i.ChargeType ?? "Other",
                Amount        = i.Amount,
                GstAmount     = i.GstAmount,
                TotalAmount   = i.TotalAmount,
                PaidAt        = i.UpdatedAt
            })
            .ToListAsync();
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

    // ── Invoice Settings ────────────────────────────────────────────

    public async Task<LawyerInvoiceSettingsDto?> GetInvoiceSettingsAsync(int lawyerUserId)
    {
        var lawyerProfile = await _db.LawyerProfiles
            .Include(l => l.InvoiceSettings)
            .FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return null;
        if (lawyerProfile.InvoiceSettings == null)
            return new LawyerInvoiceSettingsDto { LawyerProfileId = lawyerProfile.Id };

        return MapSettingsDto(lawyerProfile.InvoiceSettings);
    }

    public async Task<(bool Success, string Message, LawyerInvoiceSettingsDto? Data)> UpsertInvoiceSettingsAsync(
        int lawyerUserId, UpsertLawyerInvoiceSettingsDto dto)
    {
        var lawyerProfile = await _db.LawyerProfiles
            .Include(l => l.InvoiceSettings)
            .FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lawyerProfile == null) return (false, "Lawyer profile not found.", null);

        var settings = lawyerProfile.InvoiceSettings;
        if (settings == null)
        {
            settings = new LawyerInvoiceSettings
            {
                LawyerProfileId = lawyerProfile.Id,
                CreatedAt = DateTime.UtcNow
            };
            _db.LawyerInvoiceSettings.Add(settings);
        }

        settings.FirmName = dto.FirmName;
        settings.FirmAddress = dto.FirmAddress;
        settings.City = dto.City;
        settings.State = dto.State;
        settings.Country = dto.Country;
        settings.PostalCode = dto.PostalCode;
        settings.GSTNumber = dto.GSTNumber;
        settings.Phone = dto.Phone;
        settings.Email = dto.Email;
        settings.Website = dto.Website;
        settings.BankDetails = dto.BankDetails;
        settings.NotesForInvoice = dto.NotesForInvoice;
        settings.TermsAndConditions = dto.TermsAndConditions;
        settings.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, "Invoice settings saved.", MapSettingsDto(settings));
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

    private static InvoiceDto MapInvoiceDto(Invoice i) => new()
    {
        Id = i.Id,
        ProposalId = i.ProposalId,
        DealId = i.DealId,
        InvoiceNumber = i.InvoiceNumber,
        ChargeType = i.ChargeType,
        Amount = i.Amount,
        GstRate = i.GstRate,
        GstAmount = i.GstAmount,
        TotalAmount = i.TotalAmount,
        Description = i.Description,
        DueDate = i.DueDate,
        Status = i.Status.ToString(),
        CreatedAt = i.CreatedAt,
        UpdatedAt = i.UpdatedAt
    };

    private static LawyerInvoiceSettingsDto MapSettingsDto(LawyerInvoiceSettings s) => new()
    {
        Id = s.Id,
        LawyerProfileId = s.LawyerProfileId,
        FirmName = s.FirmName,
        FirmLogoPath = s.FirmLogoPath,
        FirmAddress = s.FirmAddress,
        City = s.City,
        State = s.State,
        Country = s.Country,
        PostalCode = s.PostalCode,
        GSTNumber = s.GSTNumber,
        Phone = s.Phone,
        Email = s.Email,
        Website = s.Website,
        AuthorizedSignImagePath = s.AuthorizedSignImagePath,
        BankDetails = s.BankDetails,
        NotesForInvoice = s.NotesForInvoice,
        TermsAndConditions = s.TermsAndConditions
    };

    private async Task ConvertDealToCaseAsync(int dealId)
    {
        var deal = await _db.Deals.Include(d => d.HireRequest).FirstOrDefaultAsync(d => d.Id == dealId);
        if (deal == null || deal.HireRequest.Status == HireRequestStatus.ConvertedToCase) return;

        var hr = deal.HireRequest;
        bool linkedToExisting = false;

        // ── Try to link to an existing case ──────────────────────────────
        if (hr.LinkedCaseId.HasValue)
        {
            var existingCase = await _db.Cases.FirstOrDefaultAsync(c => c.Id == hr.LinkedCaseId.Value);
            if (existingCase != null)
            {
                linkedToExisting = true;

                // Add lawyer only if not already active on the case
                var alreadyOnCase = await _db.CaseLawyers.AnyAsync(cl =>
                    cl.CaseId == existingCase.Id && cl.LawyerProfileId == hr.LawyerProfileId && cl.IsActive);
                if (!alreadyOnCase)
                {
                    _db.CaseLawyers.Add(new CaseLawyer
                    {
                        CaseId = existingCase.Id,
                        LawyerProfileId = hr.LawyerProfileId,
                        AddedByRole = "System",
                        AddedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                }
            }
        }

        // ── Fallback: create a new case ───────────────────────────────────
        if (!linkedToExisting)
        {
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

            _db.CaseLawyers.Add(new CaseLawyer
            {
                CaseId = newCase.Id,
                LawyerProfileId = hr.LawyerProfileId,
                AddedByRole = "System",
                AddedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        // Ensure LawyerClient relationship exists
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
        LinkedCaseId = hr.LinkedCaseId,
        LinkedCaseTitle = hr.LinkedCase?.CaseTitle,
        CreatedAt = hr.CreatedAt,
        UpdatedAt = hr.UpdatedAt
    };
}
