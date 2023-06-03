using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Packt.Shared;

namespace AudioArea.Mvc.Components
{
    public class CategoryFilterViewComponent : ViewComponent
    {
        private readonly AudioContext db;

        public CategoryFilterViewComponent(AudioContext injectedContext)
        {
            db = injectedContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(int[] companyIds)
        {
            IList<Category> Categories;
            if (companyIds != null && companyIds.Any())
            {
                Categories = await (from c in db.Categories 
                                    join p in db.Products on c.Id equals p.CategoryId
                                    where companyIds.Contains(p.CompanyId)
                                    select c).Distinct().ToListAsync();
            }
            else
            {
                Categories = await db.Categories.ToListAsync();
            }
            return View(Categories);
        }
    }
}
