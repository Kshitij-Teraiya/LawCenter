namespace LegalConnect.API.Entities;

public class LitigationDispute
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }
    public int ClientUserId { get; set; }

    /// <summary>"Unserviced" | "UnderLitigation"</summary>
    public string DisputeType { get; set; } = DisputeTypeValues.UnderLitigation;

    public decimal DisputedAmount { get; set; }
    public string Reason { get; set; } = string.Empty;

    /// <summary>"Pending" | "AdminApproved" | "LawyerApproved" | "BothApproved" | "Rejected"</summary>
    public string Status { get; set; } = DisputeStatus.Pending;

    public bool AdminApproved { get; set; } = false;
    public DateTime? AdminApprovedAt { get; set; }
    public int? AdminUserId { get; set; }

    public bool LawyerApproved { get; set; } = false;
    public DateTime? LawyerApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Invoice Invoice { get; set; } = null!;
}

public static class DisputeTypeValues
{
    public const string Unserviced       = "Unserviced";
    public const string UnderLitigation  = "UnderLitigation";
}

public static class DisputeStatus
{
    public const string Pending       = "Pending";
    public const string AdminApproved = "AdminApproved";
    public const string LawyerApproved = "LawyerApproved";
    public const string BothApproved  = "BothApproved";
    public const string Rejected      = "Rejected";
}
