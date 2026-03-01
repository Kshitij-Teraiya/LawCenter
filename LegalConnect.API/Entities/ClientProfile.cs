namespace LegalConnect.API.Entities;

public class ClientProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? City { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ApplicationUser User { get; set; } = null!;
    public List<Appointment> Appointments { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
    public List<LawyerClient> LawyerClients { get; set; } = [];
    public List<Case> Cases { get; set; } = [];
    public List<ClientLawyerRequest> LawyerRequests { get; set; } = [];
    public List<HireRequest> HireRequests { get; set; } = [];
}
