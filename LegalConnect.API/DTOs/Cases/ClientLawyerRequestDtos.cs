using System.ComponentModel.DataAnnotations;

namespace LegalConnect.API.DTOs.Cases;

public class ClientLawyerRequestDto
{
    public int Id { get; set; }
    public int ClientProfileId { get; set; }
    public int LawyerProfileId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ClientFullName { get; set; } = string.Empty;
    public string? ClientEmail { get; set; }
    public string? ClientPicture { get; set; }
    public string LawyerFullName { get; set; } = string.Empty;
    public string? LawyerPicture { get; set; }
    public string? LawyerCity { get; set; }
    public string? LawyerCategoryName { get; set; }
    public string? Message { get; set; }
    public string? LawyerNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateClientLawyerRequestDto
{
    [Required]
    public int LawyerProfileId { get; set; }

    [MaxLength(1000)]
    public string? Message { get; set; }
}

public class RespondToRequestDto
{
    [Required]
    public bool Accept { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}
