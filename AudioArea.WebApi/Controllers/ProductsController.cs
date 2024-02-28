using Microsoft.AspNetCore.Mvc; // [Route], [ApiController], ControllerBase
using Packt.Shared; // Product
using AudioArea.WebApi.Repositories; // IProductRepository
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

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
    // GET: api/products/?companyName=[companyName]&companyId=[companyId]&categoryId=[categoryId]&sortedBy=[sortedBy]page=2&itemsperpage=2
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Product>))]
    public async Task<IEnumerable<Product>> GetProducts([FromQuery(Name = "companyName")] string? companyName, [FromQuery(Name = "companyId")] string? companyId,
        [FromQuery(Name = "categoryId")] string? categoryId, [FromQuery(Name = "sortedBy")] string? sortedBy,
        [FromQuery] Pagination @params)
    {
        IEnumerable<Product> products;
        List<int?>? companyIds = companyId?.Split(',').ToList().Select(s => Int32.TryParse(s, out int n) ? n : (int?)null).ToList();
        List<int?>? categoryIds = categoryId?.Split(',').ToList().Select(s => Int32.TryParse(s, out int n) ? n : (int?)null).ToList();

        if (!string.IsNullOrWhiteSpace(companyName))
        {
            products = await repo.RetrieveAsync(companyName);
        }
        else
        {
            products = await repo.RetrieveAsync(companyIds, categoryIds, sortedBy);
        }

        if (@params.PageSize == 0)
            return products;

        var paginationMetadata = new Pagination(products.Count(), @params.CurrentPage, @params.PageSize);
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

        return products.Skip((@params.CurrentPage - 1) * @params.PageSize)
                       .Take(@params.PageSize);
    }

    // GET: api/products/[id]
    [HttpGet("{id}", Name = nameof(GetProduct))]
    [ProducesResponseType(200, Type = typeof(Product))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProduct(int id)
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
              routeName: nameof(GetProduct),
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
