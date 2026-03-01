using System.ComponentModel.DataAnnotations;

namespace LegalConnect.API.DTOs.Cases;

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
    [Required]
    public int ClientProfileId { get; set; }

    public int? FirstAppointmentId { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}

public class UpdateLawyerClientNotesDto
{
    [MaxLength(2000)]
    public string? Notes { get; set; }

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
}

/// <summary>Returned to a Client listing their hired lawyers (for case creation).</summary>
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
