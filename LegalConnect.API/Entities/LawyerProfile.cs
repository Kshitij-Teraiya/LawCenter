namespace LegalConnect.API.Entities;

public class LawyerProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string BarCouncilNumber { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string City { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public decimal ConsultationFee { get; set; }
    public string? Bio { get; set; }
    public bool IsVerified { get; set; } = false;
    public bool IsAvailable { get; set; } = true;
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ApplicationUser User { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public List<Experience> Experiences { get; set; } = [];
    public List<CaseResult> CaseResults { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
    public List<Certification> Certifications { get; set; } = [];
    public List<Publication> Publications { get; set; } = [];
    public List<Appointment> Appointments { get; set; } = [];
    public List<LawyerClient> LawyerClients { get; set; } = [];
    public List<Case> Cases { get; set; } = [];
    public List<CaseLawyer> CaseLawyers { get; set; } = [];
    public List<ClientLawyerRequest> IncomingRequests { get; set; } = [];
    public List<HireRequest> HireRequests { get; set; } = [];
    // Staff
    public List<StaffProfile> StaffMembers { get; set; } = [];
    // Invoice Settings
    public LawyerInvoiceSettings? InvoiceSettings { get; set; }
    // Appointment Slot Management
    public LawyerTimeSlotConfiguration? TimeSlotConfiguration { get; set; }
    public List<LawyerWorkingHours> WorkingHours { get; set; } = [];
    public List<LawyerHolidayPreference> HolidayPreferences { get; set; } = [];
    public List<LawyerPersonalHoliday> PersonalHolidays { get; set; } = [];
    public List<LawyerBlackoutBlock> BlackoutBlocks { get; set; } = [];

    public decimal AverageRating => Reviews.Count > 0
        ? Math.Round((decimal)Reviews.Average(r => r.Rating), 1)
        : 0;

    public int TotalReviews => Reviews.Count;

    public int ProfileCompletionPercentage
    {
        get
        {
            int score = 0;
            if (!string.IsNullOrEmpty(Bio)) score += 20;
            if (Experiences.Count > 0) score += 20;
            if (CaseResults.Count > 0) score += 20;
            if (Certifications.Count > 0) score += 20;
            if (User?.ProfilePictureUrl != null) score += 20;
            return score;
        }
    }
}
