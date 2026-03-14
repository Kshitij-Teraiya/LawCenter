using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Dues;
using LegalConnect.API.Entities;
using LegalConnect.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

// Internal model used between services
public class CreateDuesEntryInternal
{
    public int      LawyerProfileId      { get; set; }
    public string   EntryType            { get; set; } = DuesEntryType.DisputeDebit;
    public decimal  Amount               { get; set; }
    public string   Description          { get; set; } = string.Empty;
    public int      CreatedByUserId      { get; set; }
    public int?     InvoiceId            { get; set; }
    public int?     LitigationDisputeId  { get; set; }
    public int?     RefundInvoiceId      { get; set; }
}

public interface IDuesService
{
    Task<DuesEntry> AddEntryAsync(CreateDuesEntryInternal model);
    Task<DuesSummaryDto> GetMyDuesAsync(int lawyerUserId);
    Task<LawyerDuesSummaryDto?> GetLawyerDuesAsync(int lawyerProfileId);
    Task<PagedResult<LawyerDuesSummaryDto>> GetAllLawyerDuesAsync(int page, int pageSize);
}

public class DuesService : IDuesService
{
    private readonly AppDbContext _db;

    public DuesService(AppDbContext db) => _db = db;

    public async Task<DuesEntry> AddEntryAsync(CreateDuesEntryInternal model)
    {
        var entry = new DuesEntry
        {
            LawyerProfileId     = model.LawyerProfileId,
            EntryType           = model.EntryType,
            Amount              = model.Amount,
            Description         = model.Description,
            CreatedByUserId     = model.CreatedByUserId,
            CreatedAt           = DateTime.UtcNow,
            InvoiceId           = model.InvoiceId,
            LitigationDisputeId = model.LitigationDisputeId,
            RefundInvoiceId     = model.RefundInvoiceId
        };
        _db.DuesEntries.Add(entry);
        await _db.SaveChangesAsync();
        return entry;
    }

    public async Task<DuesSummaryDto> GetMyDuesAsync(int lawyerUserId)
    {
        var lp = await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);
        if (lp == null) return new DuesSummaryDto();
        return await BuildSummary(lp.Id);
    }

    public async Task<LawyerDuesSummaryDto?> GetLawyerDuesAsync(int lawyerProfileId)
    {
        var lp = await _db.LawyerProfiles.Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == lawyerProfileId);
        if (lp == null) return null;

        var summary = await BuildSummary(lawyerProfileId);
        return new LawyerDuesSummaryDto
        {
            LawyerProfileId = lp.Id,
            LawyerName      = $"{lp.User.FirstName} {lp.User.LastName}",
            TotalDues       = summary.TotalDues,
            Entries         = summary.Entries
        };
    }

    public async Task<PagedResult<LawyerDuesSummaryDto>> GetAllLawyerDuesAsync(int page, int pageSize)
    {
        var lawyers = await _db.LawyerProfiles.Include(l => l.User).ToListAsync();
        var all     = new List<LawyerDuesSummaryDto>();

        foreach (var lp in lawyers)
        {
            var total = await _db.DuesEntries
                .Where(e => e.LawyerProfileId == lp.Id)
                .SumAsync(e => (decimal?)e.Amount) ?? 0m;

            if (total != 0)  // Only show lawyers who have dues/credits
            {
                all.Add(new LawyerDuesSummaryDto
                {
                    LawyerProfileId = lp.Id,
                    LawyerName      = $"{lp.User.FirstName} {lp.User.LastName}",
                    TotalDues       = total
                });
            }
        }

        var ordered   = all.OrderByDescending(x => x.TotalDues).ToList();
        var paginated = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<LawyerDuesSummaryDto>
        {
            Items      = paginated,
            TotalCount = ordered.Count,
            PageNumber = page,
            PageSize   = pageSize
        };
    }

    private async Task<DuesSummaryDto> BuildSummary(int lawyerProfileId)
    {
        var entries = await _db.DuesEntries
            .Where(e => e.LawyerProfileId == lawyerProfileId)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new DuesEntryDto
            {
                Id                  = e.Id,
                EntryType           = e.EntryType,
                Amount              = e.Amount,
                Description         = e.Description,
                CreatedAt           = e.CreatedAt,
                InvoiceId           = e.InvoiceId,
                LitigationDisputeId = e.LitigationDisputeId,
                RefundInvoiceId     = e.RefundInvoiceId
            })
            .ToListAsync();

        var total = entries.Sum(e => e.Amount);

        return new DuesSummaryDto { TotalDues = total, Entries = entries };
    }
}
