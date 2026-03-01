using System.ComponentModel.DataAnnotations;

namespace LegalConnect.Client.Models.Appointment;

public class BookAppointmentDto
{
    [Required] public int LawyerId { get; set; }

    [Required(ErrorMessage = "Please select a date.")]
    public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Please select a time slot.")]
    public TimeSpan StartTime { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class CancelAppointmentDto
{
    [Required, MaxLength(300, ErrorMessage = "Reason must be under 300 characters.")]
    public string Reason { get; set; } = string.Empty;
}
