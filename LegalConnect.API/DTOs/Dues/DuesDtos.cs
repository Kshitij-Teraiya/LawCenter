using System.ComponentModel.DataAnnotations;

namespace LegalConnect.API.DTOs.Dues;

// ── Dues Entries ─────────────────────────────────────────────────────────────

public class DuesEntryDto
{
    public int      Id               { get; set; }
    public string   EntryType        { get; set; } = string.Empty;
    public decimal  Amount           { get; set; }
    public string   Description      { get; set; } = string.Empty;
    public DateTime CreatedAt        { get; set; }
    public int?     InvoiceId        { get; set; }
    public int?     LitigationDisputeId { get; set; }
    public int?     RefundInvoiceId  { get; set; }
}

public class DuesSummaryDto
{
    public decimal          TotalDues { get; set; }
    public List<DuesEntryDto> Entries { get; set; } = [];
}

public class LawyerDuesSummaryDto
{
    public int              LawyerProfileId { get; set; }
    public string           LawyerName      { get; set; } = string.Empty;
    public decimal          TotalDues       { get; set; }
    public List<DuesEntryDto> Entries       { get; set; } = [];
}

// ── Litigation Disputes ──────────────────────────────────────────────────────

public class LitigationDisputeDto
{
    public int      Id              { get; set; }
    public int      InvoiceId       { get; set; }
    public string   InvoiceNumber   { get; set; } = string.Empty;
    public string   DisputeType     { get; set; } = string.Empty;
    public decimal  DisputedAmount  { get; set; }
    public string   Reason          { get; set; } = string.Empty;
    public string   Status          { get; set; } = string.Empty;
    public bool     AdminApproved   { get; set; }
    public DateTime? AdminApprovedAt { get; set; }
    public bool     LawyerApproved  { get; set; }
    public DateTime? LawyerApprovedAt { get; set; }
    public DateTime CreatedAt       { get; set; }
    public string   LawyerName      { get; set; } = string.Empty;
    public string   ClientName      { get; set; } = string.Empty;
}

public class RaiseDisputeDto
{
    [Required] public int     InvoiceId       { get; set; }
    [Required] public string  DisputeType     { get; set; } = string.Empty;
    [Required] public decimal DisputedAmount  { get; set; }
    [Required] public string  Reason          { get; set; } = string.Empty;
}

public class ApproveDisputeDto
{
    [Required] public bool    Approve { get; set; }
    public string?            Notes   { get; set; }
}

public class DisputeFilterDto
{
    public string? Status   { get; set; }
    public int     Page     { get; set; } = 1;
    public int     PageSize { get; set; } = 20;
}

// ── Refund Invoices ──────────────────────────────────────────────────────────

public class RefundInvoiceDto
{
    public int      Id                   { get; set; }
    public string   RefundInvoiceNumber  { get; set; } = string.Empty;
    public string   LawyerName           { get; set; } = string.Empty;
    public decimal  Amount               { get; set; }
    public string   Reason               { get; set; } = string.Empty;
    public string   Status               { get; set; } = string.Empty;
    public DateTime GeneratedAt          { get; set; }
    public int?     ContractId           { get; set; }
    public int?     DuesEntryId          { get; set; }
}

public class CreateRefundInvoiceDto
{
    [Required] public int     LawyerProfileId { get; set; }
    [Required] public decimal Amount          { get; set; }
    [Required] public string  Reason          { get; set; } = string.Empty;
}

// ── System Settings ──────────────────────────────────────────────────────────

public class SystemSettingDto
{
    public string   Key         { get; set; } = string.Empty;
    public string   Value       { get; set; } = string.Empty;
    public string?  Description { get; set; }
    public DateTime UpdatedAt   { get; set; }
}

public class UpdateSystemSettingDto
{
    [Required] public string Value { get; set; } = string.Empty;
}
