namespace LegalConnect.Client.Models.Appointment;

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed
}

public class AppointmentDto
{
    public int Id { get; set; }
    public int LawyerId { get; set; }
    public string LawyerName { get; set; } = string.Empty;
    public string? LawyerProfilePicture { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public decimal ConsultationFee { get; set; }
    public decimal PlatformCommission { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TimeSlotDto
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }

    public string DisplayTime =>
        $"{DateTime.Today.Add(StartTime):hh:mm tt} – {DateTime.Today.Add(EndTime):hh:mm tt}";
}
