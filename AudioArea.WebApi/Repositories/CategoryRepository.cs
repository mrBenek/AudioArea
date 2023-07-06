using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; // EntityEntry<T>
using Packt.Shared; // Category
using System.Collections.Concurrent; // ConcurrentDictionary

namespace AudioArea.WebApi.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private static ConcurrentDictionary <int, Category>? categoriesCache;
    private readonly AudioContext db;

    public CategoryRepository(AudioContext injectedContext)
    {
        db = injectedContext;

        if (categoriesCache is null)
        {
            categoriesCache = new ConcurrentDictionary<int, Category>(
              db.Categories.ToDictionary(c => c.Id));
        }
    }

    public async Task<Category?> CreateAsync(Category c)
    {
        using var transaction = db.Database.BeginTransaction();
        await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Categories ON"); //allow insert primary key to db
        db.SaveChanges(true);
        EntityEntry<Category> added = await db.Categories.AddAsync(c);
        int affected = await db.SaveChangesAsync();
        await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Categories OFF");
        transaction.Commit();

        if (affected == 1)
        {
            if (categoriesCache is null) return c;
            return categoriesCache.AddOrUpdate(c.Id, c, UpdateCache);
        }
        else
        {
            return null;
        }
    }

    public Task<IEnumerable<Category>> RetrieveAllAsync()
    {
        return Task.FromResult(categoriesCache is null
            ? Enumerable.Empty<Category>() : categoriesCache.Values);
    }

    public Task<Category?> RetrieveAsync(int id)
    {
        if (categoriesCache is null) return null!;
        categoriesCache.TryGetValue(id, out Category? c);
        return Task.FromResult(c);
    }

    private Category UpdateCache(int id, Category c)
    {
        Category? old;
        if (categoriesCache is not null)
        {
            if (categoriesCache.TryGetValue(id, out old))
            {
                if (categoriesCache.TryUpdate(id, c, old))
                {
                    return c;
                }
            }
        }
        return null!;
    }

    public async Task<Category?> UpdateAsync(int id, Category c)
    {
        using var transaction = db.Database.BeginTransaction();
        await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Categories ON"); //allow insert primary key to db
        db.Categories.Update(c);
        int affected = await db.SaveChangesAsync();
        db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Categories OFF");
        await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Categories OFF");
        transaction.Commit();

        if (affected == 1)
        {
            return UpdateCache(id, c);
        }
        return null;
    }

    public async Task<bool?> DeleteAsync(int id)
    {
        Category? c = db.Categories.Find(id);
        if (c is null) return null;
        db.Categories.Remove(c);
        int affected = await db.SaveChangesAsync();
        if (affected == 1)
        {
            if (categoriesCache is null) return null;
            return categoriesCache.TryRemove(id, out _);
        }
        else
        {
            return null;
        }
    }
}
