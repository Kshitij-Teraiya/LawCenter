namespace LegalConnect.API.Entities;

public class LegalContract
{
    public int Id { get; set; }

    /// <summary>"RegistrationTnC" | "ProposalDraft" | "ProposalFinal" | "RefundInvoice"</summary>
    public string ContractType { get; set; } = string.Empty;

    public int? LawyerProfileId { get; set; }
    public int? ProposalId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;   // relative path in Uploads/Contracts/
    public long FileSize { get; set; }

    public bool IsAccepted { get; set; } = false;
    public DateTime? AcceptedAt { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    // Navigation
    public LawyerProfile? LawyerProfile { get; set; }
    public Proposal? Proposal { get; set; }
}

public static class ContractType
{
    public const string RegistrationTnC  = "RegistrationTnC";
    public const string ProposalDraft    = "ProposalDraft";
    public const string ProposalFinal    = "ProposalFinal";
    public const string RefundInvoice    = "RefundInvoice";
}
