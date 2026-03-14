using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Dues;
using LegalConnect.API.Entities;
using LegalConnect.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface IRefundInvoiceService
{
    Task<(bool Success, string Message, RefundInvoiceDto? Data)> CreateAsync(int adminUserId, string adminName, CreateRefundInvoiceDto dto);
    Task<PagedResult<RefundInvoiceDto>> GetAllAsync(int page, int pageSize);
    Task<PagedResult<RefundInvoiceDto>> GetForLawyerAsync(int lawyerUserId, int page, int pageSize);
    Task<RefundInvoiceDto?> GetByIdAsync(int id);
    Task<(FileStream? Stream, string? FileName)> DownloadAsync(int id);
}

public class RefundInvoiceService : IRefundInvoiceService
{
    private readonly AppDbContext          _db;
    private readonly IDuesService          _duesService;
    private readonly IContractService      _contractService;

    public RefundInvoiceService(
        AppDbContext db,
        IDuesService duesService,
        IContractService contractService)
    {
        _db              = db;
        _duesService     = duesService;
        _contractService = contractService;
    }

    public async Task<(bool Success, string Message, RefundInvoiceDto? Data)> CreateAsync(
        int adminUserId, string adminName, CreateRefundInvoiceDto dto)
    {
        var lawyer = await _db.LawyerProfiles
            .Include(l => l.User)
            .Include(l => l.InvoiceSettings)
            .FirstOrDefaultAsync(l => l.Id == dto.LawyerProfileId);

        if (lawyer == null) return (false, "Lawyer not found.", null);
        if (dto.Amount <= 0) return (false, "Amount must be positive.", null);

        var year   = DateTime.UtcNow.Year;
        var count  = await _db.RefundInvoices.CountAsync(r => r.GeneratedAt.Year == year);
        var number = $"RINV-{year}-{(count + 1):D4}";

        var refund = new RefundInvoice
        {
            LawyerProfileId   = dto.LawyerProfileId,
            RefundInvoiceNumber = number,
            Amount            = dto.Amount,
            Reason            = dto.Reason,
            Status            = RefundInvoiceStatus.Issued,
            GeneratedAt       = DateTime.UtcNow,
            GeneratedByUserId = adminUserId
        };
        _db.RefundInvoices.Add(refund);
        await _db.SaveChangesAsync();

        // Create credit dues entry (negative amount = platform pays lawyer)
        var duesEntry = await _duesService.AddEntryAsync(new CreateDuesEntryInternal
        {
            LawyerProfileId = dto.LawyerProfileId,
            EntryType       = DuesEntryType.RefundCredit,
            Amount          = -dto.Amount,   // negative = credit
            Description     = $"Refund Invoice {number}: {dto.Reason}",
            CreatedByUserId = adminUserId,
            RefundInvoiceId = refund.Id
        });

        refund.DuesEntryId = duesEntry.Id;
        refund.Status      = RefundInvoiceStatus.Processed;
        await _db.SaveChangesAsync();

        // Generate refund invoice PDF
        try
        {
            var contract = await _contractService.GenerateAndSaveRefundInvoicePdfAsync(refund, lawyer, adminName);
            refund.ContractId = contract.Id;
            await _db.SaveChangesAsync();
        }
        catch { }

        return (true, $"Refund invoice {number} created successfully.", ToDto(refund, lawyer));
    }

    public async Task<PagedResult<RefundInvoiceDto>> GetAllAsync(int page, int pageSize)
    {
        var query = _db.RefundInvoices
            .Include(r => r.LawyerProfile).ThenInclude(l => l.User)
            .OrderByDescending(r => r.GeneratedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<RefundInvoiceDto>
        {
            Items      = items.Select(r => ToDto(r, r.LawyerProfile)).ToList(),
            TotalCount = total,
            PageNumber = page,
            PageSize   = pageSize
        };
    }

    public async Task<PagedResult<RefundInvoiceDto>> GetForLawyerAsync(int lawyerUserId, int page, int pageSize)
    {
        var lp = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lp == null) return new PagedResult<RefundInvoiceDto> { PageNumber = page, PageSize = pageSize };

        var query = _db.RefundInvoices
            .Include(r => r.LawyerProfile).ThenInclude(l => l.User)
            .Where(r => r.LawyerProfileId == lp.Id)
            .OrderByDescending(r => r.GeneratedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<RefundInvoiceDto>
        {
            Items      = items.Select(r => ToDto(r, r.LawyerProfile)).ToList(),
            TotalCount = total,
            PageNumber = page,
            PageSize   = pageSize
        };
    }

    public async Task<RefundInvoiceDto?> GetByIdAsync(int id)
    {
        var r = await _db.RefundInvoices
            .Include(r => r.LawyerProfile).ThenInclude(l => l.User)
            .FirstOrDefaultAsync(r => r.Id == id);
        return r == null ? null : ToDto(r, r.LawyerProfile);
    }

    public async Task<(FileStream? Stream, string? FileName)> DownloadAsync(int id)
    {
        var r = await _db.RefundInvoices.FirstOrDefaultAsync(x => x.Id == id);
        if (r?.ContractId == null) return (null, null);

        var contract = await _db.LegalContracts.FindAsync(r.ContractId.Value);
        if (contract == null) return (null, null);

        var contractService = _contractService as ContractService;
        // Use base ContractService to get the file stream
        var (stream, fileName) = await _contractService.GetContractStreamAsync(contract.Id, 0, "Admin");
        return (stream, fileName);
    }

    private static RefundInvoiceDto ToDto(RefundInvoice r, LawyerProfile? lp) => new()
    {
        Id                  = r.Id,
        RefundInvoiceNumber = r.RefundInvoiceNumber,
        LawyerName          = lp != null ? $"{lp.User.FirstName} {lp.User.LastName}" : string.Empty,
        Amount              = r.Amount,
        Reason              = r.Reason,
        Status              = r.Status,
        GeneratedAt         = r.GeneratedAt,
        ContractId          = r.ContractId,
        DuesEntryId         = r.DuesEntryId
    };
}
