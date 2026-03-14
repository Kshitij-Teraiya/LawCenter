using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Dues;

namespace LegalConnect.Client.Services;

public interface IRefundInvoiceClientService
{
    Task<ApiResponse<PagedResult<RefundInvoiceModel>>?> GetAllAsync(int page = 1, int pageSize = 20);
    Task<ApiResponse<PagedResult<RefundInvoiceModel>>?> GetMyAsync(int page = 1, int pageSize = 20);
    Task<ApiResponse<RefundInvoiceModel>?> GetByIdAsync(int id);
    Task<ApiResponse<RefundInvoiceModel>?> CreateAsync(CreateRefundInvoiceModel model);
    Task<string> GetDownloadUrlAsync(int id);
}

public class RefundInvoiceClientService : IRefundInvoiceClientService
{
    private readonly HttpClient _http;

    public RefundInvoiceClientService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("secured");
    }

    public async Task<ApiResponse<PagedResult<RefundInvoiceModel>>?> GetAllAsync(int page = 1, int pageSize = 20)
        => await _http.GetFromJsonAsync<ApiResponse<PagedResult<RefundInvoiceModel>>>($"refund-invoices?page={page}&pageSize={pageSize}");

    public async Task<ApiResponse<PagedResult<RefundInvoiceModel>>?> GetMyAsync(int page = 1, int pageSize = 20)
        => await _http.GetFromJsonAsync<ApiResponse<PagedResult<RefundInvoiceModel>>>($"refund-invoices/my?page={page}&pageSize={pageSize}");

    public async Task<ApiResponse<RefundInvoiceModel>?> GetByIdAsync(int id)
        => await _http.GetFromJsonAsync<ApiResponse<RefundInvoiceModel>>($"refund-invoices/{id}");

    public async Task<ApiResponse<RefundInvoiceModel>?> CreateAsync(CreateRefundInvoiceModel model)
    {
        var resp = await _http.PostAsJsonAsync("refund-invoices", model);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<RefundInvoiceModel>>();
    }

    public Task<string> GetDownloadUrlAsync(int id)
    {
        var baseAddress = _http.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
        return Task.FromResult($"{baseAddress}/refund-invoices/{id}/download");
    }
}
