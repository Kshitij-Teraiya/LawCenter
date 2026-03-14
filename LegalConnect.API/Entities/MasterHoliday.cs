namespace LegalConnect.API.Entities;

/// <summary>
/// Platform-wide holidays created by admins.
/// Can be enabled/disabled by individual lawyers through LawyerHolidayPreference.
/// One-to-many relationship with LawyerHolidayPreference.
/// </summary>
public class MasterHoliday
{
    public int Id { get; set; }

    /// <summary>
    /// Name of the holiday (e.g., "New Year", "Christmas", "Independence Day")
    /// </summary>
    public string HolidayName { get; set; } = string.Empty;

    /// <summary>
    /// Date of the holiday
    /// </summary>
    public DateTime HolidayDate { get; set; }

    /// <summary>
    /// Optional description of the holiday
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this holiday recurs every year on the same date
    /// </summary>
    public bool AppliesYearly { get; set; } = false;

    /// <summary>
    /// Navigation property to LawyerHolidayPreference
    /// </summary>
    public virtual ICollection<LawyerHolidayPreference> LawyerPreferences { get; set; } = new List<LawyerHolidayPreference>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
