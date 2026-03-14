using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Admin;
using LegalConnect.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface IAdminStaffService
{
    Task<(bool Success, string Message, AdminStaffDto? Data)> CreateAsync(int creatorUserId, CreateAdminStaffDto dto);
    Task<List<AdminStaffDto>> GetAllAsync();
    Task<AdminStaffDto?> GetByIdAsync(int staffProfileId);
    Task<(bool Success, string Message)> UpdateRolesAsync(int staffProfileId, int updaterUserId, UpdateAdminStaffRolesDto dto);
    Task<(bool Success, string Message)> ToggleActiveAsync(int staffProfileId);
    Task<(bool Success, string Message)> ResetPasswordAsync(int staffProfileId, string newPassword);
    List<AdminStaffRoleInfoDto> GetAvailableRoles();
}

public class AdminStaffService : IAdminStaffService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminStaffService(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<(bool Success, string Message, AdminStaffDto? Data)> CreateAsync(int creatorUserId, CreateAdminStaffDto dto)
    {
        // Validate roles
        var validRoles = ParseRoles(dto.Roles);
        if (validRoles.Count == 0)
            return (false, "At least one valid role is required.", null);

        // Check email uniqueness
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            return (false, "An account with this email already exists.", null);

        // Create user
        var user = new ApplicationUser
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim(),
            UserName = dto.Email.Trim(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return (false, $"Failed to create account: {errors}", null);
        }

        await _userManager.AddToRoleAsync(user, "AdminStaff");

        // Create profile
        var profile = new AdminStaffProfile
        {
            UserId = user.Id,
            Department = dto.Department?.Trim(),
            CreatedByUserId = creatorUserId,
            IsActive = true
        };
        _db.AdminStaffProfiles.Add(profile);
        await _db.SaveChangesAsync();

        // Create role assignments
        foreach (var role in validRoles)
        {
            _db.AdminStaffRoleAssignments.Add(new AdminStaffRoleAssignment
            {
                AdminStaffProfileId = profile.Id,
                Role = role,
                AssignedByUserId = creatorUserId
            });
        }
        await _db.SaveChangesAsync();

        var result = await GetByIdAsync(profile.Id);
        return (true, "Admin staff created successfully.", result);
    }

    public async Task<List<AdminStaffDto>> GetAllAsync()
    {
        return await _db.AdminStaffProfiles
            .Include(a => a.User)
            .Include(a => a.CreatedBy)
            .Include(a => a.RoleAssignments)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => ToDto(a))
            .ToListAsync();
    }

    public async Task<AdminStaffDto?> GetByIdAsync(int staffProfileId)
    {
        var profile = await _db.AdminStaffProfiles
            .Include(a => a.User)
            .Include(a => a.CreatedBy)
            .Include(a => a.RoleAssignments)
            .FirstOrDefaultAsync(a => a.Id == staffProfileId);

        return profile == null ? null : ToDto(profile);
    }

    public async Task<(bool Success, string Message)> UpdateRolesAsync(int staffProfileId, int updaterUserId, UpdateAdminStaffRolesDto dto)
    {
        var profile = await _db.AdminStaffProfiles
            .Include(a => a.RoleAssignments)
            .FirstOrDefaultAsync(a => a.Id == staffProfileId);

        if (profile == null)
            return (false, "Admin staff not found.");

        var validRoles = ParseRoles(dto.Roles);
        if (validRoles.Count == 0)
            return (false, "At least one valid role is required.");

        // Remove existing assignments
        _db.AdminStaffRoleAssignments.RemoveRange(profile.RoleAssignments);

        // Add new assignments
        foreach (var role in validRoles)
        {
            _db.AdminStaffRoleAssignments.Add(new AdminStaffRoleAssignment
            {
                AdminStaffProfileId = profile.Id,
                Role = role,
                AssignedByUserId = updaterUserId
            });
        }

        profile.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return (true, "Roles updated successfully. Staff member must re-login for changes to take effect.");
    }

    public async Task<(bool Success, string Message)> ToggleActiveAsync(int staffProfileId)
    {
        var profile = await _db.AdminStaffProfiles
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == staffProfileId);

        if (profile == null)
            return (false, "Admin staff not found.");

        profile.IsActive = !profile.IsActive;
        profile.User.IsActive = profile.IsActive;
        profile.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var status = profile.IsActive ? "activated" : "deactivated";
        return (true, $"Admin staff {status} successfully.");
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(int staffProfileId, string newPassword)
    {
        var profile = await _db.AdminStaffProfiles
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == staffProfileId);

        if (profile == null)
            return (false, "Admin staff not found.");

        var removeResult = await _userManager.RemovePasswordAsync(profile.User);
        if (!removeResult.Succeeded)
            return (false, "Failed to reset password.");

        var addResult = await _userManager.AddPasswordAsync(profile.User, newPassword);
        if (!addResult.Succeeded)
        {
            var errors = string.Join("; ", addResult.Errors.Select(e => e.Description));
            return (false, $"Failed to set new password: {errors}");
        }

        return (true, "Password reset successfully.");
    }

    public List<AdminStaffRoleInfoDto> GetAvailableRoles()
    {
        return new List<AdminStaffRoleInfoDto>
        {
            new() { Name = "LawyerApprovalManager", Description = "Approve/reject lawyer registrations, view lawyer documents, activate/deactivate lawyers" },
            new() { Name = "UserManagementStaff", Description = "View all users, block/unblock users, reset user accounts, view profiles" },
            new() { Name = "SupportStaff", Description = "View support tickets, respond to user queries, communicate with clients and lawyers" },
            new() { Name = "DisputeRefundManager", Description = "Manage disputes, review/approve/reject refund requests, update dispute status" },
            new() { Name = "FinanceStaff", Description = "View invoices, track platform commission, monitor payments and revenue" }
        };
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static List<AdminStaffRole> ParseRoles(List<string> roleNames)
    {
        var result = new List<AdminStaffRole>();
        foreach (var name in roleNames)
        {
            if (Enum.TryParse<AdminStaffRole>(name.Trim(), out var role))
                result.Add(role);
        }
        return result.Distinct().ToList();
    }

    private static AdminStaffDto ToDto(AdminStaffProfile a) => new()
    {
        Id = a.Id,
        UserId = a.UserId,
        FullName = a.User.FullName,
        Email = a.User.Email ?? "",
        Phone = a.User.PhoneNumber,
        Department = a.Department,
        IsActive = a.IsActive,
        CreatedAt = a.CreatedAt,
        Roles = a.RoleAssignments.Select(r => r.Role.ToString()).ToList(),
        CreatedByName = a.CreatedBy.FullName
    };
}
