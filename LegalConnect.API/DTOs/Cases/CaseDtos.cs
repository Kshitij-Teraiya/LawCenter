using System.ComponentModel.DataAnnotations;
using LegalConnect.API.Entities;

namespace LegalConnect.API.DTOs.Cases;

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

public class CaseDto : CaseSummaryDto
{
    public int? LawyerProfileId { get; set; }
    public int ClientProfileId { get; set; }
    public int? AppointmentId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Outcome { get; set; }
    public string? LawyerProfilePicture { get; set; }
    public string? ClientProfilePicture { get; set; }
    public List<CaseLawyerDto> AssignedLawyers { get; set; } = [];
    public List<CaseActivityDto> RecentActivities { get; set; } = [];
    public List<CaseDocumentDto> Documents { get; set; } = [];
}

public class CreateCaseDto
{
    [Required]
    [MaxLength(300)]
    public string CaseTitle { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CaseNumber { get; set; }

    [Required]
    [MaxLength(100)]
    public string CaseType { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Court { get; set; } = string.Empty;

    public int? ClientProfileId { get; set; }

    public List<int> LawyerProfileIds { get; set; } = [];

    public int? AppointmentId { get; set; }
    public int? LawyerClientId { get; set; }
    public DateTime? FilingDate { get; set; }
    public DateTime? NextHearingDate { get; set; }

    [Required]
    [MaxLength(3000)]
    public string Description { get; set; } = string.Empty;
}

public class UpdateCaseDto
{
    [Required]
    [MaxLength(300)]
    public string CaseTitle { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CaseNumber { get; set; }

    [Required]
    [MaxLength(100)]
    public string CaseType { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Court { get; set; } = string.Empty;

    public DateTime? FilingDate { get; set; }
    public DateTime? NextHearingDate { get; set; }
    public CaseStatus Status { get; set; }

    [Required]
    [MaxLength(3000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Outcome { get; set; }
}

public class CloseCaseDto
{
    [Required]
    [MaxLength(2000)]
    public string Outcome { get; set; } = string.Empty;
}

public class CaseFilterDto
{
    public string? SearchTerm { get; set; }
    public CaseStatus? Status { get; set; }
    public string? CaseType { get; set; }
    public string? Court { get; set; }
    public int? ClientProfileId { get; set; }
    public DateTime? FilingDateFrom { get; set; }
    public DateTime? FilingDateTo { get; set; }
    public string SortBy { get; set; } = "modifiedDate";
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
