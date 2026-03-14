namespace LegalConnect.API.Entities;

/// <summary>
/// Recurring time blocks within working hours where appointments cannot be booked.
/// Used for team meetings, lunch, breaks, etc. (e.g., "2-3 PM every Monday").
/// Many-to-one relationship with LawyerProfile.
/// </summary>
public class LawyerBlackoutBlock
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to LawyerProfile
    /// </summary>
    public int LawyerProfileId { get; set; }

    /// <summary>
    /// Day of week: 0 = Sunday, 1 = Monday, ..., 6 = Saturday
    /// </summary>
    public int DayOfWeek { get; set; }

    /// <summary>
    /// Start time of the blackout block (e.g., 13:00)
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// End time of the blackout block (e.g., 14:00)
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Reason for the blackout block (e.g., "Team Meeting", "Lunch Break")
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Recurring pattern for the blackout block
    /// None = one-time only (specific occurrence)
    /// Weekly = same time every week
    /// MonthlyDate = same date/time every month
    /// Yearly = same date/time every year
    /// </summary>
    public string RecurringPattern { get; set; } = "Weekly";

    /// <summary>
    /// Navigation property to LawyerProfile
    /// </summary>
    public virtual LawyerProfile? LawyerProfile { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
