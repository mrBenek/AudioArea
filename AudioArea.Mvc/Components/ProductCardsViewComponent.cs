using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Packt.Shared;

namespace AudioArea.Mvc.Components
{
    public class ProductCardsViewComponent : ViewComponent
    {
        private readonly AudioContext db;

        public ProductCardsViewComponent(AudioContext injectedContext)
        {
            db = injectedContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(int[] companyIds, int[] categoryIds)
        {
            IList<Product> Products;
            if (companyIds != null && companyIds.Any())
            {
                Products = await (from p in db.Products 
                                    where companyIds.Contains(p.CompanyId) && categoryIds.Contains(p.CategoryId)
                                    select p).Distinct().ToListAsync();
            }
            else
            {
                Products = await db.Products.Take(3).ToListAsync();
            }
            return View(Products);
        }
    }
}
