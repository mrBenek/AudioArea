using Microsoft.EntityFrameworkCore;
using Packt.Shared; // Category

namespace AudioArea.WebApi.Repositories;

public interface ICategoryRepository
{
    Task<Category?> CreateAsync(Category c);
    Task<IEnumerable<Category>> RetrieveAllAsync();
    Task<Category?> RetrieveAsync(int id);
    Task<Category?> UpdateAsync(int id, Category c);
    Task<bool?> DeleteAsync(int id);
}
