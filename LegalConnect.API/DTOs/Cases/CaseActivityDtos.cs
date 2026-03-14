using System.ComponentModel.DataAnnotations;
using LegalConnect.API.Entities;

namespace LegalConnect.API.DTOs.Cases;

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
    public string ActivityTypeName => ActivityType.ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = ActivityCategory.Other;
    public DateTime ActivityDate { get; set; }
    public DateTime? EventDate { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string CreatedByRole { get; set; } = string.Empty;
    public List<LinkedDocumentDto> LinkedDocuments { get; set; } = [];
    public string ActivityIcon => ActivityType switch
    {
        ActivityType.Hearing          => "bi bi-calendar-event",
        ActivityType.DocumentUpload   => "bi bi-file-earmark-arrow-up",
        ActivityType.DocumentDownload => "bi bi-file-earmark-arrow-down",
        ActivityType.Note             => "bi bi-sticky",
        ActivityType.StatusChange     => "bi bi-arrow-repeat",
        ActivityType.CaseCreated      => "bi bi-folder-plus",
        ActivityType.CaseClosed       => "bi bi-folder-x",
        _                             => "bi bi-circle"
    };
}

public class AddCaseActivityDto
{
    [Required]
    public ActivityType ActivityType { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public DateTime? ActivityDate { get; set; }

    public DateTime? EventDate { get; set; }

    [MaxLength(100)]
    public string Category { get; set; } = ActivityCategory.Other;

    public List<int> LinkedDocumentIds { get; set; } = [];
}
