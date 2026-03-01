using System.ComponentModel.DataAnnotations;

namespace LegalConnect.API.DTOs.Cases;

public class CaseDocumentDto
{
    public int Id { get; set; }
    public int CaseId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public bool SharedWithAllLawyers { get; set; } = true;
    public List<int> SharedWithLawyerIds { get; set; } = [];
    public DateTime UploadedDate { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public string UploadedByRole { get; set; } = string.Empty;
    public string FileSizeDisplay => FileSize < 1024 * 1024
        ? string.Format("{0:F1} KB", FileSize / 1024.0)
        : string.Format("{0:F1} MB", FileSize / (1024.0 * 1024));
    public string FileIcon => ContentType switch
    {
        var c when c == "application/pdf" => "bi bi-file-earmark-pdf text-danger",
        var c when c == "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => "bi bi-file-earmark-word text-primary",
        var c when c.StartsWith("image/") => "bi bi-file-earmark-image text-success",
        _ => "bi bi-file-earmark"
    };
}

public class UploadDocumentDto
{
    [Required]
    [MaxLength(100)]
    public string DocumentType { get; set; } = string.Empty;

    public bool IsPrivate { get; set; } = false;

    public List<int> SharedWithLawyerIds { get; set; } = [];
}

public class CaseDocumentFilterDto
{
    public string? DocumentType { get; set; }
    public bool? IsPrivate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
