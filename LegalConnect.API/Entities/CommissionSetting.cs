namespace LegalConnect.API.Entities;

public class CommissionSetting
{
    public int Id { get; set; }
    public decimal DefaultCommissionPercentage { get; set; } = 10;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    public string UpdatedBy { get; set; } = string.Empty;
}
