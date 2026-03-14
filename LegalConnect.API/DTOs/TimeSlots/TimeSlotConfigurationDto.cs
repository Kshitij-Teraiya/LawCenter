namespace LegalConnect.API.DTOs.TimeSlots;

public class TimeSlotConfigurationDto
{
    public int LawyerProfileId { get; set; }
    public int SessionDurationMinutes { get; set; }
    public int BufferTimeMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateTimeSlotConfigurationDto
{
    public int SessionDurationMinutes { get; set; }
    public int BufferTimeMinutes { get; set; }
}
