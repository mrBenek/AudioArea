﻿using Microsoft.EntityFrameworkCore;
using Packt.Shared; // Product

namespace AudioArea.WebApi.Repositories;

public interface IProductRepository
{
    Task<Product?> CreateAsync(Product c);
    Task<IEnumerable<Product>> RetrieveAllAsync();
    Task<Product?> RetrieveAsync(int id);
    Task<IEnumerable<Product>> RetrieveAsync(string? company);
    Task<IEnumerable<Product>> RetrieveAsync(List<int?>? companyId, List<int?>? categoryId, string? sortedBy);
    Task<Product?> UpdateAsync(int id, Product c);
    Task<bool?> DeleteAsync(int id);
}
