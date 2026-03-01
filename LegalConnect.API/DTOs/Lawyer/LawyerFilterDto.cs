namespace LegalConnect.API.DTOs.Lawyer;

public class LawyerFilterDto
{
    public string? SearchTerm { get; set; }
    public string? City { get; set; }
    public int? CategoryId { get; set; }
    public string? Court { get; set; }
    public int? MinExperience { get; set; }
    public int? MaxExperience { get; set; }
    public decimal? MinFee { get; set; }
    public decimal? MaxFee { get; set; }
    public decimal? MinRating { get; set; }
    public bool? IsVerified { get; set; }
    public bool? IsAvailable { get; set; }
    public string SortBy { get; set; } = "rating";
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
