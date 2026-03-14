namespace LegalConnect.API.Entities;

/// <summary>
/// Junction table for lawyer's holiday preferences.
/// Allows lawyers to enable/disable specific master holidays.
/// Many-to-many relationship between LawyerProfile and MasterHoliday.
/// </summary>
public class LawyerHolidayPreference
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to LawyerProfile
    /// </summary>
    public int LawyerProfileId { get; set; }

    /// <summary>
    /// Foreign key to MasterHoliday
    /// </summary>
    public int MasterHolidayId { get; set; }

    /// <summary>
    /// Whether this lawyer observes this holiday
    /// False means slots will be available on this date
    /// True means no slots available on this date
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// Navigation property to LawyerProfile
    /// </summary>
    public virtual LawyerProfile? LawyerProfile { get; set; }

    /// <summary>
    /// Navigation property to MasterHoliday
    /// </summary>
    public virtual MasterHoliday? MasterHoliday { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
