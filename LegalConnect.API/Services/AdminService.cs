using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Admin;
using LegalConnect.API.DTOs.Lawyer;
using LegalConnect.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface IAdminService
{
    Task<List<PendingLawyerDto>> GetPendingLawyersAsync();
    Task<(bool Success, string Message)> ApproveLawyerAsync(int lawyerProfileId);
    Task<(bool Success, string Message)> RejectLawyerAsync(int lawyerProfileId, string reason);
    Task<CommissionSettingDto> GetCommissionSettingAsync();
    Task<(bool Success, string Message)> SetCommissionAsync(SetCommissionDto dto, string adminName);
    Task<List<CategoryDto>> GetAllCategoriesAsync();
    Task<(bool Success, string Message)> CreateCategoryAsync(CreateCategoryDto dto);
    Task<(bool Success, string Message)> UpdateCategoryAsync(UpdateCategoryDto dto);
    Task<(bool Success, string Message)> DeleteCategoryAsync(int id);
    Task<RevenueStatsDto> GetRevenueStatsAsync();
    Task<List<AdminLawyerDto>> GetAllLawyersAsync();
    Task<List<AdminClientDto>> GetAllClientsAsync();
    Task<(bool Success, string Message)> ToggleUserActiveAsync(int userId);
}

public class AdminService : IAdminService
{
    private readonly AppDbContext _db;

    public AdminService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PendingLawyerDto>> GetPendingLawyersAsync()
    {
        return await _db.LawyerProfiles
            .Include(l => l.User)
            .Include(l => l.Category)
            .Where(l => !l.IsVerified && l.RejectionReason == null)
            .OrderBy(l => l.CreatedAt)
            .Select(l => new PendingLawyerDto
            {
                Id = l.Id,
                FullName = l.User.FirstName + " " + l.User.LastName,
                Email = l.User.Email ?? "",
                City = l.City,
                BarCouncilNumber = l.BarCouncilNumber,
                CategoryName = l.Category.Name,
                RegisteredAt = l.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> ApproveLawyerAsync(int lawyerProfileId)
    {
        var lawyer = await _db.LawyerProfiles.FindAsync(lawyerProfileId);
        if (lawyer == null) return (false, "Lawyer not found.");
        if (lawyer.IsVerified) return (false, "Lawyer is already verified.");

        lawyer.IsVerified = true;
        lawyer.RejectionReason = null;
        await _db.SaveChangesAsync();
        return (true, "Lawyer approved successfully.");
    }

    public async Task<(bool Success, string Message)> RejectLawyerAsync(int lawyerProfileId, string reason)
    {
        var lawyer = await _db.LawyerProfiles.FindAsync(lawyerProfileId);
        if (lawyer == null) return (false, "Lawyer not found.");

        lawyer.IsVerified = false;
        lawyer.RejectionReason = reason;
        await _db.SaveChangesAsync();
        return (true, "Lawyer rejected.");
    }

    public async Task<CommissionSettingDto> GetCommissionSettingAsync()
    {
        var setting = await _db.CommissionSettings.FirstOrDefaultAsync()
            ?? new CommissionSetting { DefaultCommissionPercentage = 10, LastUpdatedAt = DateTime.UtcNow, UpdatedBy = "System" };

        return new CommissionSettingDto
        {
            DefaultCommissionPercentage = setting.DefaultCommissionPercentage,
            LastUpdatedAt = setting.LastUpdatedAt,
            UpdatedBy = setting.UpdatedBy
        };
    }

    public async Task<(bool Success, string Message)> SetCommissionAsync(SetCommissionDto dto, string adminName)
    {
        var setting = await _db.CommissionSettings.FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new CommissionSetting();
            _db.CommissionSettings.Add(setting);
        }

        setting.DefaultCommissionPercentage = dto.CommissionPercentage;
        setting.LastUpdatedAt = DateTime.UtcNow;
        setting.UpdatedBy = adminName;

        await _db.SaveChangesAsync();
        return (true, "Commission updated successfully.");
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        return await _db.Categories
            .Include(c => c.Lawyers)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                IconClass = c.IconClass,
                Description = c.Description,
                LawyerCount = c.Lawyers.Count(l => l.IsVerified)
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var exists = await _db.Categories.AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.IsActive);
        if (exists) return (false, "A category with this name already exists.");

        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            IconClass = dto.IconClass ?? "bi bi-briefcase"
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return (true, "Category created successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateCategoryAsync(UpdateCategoryDto dto)
    {
        var category = await _db.Categories.FindAsync(dto.Id);
        if (category == null) return (false, "Category not found.");

        var nameExists = await _db.Categories.AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.Id != dto.Id && c.IsActive);
        if (nameExists) return (false, "A category with this name already exists.");

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.IconClass = dto.IconClass ?? "bi bi-briefcase";

        await _db.SaveChangesAsync();
        return (true, "Category updated.");
    }

    public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id)
    {
        var category = await _db.Categories.Include(c => c.Lawyers).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return (false, "Category not found.");

        if (category.Lawyers.Any())
            return (false, "Cannot delete a category that has lawyers assigned to it.");

        category.IsActive = false;
        await _db.SaveChangesAsync();
        return (true, "Category deleted.");
    }

    public async Task<RevenueStatsDto> GetRevenueStatsAsync()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var totalRevenue = await _db.Appointments
            .Where(a => a.Status == AppointmentStatus.Completed)
            .SumAsync(a => (decimal?)a.TotalAmount) ?? 0;

        var monthlyRevenue = await _db.Appointments
            .Where(a => a.Status == AppointmentStatus.Completed && a.AppointmentDate >= startOfMonth)
            .SumAsync(a => (decimal?)a.TotalAmount) ?? 0;

        var platformCommission = await _db.Appointments
            .Where(a => a.Status == AppointmentStatus.Completed)
            .SumAsync(a => (decimal?)a.PlatformCommission) ?? 0;

        var totalAppointments = await _db.Appointments.CountAsync();
        var totalClients = await _db.ClientProfiles.CountAsync();
        var totalLawyers = await _db.LawyerProfiles.CountAsync(l => l.IsVerified);
        var pendingApprovals = await _db.LawyerProfiles.CountAsync(l => !l.IsVerified && l.RejectionReason == null);

        // Monthly breakdown for last 6 months
        var monthlyBreakdown = new List<MonthlyRevenueDto>();
        for (int i = 5; i >= 0; i--)
        {
            var monthDate = now.AddMonths(-i);
            var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var revenue = await _db.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed
                    && a.AppointmentDate >= monthStart
                    && a.AppointmentDate < monthEnd)
                .SumAsync(a => (decimal?)a.TotalAmount) ?? 0;

            var count = await _db.Appointments
                .CountAsync(a => a.Status == AppointmentStatus.Completed
                    && a.AppointmentDate >= monthStart
                    && a.AppointmentDate < monthEnd);

            monthlyBreakdown.Add(new MonthlyRevenueDto
            {
                Month = monthDate.ToString("MMM yyyy"),
                Revenue = revenue,
                AppointmentCount = count
            });
        }

        return new RevenueStatsDto
        {
            TotalRevenue = totalRevenue,
            MonthlyRevenue = monthlyRevenue,
            PlatformCommission = platformCommission,
            TotalAppointments = totalAppointments,
            TotalClients = totalClients,
            TotalLawyers = totalLawyers,
            PendingApprovals = pendingApprovals,
            MonthlyBreakdown = monthlyBreakdown
        };
    }

    public async Task<List<AdminLawyerDto>> GetAllLawyersAsync()
    {
        return await _db.LawyerProfiles
            .Include(l => l.User)
            .Include(l => l.Category)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new AdminLawyerDto
            {
                Id = l.Id,
                UserId = l.UserId,
                FullName = l.User.FirstName + " " + l.User.LastName,
                Email = l.User.Email ?? string.Empty,
                Phone = l.User.PhoneNumber,
                City = l.City,
                CategoryName = l.Category.Name,
                BarCouncilNumber = l.BarCouncilNumber,
                IsVerified = l.IsVerified,
                IsActive = l.User.IsActive,
                RegisteredAt = l.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<AdminClientDto>> GetAllClientsAsync()
    {
        return await _db.ClientProfiles
            .Include(c => c.User)
            .Include(c => c.Cases)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new AdminClientDto
            {
                Id = c.Id,
                UserId = c.UserId,
                FullName = c.User.FirstName + " " + c.User.LastName,
                Email = c.User.Email ?? string.Empty,
                Phone = c.User.PhoneNumber,
                City = c.City,
                IsActive = c.User.IsActive,
                RegisteredAt = c.CreatedAt,
                TotalCases = c.Cases.Count
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> ToggleUserActiveAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return (false, "User not found.");
        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();
        return (true, user.IsActive ? "User activated successfully." : "User deactivated successfully.");
    }
}
