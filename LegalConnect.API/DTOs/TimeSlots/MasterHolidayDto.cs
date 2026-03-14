namespace LegalConnect.API.DTOs.TimeSlots;

public class MasterHolidayDto
{
    public int Id { get; set; }
    public string HolidayName { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string? Description { get; set; }
    public bool AppliesYearly { get; set; }
    public int LawyersObservingCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateMasterHolidayDto
{
    public string HolidayName { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string? Description { get; set; }
    public bool AppliesYearly { get; set; } = false;
}

public class UpdateMasterHolidayDto
{
    public string HolidayName { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string? Description { get; set; }
    public bool AppliesYearly { get; set; }
}
