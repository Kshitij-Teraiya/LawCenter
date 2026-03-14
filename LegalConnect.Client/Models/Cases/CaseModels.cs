using System.ComponentModel.DataAnnotations;

namespace LegalConnect.Client.Models.Cases;

// ── Enums ────────────────────────────────────────────────────────────────────

public enum CaseStatus { Open, InProgress, Closed, OnHold }
public enum ActivityType
{
    Hearing, DocumentUpload, Note, StatusChange,
    DocumentDownload, CaseCreated, CaseClosed
}
public enum ClientLawyerRequestStatus { Pending = 0, Accepted = 1, Rejected = 2 }

// ── LawyerClient ─────────────────────────────────────────────────────────────

public class LawyerClientDto
{
    public int Id { get; set; }
    public int LawyerProfileId { get; set; }
    public int ClientProfileId { get; set; }
    public int? FirstAppointmentId { get; set; }
    public string ClientFullName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string? ClientPhone { get; set; }
    public string? ClientCity { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime AddedDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public int TotalCases { get; set; }
    public int OpenCases { get; set; }
    public DateTime? LastActivityDate { get; set; }
}

public class AddLawyerClientDto
{
    [Required] public int ClientProfileId { get; set; }
    public int? FirstAppointmentId { get; set; }
    [MaxLength(2000)] public string? Notes { get; set; }
}

public class UpdateLawyerClientNotesDto
{
    [MaxLength(2000)] public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class EligibleClientDto
{
    public int AppointmentId { get; set; }
    public int ClientProfileId { get; set; }
    public string ClientFullName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string? ClientPhone { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime AppointmentDate { get; set; }
    public bool AlreadyAdded { get; set; }
}

public class LawyerClientFilterDto
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public string SortBy { get; set; } = "addedDate";
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    public string ToQueryString()
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(SearchTerm)) parts.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
        if (IsActive.HasValue) parts.Add($"isActive={IsActive.Value}");
        parts.Add($"sortBy={SortBy}");
        parts.Add($"sortDescending={SortDescending}");
        parts.Add($"pageNumber={PageNumber}");
        parts.Add($"pageSize={PageSize}");
        return string.Join("&", parts);
    }
}

// ── Case ─────────────────────────────────────────────────────────────────────

public class CaseSummaryDto
{
    public int Id { get; set; }
    public string CaseTitle { get; set; } = string.Empty;
    public string? CaseNumber { get; set; }
    public string CaseType { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public CaseStatus Status { get; set; }
    public DateTime? FilingDate { get; set; }
    public DateTime? NextHearingDate { get; set; }
    public string LawyerName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int DocumentCount { get; set; }
    public int ActivityCount { get; set; }
    public int LawyerCount { get; set; }

    public string StatusBadgeClass => Status switch
    {
        CaseStatus.Open       => "badge bg-primary",
        CaseStatus.InProgress => "badge bg-warning text-dark",
        CaseStatus.Closed     => "badge bg-success",
        CaseStatus.OnHold     => "badge bg-secondary",
        _                     => "badge bg-light"
    };
}

public class CaseDto : CaseSummaryDto
{
    public int? LawyerProfileId { get; set; }
    public int ClientProfileId { get; set; }
    public int? AppointmentId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Outcome { get; set; }
    public string? LawyerProfilePicture { get; set; }
    public string? ClientProfilePicture { get; set; }
    /// <summary>Deal that auto-created this case (null for manually-created cases).</summary>
    public int? DealId { get; set; }
    /// <summary>HireRequest.Id for the linked deal — used for navigation to /lawyer/deals/{id} or /client/deals/{id}.</summary>
    public int? DealHireRequestId { get; set; }
    public List<CaseLawyerDto> AssignedLawyers { get; set; } = [];
    public List<CaseActivityDto> RecentActivities { get; set; } = [];
    public List<CaseDocumentDto> Documents { get; set; } = [];
}

public class CaseLawyerDto
{
    public int LawyerProfileId { get; set; }
    public string LawyerFullName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? City { get; set; }
    public string? CategoryName { get; set; }
    public DateTime AddedAt { get; set; }
    public string AddedByRole { get; set; } = string.Empty;
}

public class CreateCaseDto
{
    [Required, MaxLength(300)] public string CaseTitle { get; set; } = string.Empty;
    [MaxLength(100)] public string? CaseNumber { get; set; }
    [Required, MaxLength(100)] public string CaseType { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string Court { get; set; } = string.Empty;
    /// <summary>Used when a Lawyer creates the case.</summary>
    public int? ClientProfileId { get; set; }
    /// <summary>Lawyer IDs to assign — optional (0 or more) when a client creates the case.</summary>
    public List<int> LawyerProfileIds { get; set; } = [];
    public int? AppointmentId { get; set; }
    public int? LawyerClientId { get; set; }
    public DateTime? FilingDate { get; set; }
    public DateTime? NextHearingDate { get; set; }
    [Required, MaxLength(3000)] public string Description { get; set; } = string.Empty;
}

public class HiredLawyerDto
{
    public int LawyerProfileId { get; set; }
    public int LawyerClientId { get; set; }
    public string LawyerFullName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? City { get; set; }
    public string? Court { get; set; }
    public string? CategoryName { get; set; }
}

public class UpdateCaseDto
{
    [Required, MaxLength(300)] public string CaseTitle { get; set; } = string.Empty;
    [MaxLength(100)] public string? CaseNumber { get; set; }
    [Required, MaxLength(100)] public string CaseType { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string Court { get; set; } = string.Empty;
    public DateTime? FilingDate { get; set; }
    public DateTime? NextHearingDate { get; set; }
    public CaseStatus Status { get; set; }
    [Required, MaxLength(3000)] public string Description { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Outcome { get; set; }
}

public class CloseCaseDto
{
    [Required, MaxLength(2000)] public string Outcome { get; set; } = string.Empty;
}

public class CaseFilterDto
{
    public string? SearchTerm { get; set; }
    public CaseStatus? Status { get; set; }
    public string? CaseType { get; set; }
    public string? Court { get; set; }
    public int? ClientProfileId { get; set; }
    public string SortBy { get; set; } = "modifiedDate";
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string ToQueryString()
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(SearchTerm)) parts.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
        if (Status.HasValue) parts.Add($"status={Status.Value}");
        if (!string.IsNullOrEmpty(CaseType)) parts.Add($"caseType={Uri.EscapeDataString(CaseType)}");
        if (!string.IsNullOrEmpty(Court)) parts.Add($"court={Uri.EscapeDataString(Court)}");
        if (ClientProfileId.HasValue) parts.Add($"clientProfileId={ClientProfileId.Value}");
        parts.Add($"sortBy={SortBy}");
        parts.Add($"sortDescending={SortDescending}");
        parts.Add($"pageNumber={PageNumber}");
        parts.Add($"pageSize={PageSize}");
        return string.Join("&", parts);
    }
}

// ── CaseActivity ─────────────────────────────────────────────────────────────

public class LinkedDocumentDto
{
    public int DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}

public class CaseActivityDto
{
    public int Id { get; set; }
    public int CaseId { get; set; }
    public ActivityType ActivityType { get; set; }
    public string ActivityTypeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "Other";
    public DateTime ActivityDate { get; set; }
    public DateTime? EventDate { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string CreatedByRole { get; set; } = string.Empty;
    public List<LinkedDocumentDto> LinkedDocuments { get; set; } = [];
    public string ActivityIcon => ActivityType switch
    {
        ActivityType.Hearing          => "bi bi-calendar-event text-primary",
        ActivityType.DocumentUpload   => "bi bi-file-earmark-arrow-up text-success",
        ActivityType.DocumentDownload => "bi bi-file-earmark-arrow-down text-info",
        ActivityType.Note             => "bi bi-sticky text-warning",
        ActivityType.StatusChange     => "bi bi-arrow-repeat text-secondary",
        ActivityType.CaseCreated      => "bi bi-folder-plus text-primary",
        ActivityType.CaseClosed       => "bi bi-folder-x text-danger",
        _                             => "bi bi-circle"
    };
}

public class AddCaseActivityDto
{
    [Required] public ActivityType ActivityType { get; set; }
    [MaxLength(200)] public string Title { get; set; } = string.Empty;
    [Required, MaxLength(1000)] public string Description { get; set; } = string.Empty;
    public DateTime? ActivityDate { get; set; }
    public DateTime? EventDate { get; set; }
    [MaxLength(100)] public string Category { get; set; } = "Other";
    public List<int> LinkedDocumentIds { get; set; } = [];
}

// ── CaseDocument ─────────────────────────────────────────────────────────────

public class CaseDocumentDto
{
    public int Id { get; set; }
    public int CaseId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileSizeDisplay => FileSize < 1024 * 1024
        ? $"{FileSize / 1024.0:F1} KB"
        : $"{FileSize / (1024.0 * 1024):F1} MB";
    public string ContentType { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public bool SharedWithAllLawyers { get; set; } = true;
    public List<int> SharedWithLawyerIds { get; set; } = [];
    public bool IsAvailableForDeal { get; set; }
    public DateTime UploadedDate { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public string UploadedByRole { get; set; } = string.Empty;
    public string FileIcon => ContentType switch
    {
        "application/pdf" => "bi bi-file-earmark-pdf text-danger",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => "bi bi-file-earmark-word text-primary",
        _ when ContentType.StartsWith("image/") => "bi bi-file-earmark-image text-success",
        _ => "bi bi-file-earmark"
    };
}

// ── ClientLawyerRequest ───────────────────────────────────────────────────────

public class ClientLawyerRequestDto
{
    public int Id { get; set; }
    public int ClientProfileId { get; set; }
    public int LawyerProfileId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ClientFullName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string? ClientPictureUrl { get; set; }
    public string LawyerFullName { get; set; } = string.Empty;
    public string? LawyerPictureUrl { get; set; }
    public string? LawyerCity { get; set; }
    public string? LawyerCategoryName { get; set; }
    public string? Message { get; set; }
    public string? LawyerNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string StatusBadgeClass => Status switch
    {
        "Pending"  => "badge bg-warning text-dark",
        "Accepted" => "badge bg-success",
        "Rejected" => "badge bg-danger",
        _          => "badge bg-secondary"
    };
}

public class CreateClientLawyerRequestDto
{
    [Required] public int LawyerProfileId { get; set; }
    [MaxLength(1000)] public string? Message { get; set; }
}

public class RespondToRequestDto
{
    [Required] public bool Accept { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
}
