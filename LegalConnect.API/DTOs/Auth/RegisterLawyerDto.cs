using System.ComponentModel.DataAnnotations;

namespace LegalConnect.API.DTOs.Auth;

public class RegisterLawyerDto
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string BarCouncilNumber { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string Court { get; set; } = string.Empty;

    [Range(0, 50)]
    public int YearsOfExperience { get; set; }

    [Range(0, 1_000_000)]
    public decimal ConsultationFee { get; set; }

    [Required]
    public bool AcceptedTnC { get; set; }
}
