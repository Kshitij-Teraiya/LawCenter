namespace LegalConnect.API.Entities;

public enum CaseStatus
{
    Open,
    InProgress,
    Closed,
    OnHold
}

public class Case
{
    public int Id { get; set; }

    /// <summary>Nullable: a client may open a case without assigning a lawyer yet.</summary>
    public int? LawyerProfileId { get; set; }
    public int ClientProfileId { get; set; }
    public int? LawyerClientId { get; set; }
    public int? AppointmentId { get; set; }

    public string CaseTitle { get; set; } = string.Empty;
    public string? CaseNumber { get; set; }
    public string CaseType { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;

    public DateTime? FilingDate { get; set; }
    public DateTime? NextHearingDate { get; set; }

    public CaseStatus Status { get; set; } = CaseStatus.Open;
    public string Description { get; set; } = string.Empty;
    public string? Outcome { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public LawyerProfile? LawyerProfile { get; set; }      // nullable because LawyerProfileId is now optional
    public ClientProfile ClientProfile { get; set; } = null!;
    public LawyerClient? LawyerClient { get; set; }
    public Appointment? Appointment { get; set; }
    public List<CaseActivity> Activities { get; set; } = [];
    public List<CaseDocument> Documents { get; set; } = [];
    public List<CaseLawyer> CaseLawyers { get; set; } = [];  // multiple lawyers per case
    public int? DealId { get; set; }
    public Deal? Deal { get; set; }
}
