namespace LegalConnect.API.Entities;

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed
}

public class Appointment
{
    public int Id { get; set; }
    public int LawyerProfileId { get; set; }
    public int ClientProfileId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public decimal ConsultationFee { get; set; }
    public decimal PlatformCommission { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public LawyerProfile LawyerProfile { get; set; } = null!;
    public ClientProfile ClientProfile { get; set; } = null!;
    public Review? Review { get; set; }
}
