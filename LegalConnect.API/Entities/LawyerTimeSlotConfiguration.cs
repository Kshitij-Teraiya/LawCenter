namespace LegalConnect.API.Entities;

/// <summary>
/// Stores lawyer's appointment session duration and buffer time configuration.
/// 1:1 relationship with LawyerProfile.
/// </summary>
public class LawyerTimeSlotConfiguration
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to LawyerProfile
    /// </summary>
    public int LawyerProfileId { get; set; }

    /// <summary>
    /// Session duration in minutes: 15, 30, 45, or 60
    /// Default: 60 (1 hour) for backward compatibility
    /// </summary>
    public int SessionDurationMinutes { get; set; } = 60;

    /// <summary>
    /// Buffer time in minutes between appointments: 0-60
    /// E.g., 15 means 15 minutes gap between end of one appointment and start of next
    /// Default: 0 (no buffer)
    /// </summary>
    public int BufferTimeMinutes { get; set; } = 0;

    /// <summary>
    /// Navigation property to LawyerProfile
    /// </summary>
    public virtual LawyerProfile? LawyerProfile { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
