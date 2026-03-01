using System.ComponentModel.DataAnnotations;

namespace LegalConnect.API.DTOs.Auth;

public class GoogleLoginDto
{
    [Required]
    public string Credential { get; set; } = string.Empty;
}
