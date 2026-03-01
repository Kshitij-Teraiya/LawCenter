namespace LegalConnect.API.Entities;

public class Review
{
    public int Id { get; set; }
    public int LawyerProfileId { get; set; }
    public int ClientProfileId { get; set; }
    public int AppointmentId { get; set; }
    public int Rating { get; set; } // 1-5
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public LawyerProfile LawyerProfile { get; set; } = null!;
    public ClientProfile ClientProfile { get; set; } = null!;
    public Appointment Appointment { get; set; } = null!;
}
