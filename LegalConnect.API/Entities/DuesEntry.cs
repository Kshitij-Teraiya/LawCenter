namespace LegalConnect.API.Entities;

public class DuesEntry
{
    public int Id { get; set; }

    public int LawyerProfileId { get; set; }

    /// <summary>"DisputeDebit" | "RefundCredit" | "AdminAdjustment"</summary>
    public string EntryType { get; set; } = DuesEntryType.DisputeDebit;

    /// <summary>Positive = lawyer owes platform; Negative = platform refunds lawyer</summary>
    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }

    public int? InvoiceId { get; set; }
    public int? LitigationDisputeId { get; set; }
    public int? RefundInvoiceId { get; set; }

    // Navigation
    public LawyerProfile LawyerProfile { get; set; } = null!;
}

public static class DuesEntryType
{
    public const string DisputeDebit     = "DisputeDebit";
    public const string RefundCredit     = "RefundCredit";
    public const string AdminAdjustment  = "AdminAdjustment";
}
