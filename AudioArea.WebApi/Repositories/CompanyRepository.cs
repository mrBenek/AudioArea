using Microsoft.EntityFrameworkCore.ChangeTracking; // EntityEntry<T>
using Packt.Shared; // Company
using System.Collections.Concurrent; // ConcurrentDictionary

namespace AudioArea.WebApi.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private static ConcurrentDictionary <int, Company>? companiesCache;
    private readonly AudioContext db;

    public CompanyRepository(AudioContext injectedContext)
    {
        db = injectedContext;

        if (companiesCache is null)
        {
            companiesCache = new ConcurrentDictionary<int, Company>(
              db.Companies.ToDictionary(c => c.Id));
        }
    }

    public async Task<Company?> CreateAsync(Company c)
    {
        EntityEntry<Company> added = await db.Companies.AddAsync(c);
        int affected = await db.SaveChangesAsync();
        if (affected == 1)
        {
            if (companiesCache is null) return c;
            return companiesCache.AddOrUpdate(c.Id, c, UpdateCache);
        }
        else
        {
            return null;
        }
    }

    public Task<IEnumerable<Company>> RetrieveAllAsync()
    {
        return Task.FromResult(companiesCache is null
            ? Enumerable.Empty<Company>() : companiesCache.Values);
    }

    public Task<Company?> RetrieveAsync(int id)
    {
        if (companiesCache is null) return null!;
        companiesCache.TryGetValue(id, out Company? c);
        return Task.FromResult(c);
    }

    private Company UpdateCache(int id, Company c)
    {
        Company? old;
        if (companiesCache is not null)
        {
            if (companiesCache.TryGetValue(id, out old))
            {
                if (companiesCache.TryUpdate(id, c, old))
                {
                    return c;
                }
            }
        }
        return null!;
    }

    public async Task<Company?> UpdateAsync(int id, Company c)
    {
        db.Companies.Update(c);
        int affected = await db.SaveChangesAsync();
        if (affected == 1)
        {
            return UpdateCache(id, c);
        }
        return null;
    }

    public async Task<bool?> DeleteAsync(int id)
    {
        Company? c = db.Companies.Find(id);
        if (c is null) return null;
        db.Companies.Remove(c);
        int affected = await db.SaveChangesAsync();
        if (affected == 1)
        {
            if (companiesCache is null) return null;
            return companiesCache.TryRemove(id, out _);
        }
        else
        {
            return null;
        }
    }
}
