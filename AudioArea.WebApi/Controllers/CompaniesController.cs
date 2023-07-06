using AudioArea.WebApi.Repositories; // ICompanyRepository
using Microsoft.AspNetCore.Mvc; // [Route], [ApiController], ControllerBase
using Packt.Shared; // Company

namespace Northwind.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyRepository repo;

    public CompaniesController(ICompanyRepository repo)
    {
        this.repo = repo;
    }

    // GET: api/companies
    // GET: api/companies/?name=[name]
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Company>))]
    public async Task<IEnumerable<Company>> GetCompanies(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return await repo.RetrieveAllAsync();
        }
        else
        {
            return (await repo.RetrieveAllAsync())
              .Where(company => company.Name == name);
        }
    }

    // GET: api/companies/[id]
    [HttpGet("{id}", Name = nameof(GetCompany))]
    [ProducesResponseType(200, Type = typeof(Company))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCompany(int id)
    {
        Company? c = await repo.RetrieveAsync(id);
        if (c == null)
        {
            return NotFound();
        }
        return Ok(c);
    }

    // POST: api/companies
    // BODY: Company (JSON, XML)
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(Company))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] Company c)
    {
        if (c == null)
        {
            return BadRequest();
        }
        Company? addedCompany = await repo.CreateAsync(c);
        if (addedCompany == null)
        {
            return BadRequest("Repository failed to create company.");
        }
        else
        {
            return CreatedAtRoute(
              routeName: nameof(GetCompany),
              routeValues: new { id = addedCompany.Id },
              value: addedCompany);
        }
    }

    // PUT: api/companies/[id]
    // BODY: Company (JSON, XML)
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(
      int id, [FromBody] Company c)
    {
        if (c == null || c.Id != id)
        {
            return BadRequest();
        }
        Company? existing = await repo.RetrieveAsync(id);
        if (existing == null)
        {
            return NotFound();
        }
        await repo.UpdateAsync(id, c);
        return new NoContentResult();
    }

    // DELETE: api/Companies/[id]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        Company? existing = await repo.RetrieveAsync(id);
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
              $"Company {id} was found but failed to delete.");
        }
    }
}
