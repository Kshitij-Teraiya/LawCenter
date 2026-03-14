using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Support;
using LegalConnect.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface ISupportService
{
    Task<List<SupportTicketDto>> GetMyTicketsAsync(int userId);
    Task<List<SupportTicketDto>> GetAllTicketsAsync(string? statusFilter, string? categoryFilter);
    Task<SupportTicketDto?> GetTicketByIdAsync(int userId, string role, int ticketId);
    Task<(bool Success, string Message, SupportTicketDto? Data)> CreateTicketAsync(int userId, string role, string userName, CreateSupportTicketDto dto);
    Task<(bool Success, string Message)> UpdateTicketStatusAsync(int adminUserId, string adminName, int ticketId, UpdateTicketStatusDto dto);
    Task<List<SupportMessageDto>> GetMessagesAsync(int userId, string role, int ticketId);
    Task<(bool Success, string Message, SupportMessageDto? Data)> SendMessageAsync(int userId, string role, string userName, int ticketId, SendSupportMessageDto dto);
}

public class SupportService : ISupportService
{
    private readonly AppDbContext _db;

    public SupportService(AppDbContext db) => _db = db;

    public async Task<List<SupportTicketDto>> GetMyTicketsAsync(int userId)
    {
        return await _db.SupportTickets
            .Include(t => t.User)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.UpdatedAt)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<List<SupportTicketDto>> GetAllTicketsAsync(string? statusFilter, string? categoryFilter)
    {
        var query = _db.SupportTickets.Include(t => t.User).AsQueryable();

        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<TicketStatus>(statusFilter, true, out var status))
            query = query.Where(t => t.Status == status);

        if (!string.IsNullOrEmpty(categoryFilter) && Enum.TryParse<TicketCategory>(categoryFilter, true, out var category))
            query = query.Where(t => t.Category == category);

        return await query
            .OrderByDescending(t => t.UpdatedAt)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<SupportTicketDto?> GetTicketByIdAsync(int userId, string role, int ticketId)
    {
        var query = _db.SupportTickets
            .Include(t => t.User)
            .Include(t => t.ClosedByUser)
            .Include(t => t.Messages)
            .AsQueryable();

        if (role != "Admin")
            query = query.Where(t => t.UserId == userId);

        var ticket = await query.FirstOrDefaultAsync(t => t.Id == ticketId);
        return ticket == null ? null : MapToDto(ticket);
    }

    public async Task<(bool Success, string Message, SupportTicketDto? Data)> CreateTicketAsync(
        int userId, string role, string userName, CreateSupportTicketDto dto)
    {
        if (!Enum.TryParse<TicketCategory>(dto.Category, true, out var category))
            return (false, "Invalid category. Use: Billing, Technical, Account, General.", null);

        if (!Enum.TryParse<TicketPriority>(dto.Priority, true, out var priority))
            priority = TicketPriority.Medium;

        var ticket = new SupportTicket
        {
            UserId = userId,
            UserRole = role,
            Subject = dto.Subject.Trim(),
            Description = dto.Description.Trim(),
            Category = category,
            Priority = priority,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.SupportTickets.Add(ticket);
        await _db.SaveChangesAsync();

        // Reload with navigation
        await _db.Entry(ticket).Reference(t => t.User).LoadAsync();

        return (true, "Support ticket created successfully.", MapToDto(ticket));
    }

    public async Task<(bool Success, string Message)> UpdateTicketStatusAsync(
        int adminUserId, string adminName, int ticketId, UpdateTicketStatusDto dto)
    {
        if (!Enum.TryParse<TicketStatus>(dto.Status, true, out var newStatus))
            return (false, "Invalid status. Use: Open, InProgress, Resolved, Closed.");

        var ticket = await _db.SupportTickets.FindAsync(ticketId);
        if (ticket == null) return (false, "Ticket not found.");

        ticket.Status = newStatus;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (newStatus == TicketStatus.Closed)
        {
            ticket.ClosedAt = DateTime.UtcNow;
            ticket.ClosedByUserId = adminUserId;
        }
        else
        {
            ticket.ClosedAt = null;
            ticket.ClosedByUserId = null;
        }

        await _db.SaveChangesAsync();
        return (true, $"Ticket status updated to {newStatus}.");
    }

    public async Task<List<SupportMessageDto>> GetMessagesAsync(int userId, string role, int ticketId)
    {
        // Verify access
        var ticket = IsAdminRole(role)
            ? await _db.SupportTickets.FindAsync(ticketId)
            : await _db.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.UserId == userId);

        if (ticket == null) return [];

        return await _db.SupportMessages
            .Where(m => m.SupportTicketId == ticketId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new SupportMessageDto
            {
                Id = m.Id,
                SupportTicketId = m.SupportTicketId,
                SenderUserId = m.SenderUserId,
                SenderName = m.SenderName,
                SenderRole = m.SenderRole,
                Message = m.Message,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, SupportMessageDto? Data)> SendMessageAsync(
        int userId, string role, string userName, int ticketId, SendSupportMessageDto dto)
    {
        // Verify access
        var ticket = IsAdminRole(role)
            ? await _db.SupportTickets.FindAsync(ticketId)
            : await _db.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.UserId == userId);

        if (ticket == null)
            return (false, "Ticket not found or access denied.", null);

        if (ticket.Status == TicketStatus.Closed)
            return (false, "Cannot send messages on a closed ticket.", null);

        var message = new SupportMessage
        {
            SupportTicketId = ticketId,
            SenderUserId = userId,
            SenderName = userName,
            SenderRole = role,
            Message = dto.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.SupportMessages.Add(message);

        // Auto-transition: Open → InProgress when Admin replies
        if (IsAdminRole(role) && ticket.Status == TicketStatus.Open)
            ticket.Status = TicketStatus.InProgress;

        ticket.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return (true, "Message sent.", new SupportMessageDto
        {
            Id = message.Id,
            SupportTicketId = message.SupportTicketId,
            SenderUserId = message.SenderUserId,
            SenderName = message.SenderName,
            SenderRole = message.SenderRole,
            Message = message.Message,
            CreatedAt = message.CreatedAt
        });
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static bool IsAdminRole(string role) => role is "Admin" or "AdminStaff";

    private static SupportTicketDto MapToDto(SupportTicket t)
    {
        return new SupportTicketDto
        {
            Id = t.Id,
            UserId = t.UserId,
            UserFullName = t.User != null ? $"{t.User.FirstName} {t.User.LastName}".Trim() : "Unknown",
            UserRole = t.UserRole,
            Subject = t.Subject,
            Description = t.Description,
            Category = t.Category.ToString(),
            Priority = t.Priority.ToString(),
            Status = t.Status.ToString(),
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            ClosedAt = t.ClosedAt,
            ClosedByUserName = t.ClosedByUser != null ? $"{t.ClosedByUser.FirstName} {t.ClosedByUser.LastName}".Trim() : null,
            MessageCount = t.Messages?.Count ?? 0
        };
    }
}
