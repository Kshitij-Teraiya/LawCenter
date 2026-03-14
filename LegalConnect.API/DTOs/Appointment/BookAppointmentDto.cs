using System.ComponentModel.DataAnnotations;

namespace LegalConnect.API.DTOs.Appointment;

public class BookAppointmentDto
{
    [Required]
    public int LawyerId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class CancelAppointmentDto
{
    [Required]
    [MaxLength(300)]
    public string Reason { get; set; } = string.Empty;
}

public class RescheduleAppointmentDto
{
    [Required]
    public DateTime NewDate { get; set; }

    [Required]
    public TimeSpan NewStartTime { get; set; }
}
