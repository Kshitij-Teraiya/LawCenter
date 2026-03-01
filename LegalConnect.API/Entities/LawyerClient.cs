namespace LegalConnect.API.Entities;

public class LawyerClient
{
    public int Id { get; set; }
    public int LawyerProfileId { get; set; }
    public int ClientProfileId { get; set; }
    public int? FirstAppointmentId { get; set; }
    public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public LawyerProfile LawyerProfile { get; set; } = null!;
    public ClientProfile ClientProfile { get; set; } = null!;
    public Appointment? FirstAppointment { get; set; }
    public List<Case> Cases { get; set; } = [];
}
