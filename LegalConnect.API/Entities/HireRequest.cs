namespace LegalConnect.API.Entities;

public enum HireRequestStatus
{
    Inquiry = 0,
    AcceptedByLawyer = 1,
    Rejected = 2,
    DealInProgress = 3,
    ConvertedToCase = 4
}

public class HireRequest
{
    public int Id { get; set; }
    public int ClientProfileId { get; set; }
    public int LawyerProfileId { get; set; }
    public HireRequestStatus Status { get; set; } = HireRequestStatus.Inquiry;

    // Inquiry details
    public string Description { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public string? Message { get; set; }

    /// <summary>
    /// If set, the deal will be linked to an existing case instead of auto-creating a new one.
    /// </summary>
    public int? LinkedCaseId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    // Navigation
    public ClientProfile ClientProfile { get; set; } = null!;
    public LawyerProfile LawyerProfile { get; set; } = null!;
    public List<HireRequestMessage>  Messages  { get; set; } = [];
    public List<HireRequestDocument> Documents { get; set; } = [];
    public Deal? Deal { get; set; }
    public Case? LinkedCase { get; set; }
}
