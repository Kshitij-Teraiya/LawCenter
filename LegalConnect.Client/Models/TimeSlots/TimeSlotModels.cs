namespace LegalConnect.Client.Models.TimeSlots;

public class TimeSlotConfigurationModel
{
    public int SessionDurationMinutes { get; set; } = 60;
    public int BufferTimeMinutes { get; set; } = 0;
}

public class UpdateTimeSlotConfigurationModel
{
    public int SessionDurationMinutes { get; set; } = 60;
    public int BufferTimeMinutes { get; set; } = 0;
}

public class WorkingHoursModel
{
    public int Id { get; set; }
    public int LawyerProfileId { get; set; }
    public int DayOfWeek { get; set; }
    public string DayName => DayOfWeek switch
    {
        0 => "Sunday", 1 => "Monday", 2 => "Tuesday",
        3 => "Wednesday", 4 => "Thursday", 5 => "Friday",
        6 => "Saturday", _ => "Unknown"
    };
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsWorking { get; set; }
    public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateWorkingHoursModel
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsWorking { get; set; }
}

public class BlackoutBlockModel
{
    public int Id { get; set; }
    public int LawyerProfileId { get; set; }
    public int DayOfWeek { get; set; }
    public string DayName => DayOfWeek switch
    {
        0 => "Sunday", 1 => "Monday", 2 => "Tuesday",
        3 => "Wednesday", 4 => "Thursday", 5 => "Friday",
        6 => "Saturday", _ => "Unknown"
    };
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string RecurringPattern { get; set; } = "Weekly";
    public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateBlackoutBlockModel
{
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string RecurringPattern { get; set; } = "Weekly";
}

public class MasterHolidayModel
{
    public int Id { get; set; }
    public string HolidayName { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string? Description { get; set; }
    public bool AppliesYearly { get; set; }
    public int LawyersObservingCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateMasterHolidayModel
{
    public string HolidayName { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; } = DateTime.Today;
    public string? Description { get; set; }
    public bool AppliesYearly { get; set; }
}

public class UpdateMasterHolidayModel
{
    public string HolidayName { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string? Description { get; set; }
    public bool AppliesYearly { get; set; }
}

public class PersonalHolidayModel
{
    public int Id { get; set; }
    public int LawyerProfileId { get; set; }
    public DateTime HolidayDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string RecurringPattern { get; set; } = "None";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreatePersonalHolidayModel
{
    public DateTime HolidayDate { get; set; } = DateTime.Today;
    public string Reason { get; set; } = string.Empty;
    public string RecurringPattern { get; set; } = "None";
}

public class HolidayPreferenceModel
{
    public int MasterHolidayId { get; set; }
    public string HolidayName { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string? Description { get; set; }
    public bool AppliesYearly { get; set; }
    public bool IsEnabled { get; set; }
}

public class RescheduleAppointmentModel
{
    public DateTime NewDate { get; set; }
    public TimeSpan NewStartTime { get; set; }
}
