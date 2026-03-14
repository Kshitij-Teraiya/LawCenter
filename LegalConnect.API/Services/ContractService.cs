using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Contracts;
using LegalConnect.API.Entities;
using LegalConnect.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface IContractService
{
    Task<LegalContract> GenerateAndSaveRegistrationTnCAsync(int lawyerProfileId, string lawyerName, string email, string tncText);
    Task<LegalContract> GenerateAndSaveProposalDraftAsync(int proposalId);
    Task<LegalContract> GenerateAndSaveFinalProposalAsync(int proposalId);
    Task<LegalContract> GenerateAndSaveRefundInvoicePdfAsync(RefundInvoice refund, LawyerProfile lawyer, string adminName);
    Task AcceptContractAsync(int contractId);
    Task<PagedResult<LegalContractDto>> GetContractsAsync(int userId, string role, ContractFilterDto filter);
    Task<LegalContract?> GetContractByIdAsync(int id, int userId, string role);
    Task<(FileStream? Stream, string? FileName)> GetContractStreamAsync(int id, int userId, string role);
}

public class ContractService : IContractService
{
    private readonly AppDbContext _db;
    private readonly IContractFileService _fileService;
    private readonly IPdfGenerationService _pdfService;

    public ContractService(
        AppDbContext db,
        IContractFileService fileService,
        IPdfGenerationService pdfService)
    {
        _db          = db;
        _fileService = fileService;
        _pdfService  = pdfService;
    }

    // ── Registration T&C ─────────────────────────────────────────────────────

    public async Task<LegalContract> GenerateAndSaveRegistrationTnCAsync(
        int lawyerProfileId, string lawyerName, string email, string tncText)
    {
        var acceptedAt = DateTime.UtcNow;
        var pdf        = _pdfService.GenerateRegistrationTnC(lawyerName, email, tncText, acceptedAt);
        var fileName   = $"tnc_{lawyerProfileId}_{Guid.NewGuid():N}.pdf";
        var path       = await _fileService.SaveContractAsync(pdf, "registration", fileName);

        var contract = new LegalContract
        {
            ContractType    = ContractType.RegistrationTnC,
            LawyerProfileId = lawyerProfileId,
            Title           = $"Registration T&C — {lawyerName}",
            FileName        = fileName,
            FilePath        = path,
            FileSize        = pdf.LongLength,
            IsAccepted      = true,
            AcceptedAt      = acceptedAt,
            GeneratedAt     = acceptedAt
        };

        _db.LegalContracts.Add(contract);
        await _db.SaveChangesAsync();
        return contract;
    }

    // ── Proposal Draft ───────────────────────────────────────────────────────

    public async Task<LegalContract> GenerateAndSaveProposalDraftAsync(int proposalId)
    {
        var data = await BuildProposalContractData(proposalId, isFinal: false);
        var pdf  = _pdfService.GenerateProposalContract(data);
        var fileName = $"proposal_draft_{proposalId}_{Guid.NewGuid():N}.pdf";
        var path = await _fileService.SaveContractAsync(pdf, "proposals", fileName);

        var contract = new LegalContract
        {
            ContractType = ContractType.ProposalDraft,
            ProposalId   = proposalId,
            Title        = $"Proposal Draft — {data.ProposalTitle}",
            FileName     = fileName,
            FilePath     = path,
            FileSize     = pdf.LongLength,
            GeneratedAt  = DateTime.UtcNow
        };

        _db.LegalContracts.Add(contract);
        await _db.SaveChangesAsync();
        return contract;
    }

    // ── Proposal Final ───────────────────────────────────────────────────────

    public async Task<LegalContract> GenerateAndSaveFinalProposalAsync(int proposalId)
    {
        var data = await BuildProposalContractData(proposalId, isFinal: true);
        var pdf  = _pdfService.GenerateProposalContract(data);
        var fileName = $"proposal_final_{proposalId}_{Guid.NewGuid():N}.pdf";
        var path = await _fileService.SaveContractAsync(pdf, "proposals", fileName);

        var contract = new LegalContract
        {
            ContractType = ContractType.ProposalFinal,
            ProposalId   = proposalId,
            Title        = $"Proposal Contract — {data.ProposalTitle}",
            FileName     = fileName,
            FilePath     = path,
            FileSize     = pdf.LongLength,
            IsAccepted   = true,
            AcceptedAt   = DateTime.UtcNow,
            GeneratedAt  = DateTime.UtcNow
        };

        _db.LegalContracts.Add(contract);
        await _db.SaveChangesAsync();
        return contract;
    }

    // ── Refund Invoice PDF ───────────────────────────────────────────────────

    public async Task<LegalContract> GenerateAndSaveRefundInvoicePdfAsync(
        RefundInvoice refund, LawyerProfile lawyer, string adminName)
    {
        var lawyerUser = lawyer.User;
        var data = new RefundInvoiceData
        {
            RefundInvoiceNumber = refund.RefundInvoiceNumber,
            Amount              = refund.Amount,
            Reason              = refund.Reason,
            GeneratedAt         = refund.GeneratedAt,
            LawyerName          = $"{lawyerUser.FirstName} {lawyerUser.LastName}",
            LawyerEmail         = lawyerUser.Email ?? string.Empty,
            FirmName            = lawyer.InvoiceSettings?.FirmName,
            GeneratedByName     = adminName
        };

        var pdf      = _pdfService.GenerateRefundInvoicePdf(data);
        var fileName = $"refund_{refund.RefundInvoiceNumber}_{Guid.NewGuid():N}.pdf";
        var path     = await _fileService.SaveContractAsync(pdf, "refund-invoices", fileName);

        var contract = new LegalContract
        {
            ContractType    = ContractType.RefundInvoice,
            LawyerProfileId = lawyer.Id,
            Title           = $"Refund Invoice — {refund.RefundInvoiceNumber}",
            FileName        = fileName,
            FilePath        = path,
            FileSize        = pdf.LongLength,
            GeneratedAt     = DateTime.UtcNow
        };

        _db.LegalContracts.Add(contract);
        await _db.SaveChangesAsync();
        return contract;
    }

    // ── Accept ────────────────────────────────────────────────────────────────

    public async Task AcceptContractAsync(int contractId)
    {
        var contract = await _db.LegalContracts.FindAsync(contractId);
        if (contract == null) return;
        contract.IsAccepted = true;
        contract.AcceptedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── List/Get ─────────────────────────────────────────────────────────────

    public async Task<PagedResult<LegalContractDto>> GetContractsAsync(
        int userId, string role, ContractFilterDto filter)
    {
        var query = _db.LegalContracts
            .Include(c => c.LawyerProfile).ThenInclude(l => l!.User)
            .Include(c => c.Proposal)
            .AsQueryable();

        if (role == "Lawyer")
        {
            var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
            if (lawyerProfile != null)
                query = query.Where(c => c.LawyerProfileId == lawyerProfile.Id
                                      || (c.ProposalId != null && c.Proposal!.Deal.LawyerProfileId == lawyerProfile.Id));
            else
                query = query.Where(c => false);
        }

        if (!string.IsNullOrWhiteSpace(filter.ContractType))
            query = query.Where(c => c.ContractType == filter.ContractType);

        if (!string.IsNullOrWhiteSpace(filter.LawyerName))
        {
            var name = filter.LawyerName.ToLower();
            query = query.Where(c =>
                c.LawyerProfile != null &&
                (c.LawyerProfile.User.FirstName + " " + c.LawyerProfile.User.LastName).ToLower().Contains(name));
        }

        if (filter.DateFrom.HasValue)
            query = query.Where(c => c.GeneratedAt >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue)
            query = query.Where(c => c.GeneratedAt <= filter.DateTo.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.GeneratedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => new LegalContractDto
            {
                Id            = c.Id,
                ContractType  = c.ContractType,
                Title         = c.Title,
                FileName      = c.FileName,
                FileSize      = c.FileSize,
                GeneratedAt   = c.GeneratedAt,
                IsAccepted    = c.IsAccepted,
                AcceptedAt    = c.AcceptedAt,
                Notes         = c.Notes,
                LawyerName    = c.LawyerProfile != null
                    ? c.LawyerProfile.User.FirstName + " " + c.LawyerProfile.User.LastName
                    : null,
                ProposalTitle = c.Proposal != null ? c.Proposal.Title : null
            })
            .ToListAsync();

        return new PagedResult<LegalContractDto>
        {
            Items      = items,
            TotalCount = totalCount,
            PageNumber = filter.Page,
            PageSize   = filter.PageSize
        };
    }

    public async Task<LegalContract?> GetContractByIdAsync(int id, int userId, string role)
    {
        var contract = await _db.LegalContracts
            .Include(c => c.LawyerProfile).ThenInclude(l => l!.User)
            .Include(c => c.Proposal).ThenInclude(p => p!.Deal)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null) return null;
        if (role == "Admin") return contract;

        // Lawyer: must be the lawyer's own contract
        var lawyerProfile = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == userId);
        if (lawyerProfile == null) return null;

        var isOwner = contract.LawyerProfileId == lawyerProfile.Id
            || (contract.Proposal != null && contract.Proposal.Deal.LawyerProfileId == lawyerProfile.Id);
        return isOwner ? contract : null;
    }

    public async Task<(FileStream? Stream, string? FileName)> GetContractStreamAsync(
        int id, int userId, string role)
    {
        var contract = await GetContractByIdAsync(id, userId, role);
        if (contract == null) return (null, null);

        var stream = _fileService.GetContractStream(contract.FilePath);
        return (stream, contract.FileName);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<ProposalContractData> BuildProposalContractData(int proposalId, bool isFinal)
    {
        var proposal = await _db.Proposals
            .Include(p => p.Deal)
                .ThenInclude(d => d.LawyerProfile)
                    .ThenInclude(l => l.User)
            .Include(p => p.Deal)
                .ThenInclude(d => d.LawyerProfile)
                    .ThenInclude(l => l.InvoiceSettings)
            .Include(p => p.Deal)
                .ThenInclude(d => d.ClientProfile)
                    .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.Id == proposalId)
            ?? throw new InvalidOperationException($"Proposal {proposalId} not found.");

        var lawyer     = proposal.Deal.LawyerProfile;
        var lawyerUser = lawyer.User;
        var clientUser = proposal.Deal.ClientProfile.User;

        // Build absolute path for sign image
        string? signImageAbsPath = null;
        var signRelPath = lawyer.InvoiceSettings?.AuthorizedSignImagePath;
        if (!string.IsNullOrWhiteSpace(signRelPath))
        {
            var abs = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", signRelPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(abs)) signImageAbsPath = abs;
        }

        return new ProposalContractData
        {
            ProposalId    = proposal.Id,
            ProposalTitle = proposal.Title,
            Description   = proposal.Description,
            Amount        = proposal.Amount,
            IsNegotiable  = proposal.IsNegotiable,
            CreatedAt     = proposal.CreatedAt,
            IsFinal       = isFinal,
            LawyerName    = $"{lawyerUser.FirstName} {lawyerUser.LastName}",
            LawyerEmail   = lawyerUser.Email ?? string.Empty,
            LawyerCity    = lawyer.City,
            FirmName      = lawyer.InvoiceSettings?.FirmName,
            SignImagePath = signImageAbsPath,
            ClientName    = $"{clientUser.FirstName} {clientUser.LastName}",
            ClientEmail   = clientUser.Email ?? string.Empty
        };
    }
}
