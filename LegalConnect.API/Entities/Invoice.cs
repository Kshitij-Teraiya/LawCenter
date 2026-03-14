namespace LegalConnect.API.Entities;

public enum InvoiceStatus
{
    Pending = 0,
    Accepted = 1,
    Paid = 2,
    Rejected = 3
}

public static class InvoiceChargeType
{
    public const string AdvancePayment          = "Advance Payment";
    public const string HiringCharges           = "Hiring Charges";
    public const string CourtFilingFees         = "Court Filing Fees";
    public const string HearingCharges          = "Hearing Charges";
    public const string DocumentationCharges    = "Documentation Charges";
    public const string FinalSettlement         = "Final Settlement / Judgement Fee";
    public const string Other                   = "Other";

    public static readonly string[] All =
    [
        AdvancePayment, HiringCharges, CourtFilingFees,
        HearingCharges, DocumentationCharges, FinalSettlement, Other
    ];
}

public class Invoice
{
    public int Id { get; set; }
    public int? ProposalId { get; set; }   // nullable – multiple invoices per proposal/deal
    public int DealId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? ChargeType { get; set; }
    public decimal Amount { get; set; }
    public decimal? GstRate { get; set; }  // percentage e.g. 18.0
    public decimal GstAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Proposal? Proposal { get; set; }
    public Deal Deal { get; set; } = null!;
}
