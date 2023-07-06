using Microsoft.AspNetCore.Mvc; // [Route], [ApiController], ControllerBase
using Packt.Shared; // Product
using AudioArea.WebApi.Repositories; // IProductRepository

namespace Northwind.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository repo;

    public ProductsController(IProductRepository repo)
    {
        this.repo = repo;
    }

    // GET: api/products
    // GET: api/products/?company=[company]
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Product>))]
    public async Task<IEnumerable<Product>> GetProducts(string? company)
    {
        if (string.IsNullOrWhiteSpace(company))
        {
            return await repo.RetrieveAllAsync();
        }
        else
        {
            return (await repo.RetrieveAllAsync())
              .Where(product => product.Company?.Name == company);
        }
    }

    // GET: api/products/[id]
    [HttpGet("{id}", Name = nameof(GetProdut))]
    [ProducesResponseType(200, Type = typeof(Product))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProdut(int id)
    {
        Product? c = await repo.RetrieveAsync(id);
        if (c == null)
        {
            return NotFound();
        }
        return Ok(c);
    }

    // POST: api/products
    // BODY: Product (JSON, XML)
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(Product))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] Product c)
    {
        if (c == null)
        {
            return BadRequest();
        }
        Product? addedProduct = await repo.CreateAsync(c);
        if (addedProduct == null)
        {
            return BadRequest("Repository failed to create product.");
        }
        else
        {
            return CreatedAtRoute(
              routeName: nameof(GetProdut),
              routeValues: new { id = addedProduct.Id },
              value: addedProduct);
        }
    }

    // PUT: api/products/[id]
    // BODY: Product (JSON, XML)
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(
      int id, [FromBody] Product c)
    {
        if (c == null || c.Id != id)
        {
            return BadRequest();
        }
        Product? existing = await repo.RetrieveAsync(id);
        if (existing == null)
        {
            return NotFound();
        }
        await repo.UpdateAsync(id, c);
        return new NoContentResult();
    }

    // DELETE: api/products/[id]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        Product? existing = await repo.RetrieveAsync(id);
        if (existing == null)
        {
            return NotFound();
        }
        bool? deleted = await repo.DeleteAsync(id);
        if (deleted.HasValue && deleted.Value)
        {
            return new NoContentResult();
        }
        else
        {
            return BadRequest(
              $"Product {id} was found but failed to delete.");
        }
    }
}
