using Microsoft.EntityFrameworkCore;
using Packt.Shared; // Company

namespace AudioArea.WebApi.Repositories;

public interface ICompanyRepository
{
    Task<Company?> CreateAsync(Company c);
    Task<IEnumerable<Company>> RetrieveAllAsync();
    Task<Company?> RetrieveAsync(int id);
    Task<Company?> UpdateAsync(int id, Company c);
    Task<bool?> DeleteAsync(int id);
}
