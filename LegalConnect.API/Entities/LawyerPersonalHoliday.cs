namespace LegalConnect.API.Entities;

/// <summary>
/// Personal holidays created by lawyers (vacation, personal days, etc.).
/// Can be one-time or recurring (weekly, monthly, yearly).
/// Many-to-one relationship with LawyerProfile.
/// </summary>
public class LawyerPersonalHoliday
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to LawyerProfile
    /// </summary>
    public int LawyerProfileId { get; set; }

    /// <summary>
    /// Date of the holiday
    /// </summary>
    public DateTime HolidayDate { get; set; }

    /// <summary>
    /// Reason for the holiday (e.g., "Vacation", "Personal Leave", "Medical Appointment")
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Recurring pattern for the holiday
    /// None = one-time only
    /// Weekly = same day every week for 52 weeks
    /// MonthlyDate = same date every month for 12 months
    /// Yearly = same date every year for 5 years
    /// </summary>
    public string RecurringPattern { get; set; } = "None";

    /// <summary>
    /// Navigation property to LawyerProfile
    /// </summary>
    public virtual LawyerProfile? LawyerProfile { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
