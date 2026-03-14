namespace LegalConnect.API.Entities;

public class RefundInvoice
{
    public int Id { get; set; }

    public int LawyerProfileId { get; set; }

    public string RefundInvoiceNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;

    /// <summary>"Issued" | "Processed"</summary>
    public string Status { get; set; } = RefundInvoiceStatus.Issued;

    public int? DuesEntryId { get; set; }
    public int? ContractId { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public int GeneratedByUserId { get; set; }

    // Navigation
    public LawyerProfile LawyerProfile { get; set; } = null!;
    public DuesEntry? DuesEntry { get; set; }
    public LegalContract? Contract { get; set; }
}

public static class RefundInvoiceStatus
{
    public const string Issued    = "Issued";
    public const string Processed = "Processed";
}
