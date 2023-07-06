using AudioArea.WebApi.Repositories; // ICategoryRepository
using Microsoft.AspNetCore.Mvc; // [Route], [ApiController], ControllerBase
using Packt.Shared; // Category

namespace Northwind.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository repo;

    public CategoriesController(ICategoryRepository repo)
    {
        this.repo = repo;
    }

    // GET: api/categories
    // GET: api/categories/?name=[name]
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Category>))]
    public async Task<IEnumerable<Category>> GetCategories(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return await repo.RetrieveAllAsync();
        }
        else
        {
            return (await repo.RetrieveAllAsync())
              .Where(category => category.Name == name);
        }
    }

    // GET: api/categories/[id]
    [HttpGet("{id}", Name = nameof(GetCategory))]
    [ProducesResponseType(200, Type = typeof(Category))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCategory(int id)
    {
        Category? c = await repo.RetrieveAsync(id);
        if (c == null)
        {
            return NotFound();
        }
        return Ok(c);
    }

    // POST: api/categories
    // BODY: Category (JSON, XML)
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(Category))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] Category c)
    {
        if (c == null)
        {
            return BadRequest();
        }
        Category? addedCategory = await repo.CreateAsync(c);
        if (addedCategory == null)
        {
            return BadRequest("Repository failed to create category.");
        }
        else
        {
            return CreatedAtRoute(
              routeName: nameof(GetCategory),
              routeValues: new { id = addedCategory.Id },
              value: addedCategory);
        }
    }

    // PUT: api/categories/[id]
    // BODY: Category (JSON, XML)
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(
      int id, [FromBody] Category c)
    {
        if (c == null || c.Id != id)
        {
            return BadRequest();
        }
        Category? existing = await repo.RetrieveAsync(id);
        if (existing == null)
        {
            return NotFound();
        }
        await repo.UpdateAsync(id, c);
        return new NoContentResult();
    }

    // DELETE: api/categories/[id]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        Category? existing = await repo.RetrieveAsync(id);
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
              $"Category {id} was found but failed to delete.");
        }
    }
}
