namespace LegalConnect.API.DTOs.TimeSlots;

public class HolidayPreferenceDto
{
    public int MasterHolidayId { get; set; }
    public string HolidayName { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string? Description { get; set; }
    public bool AppliesYearly { get; set; }
    public bool IsEnabled { get; set; }
}

public class SetHolidayPreferenceDto
{
    public bool IsEnabled { get; set; }
}
