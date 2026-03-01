using System.ComponentModel.DataAnnotations;

namespace LegalConnect.Client.Models.Lawyer;

public class UpdateLawyerProfileDto
{
    [Required, MaxLength(50)] public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string LastName  { get; set; } = string.Empty;
    [Phone]                   public string PhoneNumber { get; set; } = string.Empty;
    [Required]                public string City { get; set; } = string.Empty;
    [Required]                public string Court { get; set; } = string.Empty;
    [MaxLength(1000)]         public string? Bio { get; set; }
    [Range(0, 1_000_000)]     public decimal ConsultationFee { get; set; }
    public bool IsAvailable { get; set; } = true;
}

public class AddExperienceDto
{
    [Required] public string Title        { get; set; } = string.Empty;
    [Required] public string Organization { get; set; } = string.Empty;
    [Required] public DateTime StartDate  { get; set; } = DateTime.Today;
    public DateTime? EndDate   { get; set; }
    public bool IsCurrent      { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
}

public class AddCaseResultDto
{
    [Required]                public string CaseTitle   { get; set; } = string.Empty;
    [Required]                public string Court       { get; set; } = string.Empty;
    [Required]                public string Outcome     { get; set; } = string.Empty;
    [Range(1950, 2100)]       public int Year           { get; set; } = DateTime.Today.Year;
    [MaxLength(500)]          public string? Description { get; set; }
}

public class SetServiceChargeDto
{
    [Range(0, 1_000_000, ErrorMessage = "Enter a valid fee amount.")]
    public decimal ConsultationFee { get; set; }
}
