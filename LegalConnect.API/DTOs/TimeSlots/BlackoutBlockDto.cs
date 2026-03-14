namespace LegalConnect.API.DTOs.TimeSlots;

public class BlackoutBlockDto
{
    public int Id { get; set; }
    public int LawyerProfileId { get; set; }
    public int DayOfWeek { get; set; }
    public string DayName => GetDayName(DayOfWeek);
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string RecurringPattern { get; set; } = "Weekly";
    public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    private static string GetDayName(int dayOfWeek)
    {
        return dayOfWeek switch
        {
            0 => "Sunday",
            1 => "Monday",
            2 => "Tuesday",
            3 => "Wednesday",
            4 => "Thursday",
            5 => "Friday",
            6 => "Saturday",
            _ => "Unknown"
        };
    }
}

public class CreateBlackoutBlockDto
{
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string RecurringPattern { get; set; } = "Weekly";
}

public class UpdateBlackoutBlockDto
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string RecurringPattern { get; set; } = "Weekly";
}
