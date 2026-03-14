using System.ComponentModel.DataAnnotations;
using LegalConnect.API.DTOs.Lawyer;

namespace LegalConnect.API.DTOs.Admin;

public class PendingLawyerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string BarCouncilNumber { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}

public class RevenueStatsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal PlatformCommission { get; set; }
    public int TotalAppointments { get; set; }
    public int TotalClients { get; set; }
    public int TotalLawyers { get; set; }
    public int PendingApprovals { get; set; }
    public List<MonthlyRevenueDto> MonthlyBreakdown { get; set; } = [];
}

public class MonthlyRevenueDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int AppointmentCount { get; set; }
}

public class CommissionSettingDto
{
    public decimal DefaultCommissionPercentage { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}

public class SetCommissionDto
{
    [Range(0, 100)]
    public decimal CommissionPercentage { get; set; }
}

public class CreateCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    public string? IconClass { get; set; } = "bi bi-briefcase";
}

public class UpdateCategoryDto : CreateCategoryDto
{
    public int Id { get; set; }
}

public class RejectLawyerDto
{
    [Required]
    public string Reason { get; set; } = string.Empty;
}

public class AdminLawyerDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string City { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string BarCouncilNumber { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime RegisteredAt { get; set; }
}

public class AdminClientDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? City { get; set; }
    public bool IsActive { get; set; }
    public DateTime RegisteredAt { get; set; }
    public int TotalCases { get; set; }
}
