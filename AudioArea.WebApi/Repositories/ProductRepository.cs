using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; // EntityEntry<T>
using Packt.Shared; // Product
using System.Collections.Concurrent; // ConcurrentDictionary

namespace AudioArea.WebApi.Repositories;

public class ProductRepository : IProductRepository
{
    private static ConcurrentDictionary <int, Product>? productsCache;
    private readonly AudioContext db;

    public ProductRepository(AudioContext injectedContext)
    {
        db = injectedContext;

        if (productsCache is null)
        {
            productsCache = new ConcurrentDictionary<int, Product>(
              db.Products.Include(p => p.Company).ToDictionary(c => c.Id));
        }
    }

    public async Task<Product?> CreateAsync(Product c)
    {
        EntityEntry<Product> added = await db.Products.AddAsync(c);
        int affected = await db.SaveChangesAsync();
        if (affected == 1)
        {
            if (productsCache is null) return c;
            return productsCache.AddOrUpdate(c.Id, c, UpdateCache);
        }
        else
        {
            return null;
        }
    }

    public Task<IEnumerable<Product>> RetrieveAllAsync()
    {
        return Task.FromResult(productsCache is null
            ? Enumerable.Empty<Product>() : productsCache.Values);
    }

    public Task<IEnumerable<Product>> RetrieveAsync(string? company)
    {
        if (string.IsNullOrWhiteSpace(company))
            return RetrieveAllAsync();

        return Task.FromResult(productsCache is null
            ? Enumerable.Empty<Product>() : productsCache.Values.Where(product => product.Company?.Name == company));
    }

    public Task<Product?> RetrieveAsync(int id)
    {
        if (productsCache is null) return null!;
        productsCache.TryGetValue(id, out Product? c);
        return Task.FromResult(c);
    }

    private Product UpdateCache(int id, Product c)
    {
        Product? old;
        if (productsCache is not null)
        {
            if (productsCache.TryGetValue(id, out old))
            {
                if (productsCache.TryUpdate(id, c, old))
                {
                    return c;
                }
            }
        }
        return null!;
    }

    public async Task<Product?> UpdateAsync(int id, Product c)
    {
        db.Products.Update(c);
        int affected = await db.SaveChangesAsync();
        if (affected == 1)
        {
            return UpdateCache(id, c);
        }
        return null;
    }

    public async Task<bool?> DeleteAsync(int id)
    {
        Product? c = db.Products.Find(id);
        if (c is null) return null;
        db.Products.Remove(c);
        int affected = await db.SaveChangesAsync();
        if (affected == 1)
        {
            if (productsCache is null) return null;
            return productsCache.TryRemove(id, out _);
        }
        else
        {
            return null;
        }
    }
}
