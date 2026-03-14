using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Staff;
using LegalConnect.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface IStaffService
{
    // ── Lawyer: Staff management ──────────────────────────────────────────
    Task<(bool Success, string Message, StaffProfileDto? Data)> CreateStaffAsync(int lawyerUserId, CreateStaffDto dto);
    Task<List<StaffProfileDto>> GetMyStaffAsync(int lawyerUserId);
    Task<StaffProfileDto?> GetStaffByIdAsync(int lawyerUserId, int staffProfileId);
    Task<(bool Success, string Message)> ToggleStaffActiveAsync(int lawyerUserId, int staffProfileId);

    // ── Lawyer: Case assignment ───────────────────────────────────────────
    Task<(bool Success, string Message)> AssignStaffToCaseAsync(int lawyerUserId, int caseId, AssignStaffDto dto);
    Task<(bool Success, string Message)> RemoveStaffFromCaseAsync(int lawyerUserId, int caseId, int staffProfileId);
    Task<List<CaseStaffDto>> GetCaseStaffAsync(int caseId);
    Task<StaffCasePermissionsDto?> GetMyPermissionsAsync(int staffUserId, int caseId);

    // ── Lawyer: Task management ───────────────────────────────────────────
    Task<List<StaffTaskDto>> GetAllTasksAsync(int lawyerUserId);
    Task<(bool Success, string Message, StaffTaskDto? Data)> CreateTaskAsync(int lawyerUserId, CreateStaffTaskDto dto);
    Task<(bool Success, string Message, StaffTaskDto? Data)> UpdateTaskAsync(int lawyerUserId, int taskId, UpdateStaffTaskDto dto);
    Task<(bool Success, string Message)> DeleteTaskAsync(int lawyerUserId, int taskId);

    // ── Staff: Task access ────────────────────────────────────────────────
    Task<List<StaffTaskDto>> GetMyTasksAsync(int staffUserId);
    Task<(bool Success, string Message, StaffTaskDto? Data)> UpdateMyTaskStatusAsync(int staffUserId, int taskId, UpdateTaskStatusDto dto);
}

public class StaffService : IStaffService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public StaffService(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private async Task<LawyerProfile?> GetLawyerProfileAsync(int lawyerUserId)
        => await _db.LawyerProfiles.FirstOrDefaultAsync(l => l.UserId == lawyerUserId);

    private async Task<StaffProfile?> GetStaffProfileAsync(int staffUserId)
        => await _db.StaffProfiles.FirstOrDefaultAsync(s => s.UserId == staffUserId);

    private static StaffProfileDto MapStaff(StaffProfile s) => new()
    {
        Id             = s.Id,
        UserId         = s.UserId,
        LawyerProfileId = s.LawyerProfileId,
        FullName       = s.User.FirstName + " " + s.User.LastName,
        Email          = s.User.Email ?? string.Empty,
        StaffRole      = s.StaffRole,
        IsActive       = s.IsActive,
        CreatedAt      = s.CreatedAt
    };

    private static StaffTaskDto MapTask(StaffTask t) => new()
    {
        Id                         = t.Id,
        LawyerProfileId            = t.LawyerProfileId,
        AssignedToStaffProfileId   = t.AssignedToStaffProfileId,
        AssignedToName             = t.AssignedTo == null ? null
                                     : t.AssignedTo.User.FirstName + " " + t.AssignedTo.User.LastName,
        CaseId    = t.CaseId,
        CaseTitle = t.Case?.CaseTitle,
        Title       = t.Title,
        Description = t.Description,
        StaffNote   = t.StaffNote,
        Priority    = t.Priority.ToString(),
        Status      = t.Status.ToString(),
        DueDate     = t.DueDate,
        CompletedAt = t.CompletedAt,
        CreatedAt   = t.CreatedAt,
        UpdatedAt   = t.UpdatedAt
    };

    // ── Staff Management ──────────────────────────────────────────────────

    public async Task<(bool Success, string Message, StaffProfileDto? Data)> CreateStaffAsync(
        int lawyerUserId, CreateStaffDto dto)
    {
        var lawyer = await GetLawyerProfileAsync(lawyerUserId);
        if (lawyer == null) return (false, "Lawyer profile not found.", null);

        if (!StaffRoleTypeDto.All.Contains(dto.StaffRole))
            return (false, $"Invalid staff role. Valid roles: {string.Join(", ", StaffRoleTypeDto.All)}", null);

        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null) return (false, "Email is already registered.", null);

        var user = new ApplicationUser
        {
            FirstName  = dto.FirstName.Trim(),
            LastName   = dto.LastName.Trim(),
            Email      = dto.Email.Trim().ToLower(),
            UserName   = dto.Email.Trim().ToLower(),
            IsActive   = true,
            CreatedAt  = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return (false, $"Failed to create user: {errors}", null);
        }

        await _userManager.AddToRoleAsync(user, "Staff");

        var staffProfile = new StaffProfile
        {
            UserId          = user.Id,
            LawyerProfileId = lawyer.Id,
            StaffRole       = dto.StaffRole,
            IsActive        = true,
            CreatedAt       = DateTime.UtcNow,
            UpdatedAt       = DateTime.UtcNow
        };
        _db.StaffProfiles.Add(staffProfile);
        await _db.SaveChangesAsync();

        // Reload with navigation
        var created = await _db.StaffProfiles.Include(s => s.User)
            .FirstAsync(s => s.Id == staffProfile.Id);
        return (true, "Staff member created successfully.", MapStaff(created));
    }

    public async Task<List<StaffProfileDto>> GetMyStaffAsync(int lawyerUserId)
    {
        var lawyer = await GetLawyerProfileAsync(lawyerUserId);
        if (lawyer == null) return [];

        return await _db.StaffProfiles
            .Where(s => s.LawyerProfileId == lawyer.Id)
            .Include(s => s.User)
            .OrderBy(s => s.User.FirstName).ThenBy(s => s.User.LastName)
            .Select(s => MapStaff(s))
            .ToListAsync();
    }

    public async Task<StaffProfileDto?> GetStaffByIdAsync(int lawyerUserId, int staffProfileId)
    {
        var lawyer = await GetLawyerProfileAsync(lawyerUserId);
        if (lawyer == null) return null;

        var staff = await _db.StaffProfiles
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == staffProfileId && s.LawyerProfileId == lawyer.Id);

        return staff == null ? null : MapStaff(staff);
    }

    public async Task<(bool Success, string Message)> ToggleStaffActiveAsync(int lawyerUserId, int staffProfileId)
    {
        var lawyer = await GetLawyerProfileAsync(lawyerUserId);
        if (lawyer == null) return (false, "Lawyer profile not found.");

        var staff = await _db.StaffProfiles.Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == staffProfileId && s.LawyerProfileId == lawyer.Id);
        if (staff == null) return (false, "Staff member not found.");

        staff.IsActive  = !staff.IsActive;
        staff.UpdatedAt = DateTime.UtcNow;
        staff.User.IsActive = staff.IsActive;
        await _db.SaveChangesAsync();

        return (true, staff.IsActive ? "Staff member activated." : "Staff member deactivated.");
    }

    // ── Case Assignment ───────────────────────────────────────────────────

    public async Task<(bool Success, string Message)> AssignStaffToCaseAsync(
        int lawyerUserId, int caseId, AssignStaffDto dto)
    {
        var lawyer = await GetLawyerProfileAsync(lawyerUserId);
        if (lawyer == null) return (false, "Lawyer profile not found.");

        // Verify case belongs to this lawyer
        var caseExists = await _db.Cases.AnyAsync(c => c.Id == caseId &&
            (c.LawyerProfileId == lawyer.Id || c.CaseLawyers.Any(cl => cl.LawyerProfileId == lawyer.Id && cl.IsActive)));
        if (!caseExists) return (false, "Case not found or access denied.");

        // Verify staff belongs to this lawyer's firm
        var staff = await _db.StaffProfiles
            .FirstOrDefaultAsync(s => s.Id == dto.StaffProfileId && s.LawyerProfileId == lawyer.Id && s.IsActive);
        if (staff == null) return (false, "Staff member not found or not active.");

        // Upsert: re-activate if previously removed
        var existing = await _db.CaseStaffs.IgnoreQueryFilters()
            .FirstOrDefaultAsync(cs => cs.CaseId == caseId && cs.StaffProfileId == dto.StaffProfileId);

        if (existing != null)
        {
            if (existing.IsActive) return (false, "Staff is already assigned to this case.");
            existing.IsActive        = true;
            existing.AddedByUserId   = lawyerUserId;
            existing.AddedAt         = DateTime.UtcNow;
            existing.CanAddActivity  = dto.CanAddActivity;
            existing.CanUploadDocument = dto.CanUploadDocument;
        }
        else
        {
            _db.CaseStaffs.Add(new CaseStaff
            {
                CaseId             = caseId,
                StaffProfileId     = dto.StaffProfileId,
                AddedByUserId      = lawyerUserId,
                AddedAt            = DateTime.UtcNow,
                IsActive           = true,
                CanAddActivity     = dto.CanAddActivity,
                CanUploadDocument  = dto.CanUploadDocument
            });
        }

        await _db.SaveChangesAsync();
        return (true, "Staff assigned to case.");
    }

    public async Task<(bool Success, string Message)> RemoveStaffFromCaseAsync(
        int lawyerUserId, int caseId, int staffProfileId)
    {
        var lawyer = await GetLawyerProfileAsync(lawyerUserId);
        if (lawyer == null) return (false, "Lawyer profile not found.");

        var caseStaff = await _db.CaseStaffs
            .FirstOrDefaultAsync(cs => cs.CaseId == caseId && cs.StaffProfileId == staffProfileId);
        if (caseStaff == null || !caseStaff.IsActive) return (false, "Assignment not found.");

        caseStaff.IsActive = false;
        await _db.SaveChangesAsync();
        return (true, "Staff removed from case.");
    }

    public async Task<List<CaseStaffDto>> GetCaseStaffAsync(int caseId)
    {
        return await _db.CaseStaffs
            .Where(cs => cs.CaseId == caseId && cs.IsActive)
            .Include(cs => cs.StaffProfile).ThenInclude(s => s.User)
            .OrderBy(cs => cs.AddedAt)
            .Select(cs => new CaseStaffDto
            {
                Id                = cs.Id,
                StaffProfileId    = cs.StaffProfileId,
                FullName          = cs.StaffProfile.User.FirstName + " " + cs.StaffProfile.User.LastName,
                Email             = cs.StaffProfile.User.Email ?? string.Empty,
                StaffRole         = cs.StaffProfile.StaffRole,
                AddedAt           = cs.AddedAt,
                IsActive          = cs.IsActive,
                CanAddActivity    = cs.CanAddActivity,
                CanUploadDocument = cs.CanUploadDocument
            }).ToListAsync();
    }

    public async Task<StaffCasePermissionsDto?> GetMyPermissionsAsync(int staffUserId, int caseId)
    {
        var staffProfile = await GetStaffProfileAsync(staffUserId);
        if (staffProfile == null) return null;

        var caseStaff = await _db.CaseStaffs
            .FirstOrDefaultAsync(cs => cs.CaseId == caseId && cs.StaffProfileId == staffProfile.Id && cs.IsActive);
        if (caseStaff == null) return null;

        return new StaffCasePermissionsDto
        {
            CanAddActivity    = caseStaff.CanAddActivity,
            CanUploadDocument = caseStaff.CanUploadDocument
        };
    }

    // ── Task Management (Lawyer) ──────────────────────────────────────────

    public async Task<List<StaffTaskDto>> GetAllTasksAsync(int lawyerUserId)
    {
        var lawyer = await GetLawyerProfileAsync(lawyerUserId);
        if (lawyer == null) return [];

        return await _db.StaffTasks
            .Where(t => t.LawyerProfileId == lawyer.Id)
            .Include(t => t.AssignedTo).ThenInclude(s => s!.User)
            .Include(t => t.Case)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => MapTask(t))
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, StaffTaskDto? Data)> CreateTaskAsync(
        int lawyerUserId, CreateStaffTaskDto dto)
    {
        var lawyer = await GetLawyerProfileAsync(lawyerUserId);
        if (lawyer == null) return (false, "Lawyer profile not found.", null);

        if (!Enum.TryParse<TaskPriority>(dto.Priority, out var priority))
            priority = TaskPriority.Medium;

        // Validate assigned staff belongs to this lawyer
        if (dto.AssignedToStaffProfileId.HasValue)
        {
            var staffExists = await _db.StaffProfiles.AnyAsync(s =>
                s.Id == dto.AssignedToStaffProfileId && s.LawyerProfileId == lawyer.Id && s.IsActive);
            if (!staffExists) return (false, "Staff member not found or not active.", null);
        }

        // Validate case belongs to this lawyer
        if (dto.CaseId.HasValue)
        {
            var caseExists = await _db.Cases.AnyAsync(c => c.Id == dto.CaseId &&
                (c.LawyerProfileId == lawyer.Id || c.CaseLawyers.Any(cl => cl.LawyerProfileId == lawyer.Id)));
            if (!caseExists) return (false, "Case not found.", null);
        }

        var task = new StaffTask
        {
            LawyerProfileId          = lawyer.Id,
            AssignedToStaffProfileId = dto.AssignedToStaffProfileId,
            CaseId      = dto.CaseId,
            Title       = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Priority    = priority,
            Status      = StaffTaskStatus.Pending,
            DueDate     = dto.DueDate,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        };
        _db.StaffTasks.Add(task);
        await _db.SaveChangesAsync();

        var created = await LoadTaskAsync(task.Id);
        return (true, "Task created.", MapTask(created!));
    }

    public async Task<(bool Success, string Message, StaffTaskDto? Data)> UpdateTaskAsync(
        int lawyerUserId, int taskId, UpdateStaffTaskDto dto)
    {
        var lawyer = await GetLawyerProfileAsync(lawyerUserId);
        if (lawyer == null) return (false, "Lawyer profile not found.", null);

        var task = await LoadTaskAsync(taskId);
        if (task == null || task.LawyerProfileId != lawyer.Id) return (false, "Task not found.", null);

        if (dto.Title is not null) task.Title = dto.Title.Trim();
        if (dto.Description is not null) task.Description = dto.Description.Trim();
        if (dto.DueDate is not null) task.DueDate = dto.DueDate;
        if (dto.CaseId is not null) task.CaseId = dto.CaseId;
        if (dto.AssignedToStaffProfileId is not null)
            task.AssignedToStaffProfileId = dto.AssignedToStaffProfileId;
        if (dto.Priority is not null && Enum.TryParse<TaskPriority>(dto.Priority, out var p))
            task.Priority = p;
        if (dto.Status is not null && Enum.TryParse<StaffTaskStatus>(dto.Status, out var s))
        {
            task.Status = s;
            if (s == StaffTaskStatus.Completed && task.CompletedAt == null)
                task.CompletedAt = DateTime.UtcNow;
        }

        task.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var updated = await LoadTaskAsync(task.Id);
        return (true, "Task updated.", MapTask(updated!));
    }

    public async Task<(bool Success, string Message)> DeleteTaskAsync(int lawyerUserId, int taskId)
    {
        var lawyer = await GetLawyerProfileAsync(lawyerUserId);
        if (lawyer == null) return (false, "Lawyer profile not found.");

        var task = await _db.StaffTasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.LawyerProfileId == lawyer.Id);
        if (task == null) return (false, "Task not found.");

        task.IsDeleted  = true;
        task.UpdatedAt  = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, "Task deleted.");
    }

    // ── Staff: Task Access ────────────────────────────────────────────────

    public async Task<List<StaffTaskDto>> GetMyTasksAsync(int staffUserId)
    {
        var staff = await GetStaffProfileAsync(staffUserId);
        if (staff == null) return [];

        return await _db.StaffTasks
            .Where(t => t.AssignedToStaffProfileId == staff.Id)
            .Include(t => t.AssignedTo).ThenInclude(s => s!.User)
            .Include(t => t.Case)
            .OrderBy(t => t.DueDate.HasValue ? 0 : 1).ThenBy(t => t.DueDate)
            .Select(t => MapTask(t))
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, StaffTaskDto? Data)> UpdateMyTaskStatusAsync(
        int staffUserId, int taskId, UpdateTaskStatusDto dto)
    {
        var staff = await GetStaffProfileAsync(staffUserId);
        if (staff == null) return (false, "Staff profile not found.", null);

        var task = await LoadTaskAsync(taskId);
        if (task == null || task.AssignedToStaffProfileId != staff.Id)
            return (false, "Task not found.", null);

        if (!Enum.TryParse<StaffTaskStatus>(dto.Status, out var newStatus))
            return (false, "Invalid status value.", null);

        task.Status    = newStatus;
        task.StaffNote = dto.StaffNote?.Trim();
        task.UpdatedAt = DateTime.UtcNow;

        if (newStatus == StaffTaskStatus.Completed && task.CompletedAt == null)
            task.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var updated = await LoadTaskAsync(task.Id);
        return (true, "Task updated.", MapTask(updated!));
    }

    // ── Private ───────────────────────────────────────────────────────────

    private async Task<StaffTask?> LoadTaskAsync(int taskId) =>
        await _db.StaffTasks
            .Include(t => t.AssignedTo).ThenInclude(s => s!.User)
            .Include(t => t.Case)
            .FirstOrDefaultAsync(t => t.Id == taskId);
}
