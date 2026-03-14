namespace LegalConnect.Client.Models.Dues;

// ── Dues ─────────────────────────────────────────────────────────────────────

public class DuesEntryModel
{
    public int      Id                  { get; set; }
    public string   EntryType           { get; set; } = string.Empty;
    public decimal  Amount              { get; set; }
    public string   Description         { get; set; } = string.Empty;
    public DateTime CreatedAt           { get; set; }
    public int?     InvoiceId           { get; set; }
    public int?     LitigationDisputeId { get; set; }
    public int?     RefundInvoiceId     { get; set; }
    public bool     IsCredit            => Amount < 0;
}

public class DuesSummaryModel
{
    public decimal              TotalDues { get; set; }
    public List<DuesEntryModel> Entries   { get; set; } = [];
}

public class LawyerDuesSummaryModel
{
    public int                  LawyerProfileId { get; set; }
    public string               LawyerName      { get; set; } = string.Empty;
    public decimal              TotalDues       { get; set; }
    public List<DuesEntryModel> Entries         { get; set; } = [];
}

// ── Litigation Disputes ──────────────────────────────────────────────────────

public class LitigationDisputeModel
{
    public int      Id               { get; set; }
    public int      InvoiceId        { get; set; }
    public string   InvoiceNumber    { get; set; } = string.Empty;
    public string   DisputeType      { get; set; } = string.Empty;
    public decimal  DisputedAmount   { get; set; }
    public string   Reason           { get; set; } = string.Empty;
    public string   Status           { get; set; } = string.Empty;
    public bool     AdminApproved    { get; set; }
    public DateTime? AdminApprovedAt  { get; set; }
    public bool     LawyerApproved   { get; set; }
    public DateTime? LawyerApprovedAt { get; set; }
    public DateTime CreatedAt        { get; set; }
    public string   LawyerName       { get; set; } = string.Empty;
    public string   ClientName       { get; set; } = string.Empty;
}

public class RaiseDisputeModel
{
    public int     InvoiceId      { get; set; }
    public string  DisputeType    { get; set; } = "UnderLitigation";
    public decimal DisputedAmount { get; set; }
    public string  Reason         { get; set; } = string.Empty;
}

public class ApproveDisputeModel
{
    public bool   Approve { get; set; }
    public string? Notes  { get; set; }
}

// ── Refund Invoices ──────────────────────────────────────────────────────────

public class RefundInvoiceModel
{
    public int      Id                  { get; set; }
    public string   RefundInvoiceNumber { get; set; } = string.Empty;
    public string   LawyerName          { get; set; } = string.Empty;
    public decimal  Amount              { get; set; }
    public string   Reason              { get; set; } = string.Empty;
    public string   Status              { get; set; } = string.Empty;
    public DateTime GeneratedAt         { get; set; }
    public int?     ContractId          { get; set; }
    public int?     DuesEntryId         { get; set; }
}

public class CreateRefundInvoiceModel
{
    public int     LawyerProfileId { get; set; }
    public decimal Amount          { get; set; }
    public string  Reason          { get; set; } = string.Empty;
}

// ── System Settings ──────────────────────────────────────────────────────────

public class SystemSettingModel
{
    public string   Key         { get; set; } = string.Empty;
    public string   Value       { get; set; } = string.Empty;
    public string?  Description { get; set; }
    public DateTime UpdatedAt   { get; set; }
}
