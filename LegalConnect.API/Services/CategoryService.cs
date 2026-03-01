using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Lawyer;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetCategoriesAsync();
}

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
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
}
