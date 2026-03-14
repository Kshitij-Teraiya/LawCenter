namespace LegalConnect.API.Entities;

public enum ActivityType
{
    Hearing,
    DocumentUpload,
    Note,
    StatusChange,
    DocumentDownload,
    CaseCreated,
    CaseClosed
}

/// <summary>Business-level categories for manually logged activities.</summary>
public static class ActivityCategory
{
    public const string Clerical        = "Clerical";
    public const string PoliceInquiry   = "Police Inquiry";
    public const string FIR             = "FIR";
    public const string MainEvent       = "Main Event";
    public const string CaseSubmission  = "Case Submission";
    public const string CourtHearing    = "Court Hearing";
    public const string JudgementDay    = "Judgement Day";
    public const string SummonsAttended = "Summons Attended";
    public const string Payment         = "Payment";
    public const string Other           = "Other";

    public static readonly string[] All =
    [
        Clerical, PoliceInquiry, FIR, MainEvent, CaseSubmission,
        CourtHearing, JudgementDay, SummonsAttended, Payment, Other
    ];
}

public class CaseActivity
{
    public int Id { get; set; }
    public int CaseId { get; set; }
    public int CreatedByUserId { get; set; }

    public ActivityType ActivityType { get; set; }

    /// <summary>Short headline for the activity entry.</summary>
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>System timestamp — when this record was logged.</summary>
    public DateTime ActivityDate { get; set; } = DateTime.UtcNow;

    /// <summary>User-supplied event date (e.g. the actual hearing date).</summary>
    public DateTime? EventDate { get; set; }

    /// <summary>Business category — one of ActivityCategory constants.</summary>
    public string Category { get; set; } = ActivityCategory.Other;

    public string CreatedByRole { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;

    // Navigation
    public Case Case { get; set; } = null!;
    public ApplicationUser CreatedBy { get; set; } = null!;
    public List<CaseActivityDocument> LinkedDocuments { get; set; } = [];
}

/// <summary>Junction: one activity linked to zero-or-more case documents.</summary>
public class CaseActivityDocument
{
    public int CaseActivityId { get; set; }
    public int CaseDocumentId { get; set; }

    public CaseActivity CaseActivity { get; set; } = null!;
    public CaseDocument CaseDocument { get; set; } = null!;
}
