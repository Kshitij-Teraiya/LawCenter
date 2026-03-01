namespace LegalConnect.API.Entities;

public enum ClientLawyerRequestStatus
{
    Pending  = 0,
    Accepted = 1,
    Rejected = 2
}

public class ClientLawyerRequest
{
    public int Id { get; set; }
    public int ClientProfileId { get; set; }
    public int LawyerProfileId { get; set; }
    public ClientLawyerRequestStatus Status { get; set; } = ClientLawyerRequestStatus.Pending;

    /// <summary>Optional message from the client to the lawyer.</summary>
    public string? Message { get; set; }

    /// <summary>Lawyer's note when accepting or rejecting.</summary>
    public string? LawyerNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    // Navigation
    public ClientProfile ClientProfile { get; set; } = null!;
    public LawyerProfile LawyerProfile { get; set; } = null!;
}
