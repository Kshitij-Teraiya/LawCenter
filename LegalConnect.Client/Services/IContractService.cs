using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Contracts;

namespace LegalConnect.Client.Services;

public interface IContractService
{
    Task<ApiResponse<PagedResult<LegalContractModel>>?> GetContractsAsync(ContractFilterModel filter);
    Task<ApiResponse<LegalContractModel>?> GetByIdAsync(int id);
    Task<byte[]?> DownloadContractAsync(int id);
}

public class ContractService : IContractService
{
    private readonly HttpClient _http;

    public ContractService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("secured");
    }

    public async Task<ApiResponse<PagedResult<LegalContractModel>>?> GetContractsAsync(ContractFilterModel filter)
    {
        var qs = $"contracts?page={filter.Page}&pageSize={filter.PageSize}";
        if (!string.IsNullOrWhiteSpace(filter.ContractType)) qs += $"&contractType={Uri.EscapeDataString(filter.ContractType)}";
        if (!string.IsNullOrWhiteSpace(filter.LawyerName))   qs += $"&lawyerName={Uri.EscapeDataString(filter.LawyerName)}";
        if (filter.DateFrom.HasValue) qs += $"&dateFrom={filter.DateFrom.Value:yyyy-MM-dd}";
        if (filter.DateTo.HasValue)   qs += $"&dateTo={filter.DateTo.Value:yyyy-MM-dd}";

        return await _http.GetFromJsonAsync<ApiResponse<PagedResult<LegalContractModel>>>(qs);
    }

    public async Task<ApiResponse<LegalContractModel>?> GetByIdAsync(int id)
        => await _http.GetFromJsonAsync<ApiResponse<LegalContractModel>>($"contracts/{id}");

    public async Task<byte[]?> DownloadContractAsync(int id)
    {
        try { return await _http.GetByteArrayAsync($"contracts/{id}/download"); }
        catch { return null; }
    }
}
