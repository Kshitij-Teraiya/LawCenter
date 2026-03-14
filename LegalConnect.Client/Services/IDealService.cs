using System.Net.Http.Json;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Deals;
using Microsoft.AspNetCore.Components.Forms;

namespace LegalConnect.Client.Services;

public interface IDealService
{
    // HireRequest
    Task<ApiResponse<HireRequestDto>?> CreateHireRequestAsync(CreateHireRequestDto dto);
    Task<ApiResponse<List<HireRequestDto>>?> GetMyHireRequestsAsync();
    Task<ApiResponse<List<ClientInvoiceSummaryDto>>?> GetMyInvoicesAsync();
    Task<ApiResponse<List<HireRequestDto>>?> GetIncomingHireRequestsAsync();
    Task<ApiResponse<HireRequestDetailDto>?> GetHireRequestByIdAsync(int id);
    Task<ApiResponse<DealDto>?> AcceptHireRequestAsync(int id);
    Task<ApiResponse?> RejectHireRequestAsync(int id);
    Task<ApiResponse?> CancelHireRequestAsync(int id);

    // Messages
    Task<ApiResponse<List<HireRequestMessageDto>>?> GetMessagesAsync(int hireRequestId);
    Task<ApiResponse<HireRequestMessageDto>?> SendMessageAsync(int hireRequestId, SendMessageDto dto);
    Task<ApiResponse?> MarkMessagesReadAsync(int hireRequestId);

    // Proposals (on Deal)
    Task<ApiResponse<ProposalDto>?> CreateProposalAsync(int dealId, CreateProposalDto dto);
    Task<ApiResponse?> AcceptProposalAsync(int proposalId, RespondToProposalDto? dto);
    Task<ApiResponse?> RejectProposalAsync(int proposalId, RespondToProposalDto? dto);

    // Invoices (on Deal)
    Task<ApiResponse<InvoiceDto>?> GenerateInvoiceAsync(int dealId, CreateInvoiceDto dto);
    Task<ApiResponse?> AcceptInvoiceAsync(int invoiceId);
    Task<ApiResponse?> MarkInvoicePaidAsync(int invoiceId);
    Task<ApiResponse?> RejectInvoiceAsync(int invoiceId);

    // Invoice Settings
    Task<ApiResponse<LawyerInvoiceSettingsDto>?> GetInvoiceSettingsAsync();
    Task<ApiResponse<LawyerInvoiceSettingsDto>?> UpsertInvoiceSettingsAsync(UpsertLawyerInvoiceSettingsDto dto);

    // Earnings
    Task<ApiResponse<List<LawyerPaidInvoiceDto>>?> GetMyPaidInvoicesAsync();

    // Hire Request Documents
    Task<ApiResponse<List<HireRequestDocumentDto>>?> GetHireRequestDocumentsAsync(int hireRequestId);
    Task<ApiResponse<HireRequestDocumentDto>?> UploadHireRequestDocumentAsync(int hireRequestId, IBrowserFile file);
    Task<ApiResponse?> DeleteHireRequestDocumentAsync(int hireRequestId, int docId);
    Task<byte[]?> DownloadHireRequestDocumentAsync(int hireRequestId, int docId);
    Task<byte[]?> DownloadCaseDocumentViaHireRequestAsync(int hireRequestId, int caseDocId);
}

public class DealService : IDealService
{
    private readonly HttpClient _http;

    public DealService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("secured");
    }

    // ── HireRequest ──────────────────────────────────────────────────

    public async Task<ApiResponse<HireRequestDto>?> CreateHireRequestAsync(CreateHireRequestDto dto)
    {
        var resp = await _http.PostAsJsonAsync("hire-requests", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<HireRequestDto>>();
    }

    public async Task<ApiResponse<List<HireRequestDto>>?> GetMyHireRequestsAsync()
        => await _http.GetFromJsonAsync<ApiResponse<List<HireRequestDto>>>("hire-requests/my");

    public async Task<ApiResponse<List<ClientInvoiceSummaryDto>>?> GetMyInvoicesAsync()
        => await _http.GetFromJsonAsync<ApiResponse<List<ClientInvoiceSummaryDto>>>("hire-requests/my-invoices");

    public async Task<ApiResponse<List<HireRequestDto>>?> GetIncomingHireRequestsAsync()
        => await _http.GetFromJsonAsync<ApiResponse<List<HireRequestDto>>>("hire-requests/incoming");

    public async Task<ApiResponse<HireRequestDetailDto>?> GetHireRequestByIdAsync(int id)
        => await _http.GetFromJsonAsync<ApiResponse<HireRequestDetailDto>>($"hire-requests/{id}");

    public async Task<ApiResponse<DealDto>?> AcceptHireRequestAsync(int id)
    {
        var resp = await _http.PutAsync($"hire-requests/{id}/accept", null);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<DealDto>>();
    }

    public async Task<ApiResponse?> RejectHireRequestAsync(int id)
    {
        var resp = await _http.PutAsync($"hire-requests/{id}/reject", null);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse?> CancelHireRequestAsync(int id)
    {
        var resp = await _http.PutAsync($"hire-requests/{id}/cancel", null);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    // ── Messages ─────────────────────────────────────────────────────

    public async Task<ApiResponse<List<HireRequestMessageDto>>?> GetMessagesAsync(int hireRequestId)
        => await _http.GetFromJsonAsync<ApiResponse<List<HireRequestMessageDto>>>($"hire-requests/{hireRequestId}/messages");

    public async Task<ApiResponse<HireRequestMessageDto>?> SendMessageAsync(int hireRequestId, SendMessageDto dto)
    {
        var resp = await _http.PostAsJsonAsync($"hire-requests/{hireRequestId}/messages", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<HireRequestMessageDto>>();
    }

    public async Task<ApiResponse?> MarkMessagesReadAsync(int hireRequestId)
    {
        var resp = await _http.PutAsync($"hire-requests/{hireRequestId}/messages/read", null);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    // ── Proposals (on Deal) ──────────────────────────────────────────

    public async Task<ApiResponse<ProposalDto>?> CreateProposalAsync(int dealId, CreateProposalDto dto)
    {
        var resp = await _http.PostAsJsonAsync($"hire-requests/deals/{dealId}/proposals", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<ProposalDto>>();
    }

    public async Task<ApiResponse?> AcceptProposalAsync(int proposalId, RespondToProposalDto? dto)
    {
        var resp = await _http.PutAsJsonAsync($"hire-requests/proposals/{proposalId}/accept", dto ?? new());
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse?> RejectProposalAsync(int proposalId, RespondToProposalDto? dto)
    {
        var resp = await _http.PutAsJsonAsync($"hire-requests/proposals/{proposalId}/reject", dto ?? new());
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    // ── Invoices (on Deal) ───────────────────────────────────────────

    public async Task<ApiResponse<InvoiceDto>?> GenerateInvoiceAsync(int dealId, CreateInvoiceDto dto)
    {
        var resp = await _http.PostAsJsonAsync($"hire-requests/deals/{dealId}/invoices", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<InvoiceDto>>();
    }

    public async Task<ApiResponse?> AcceptInvoiceAsync(int invoiceId)
    {
        var resp = await _http.PutAsync($"hire-requests/invoices/{invoiceId}/accept", null);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse?> MarkInvoicePaidAsync(int invoiceId)
    {
        var resp = await _http.PutAsync($"hire-requests/invoices/{invoiceId}/paid", null);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse?> RejectInvoiceAsync(int invoiceId)
    {
        var resp = await _http.PutAsync($"hire-requests/invoices/{invoiceId}/reject", null);
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    // ── Invoice Settings ─────────────────────────────────────────────

    public async Task<ApiResponse<LawyerInvoiceSettingsDto>?> GetInvoiceSettingsAsync()
        => await _http.GetFromJsonAsync<ApiResponse<LawyerInvoiceSettingsDto>>("hire-requests/invoice-settings");

    public async Task<ApiResponse<LawyerInvoiceSettingsDto>?> UpsertInvoiceSettingsAsync(UpsertLawyerInvoiceSettingsDto dto)
    {
        var resp = await _http.PutAsJsonAsync("hire-requests/invoice-settings", dto);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<LawyerInvoiceSettingsDto>>();
    }

    // ── Hire Request Documents ────────────────────────────────────────

    public async Task<ApiResponse<List<HireRequestDocumentDto>>?> GetHireRequestDocumentsAsync(int hireRequestId)
        => await _http.GetFromJsonAsync<ApiResponse<List<HireRequestDocumentDto>>>($"hire-requests/{hireRequestId}/documents");

    public async Task<ApiResponse<HireRequestDocumentDto>?> UploadHireRequestDocumentAsync(int hireRequestId, IBrowserFile file)
    {
        const long maxSize = 20 * 1024 * 1024;
        using var content = new MultipartFormDataContent();
        var stream = file.OpenReadStream(maxSize);
        content.Add(new StreamContent(stream), "file", file.Name);
        var resp = await _http.PostAsync($"hire-requests/{hireRequestId}/documents", content);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<HireRequestDocumentDto>>();
    }

    public async Task<ApiResponse?> DeleteHireRequestDocumentAsync(int hireRequestId, int docId)
    {
        var resp = await _http.DeleteAsync($"hire-requests/{hireRequestId}/documents/{docId}");
        return await resp.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<byte[]?> DownloadHireRequestDocumentAsync(int hireRequestId, int docId)
    {
        try { return await _http.GetByteArrayAsync($"hire-requests/{hireRequestId}/documents/{docId}/download"); }
        catch { return null; }
    }

    public async Task<byte[]?> DownloadCaseDocumentViaHireRequestAsync(int hireRequestId, int caseDocId)
    {
        try { return await _http.GetByteArrayAsync($"hire-requests/{hireRequestId}/case-documents/{caseDocId}/download"); }
        catch { return null; }
    }

    public async Task<ApiResponse<List<LawyerPaidInvoiceDto>>?> GetMyPaidInvoicesAsync()
        => await _http.GetFromJsonAsync<ApiResponse<List<LawyerPaidInvoiceDto>>>("hire-requests/my-paid-invoices");
}
