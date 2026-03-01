using System.ComponentModel.DataAnnotations;

namespace LegalConnect.Client.Models.Auth;

public class RegisterLawyerDto
{
    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bar Council number is required.")]
    public string BarCouncilNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a specialization.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "City is required.")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Court is required.")]
    public string Court { get; set; } = string.Empty;

    [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50.")]
    public int YearsOfExperience { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Enter a valid consultation fee.")]
    public decimal ConsultationFee { get; set; }
}
