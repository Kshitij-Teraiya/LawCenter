namespace LegalConnect.Client.Models.Lawyer;

public class LawyerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public string BarCouncilNumber { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public decimal ConsultationFee { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsVerified { get; set; }
    public bool IsAvailable { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public CategoryDto? Category { get; set; }
    public List<ExperienceDto> Experiences { get; set; } = [];
    public List<CaseResultDto> CaseResults { get; set; } = [];
    public List<ReviewDto> Reviews { get; set; } = [];
    public List<CertificationDto> Certifications { get; set; } = [];
    public List<PublicationDto> Publications { get; set; } = [];
    public int ProfileCompletionPercentage { get; set; }
}

public class LawyerSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public decimal ConsultationFee { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsVerified { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? CategoryName { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconClass { get; set; }
    public string? Description { get; set; }
    public int LawyerCount { get; set; }
}

public class ExperienceDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string? Description { get; set; }
}

public class CaseResultDto
{
    public int Id { get; set; }
    public string CaseTitle { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Description { get; set; }
}

public class ReviewDto
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CertificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string IssuingOrganization { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? CertificateUrl { get; set; }
}

public class PublicationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Url { get; set; }
}
