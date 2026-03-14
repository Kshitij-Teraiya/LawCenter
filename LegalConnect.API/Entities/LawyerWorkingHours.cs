namespace LegalConnect.API.Entities;

/// <summary>
/// Defines working hours for a lawyer on a specific day of the week.
/// Allows multiple time ranges per day (e.g., morning 9-1, afternoon 2-6).
/// Many-to-one relationship with LawyerProfile.
/// </summary>
public class LawyerWorkingHours
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
    /// Start time of work shift (e.g., 09:00)
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// End time of work shift (e.g., 18:00)
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Whether lawyer works on this day of week
    /// False means no appointments are available on this day
    /// </summary>
    public bool IsWorking { get; set; } = true;

    /// <summary>
    /// Navigation property to LawyerProfile
    /// </summary>
    public virtual LawyerProfile? LawyerProfile { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
