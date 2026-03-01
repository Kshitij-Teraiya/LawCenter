namespace LegalConnect.Client.Models.Lawyer;

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
    public string SortBy { get; set; } = "rating";   // rating | fee | experience | name
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    /// <summary>Convert to query string for the API GET request.</summary>
    public string ToQueryString()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(SearchTerm))  parts.Add($"search={Uri.EscapeDataString(SearchTerm)}");
        if (!string.IsNullOrWhiteSpace(City))         parts.Add($"city={Uri.EscapeDataString(City)}");
        if (CategoryId.HasValue)                       parts.Add($"categoryId={CategoryId}");
        if (!string.IsNullOrWhiteSpace(Court))         parts.Add($"court={Uri.EscapeDataString(Court)}");
        if (MinExperience.HasValue)                    parts.Add($"minExp={MinExperience}");
        if (MaxExperience.HasValue)                    parts.Add($"maxExp={MaxExperience}");
        if (MinFee.HasValue)                           parts.Add($"minFee={MinFee}");
        if (MaxFee.HasValue)                           parts.Add($"maxFee={MaxFee}");
        if (MinRating.HasValue)                        parts.Add($"minRating={MinRating}");
        if (IsVerified.HasValue)                       parts.Add($"verified={IsVerified}");
        if (IsAvailable.HasValue)                      parts.Add($"available={IsAvailable}");
        parts.Add($"sortBy={SortBy}");
        parts.Add($"sortDesc={SortDescending}");
        parts.Add($"page={PageNumber}");
        parts.Add($"pageSize={PageSize}");

        return string.Join("&", parts);
    }
}
