namespace LegalConnect.API.DTOs.TimeSlots;

public class PersonalHolidayDto
{
    public int Id { get; set; }
    public int LawyerProfileId { get; set; }
    public DateTime HolidayDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string RecurringPattern { get; set; } = "None";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreatePersonalHolidayDto
{
    public DateTime HolidayDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string RecurringPattern { get; set; } = "None";
}

public class UpdatePersonalHolidayDto
{
    public DateTime HolidayDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string RecurringPattern { get; set; } = "None";
}
