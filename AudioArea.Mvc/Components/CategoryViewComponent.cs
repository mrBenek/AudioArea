using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Packt.Shared;

namespace AudioArea.Mvc.Components
{
    public class CategoryViewComponent : ViewComponent
    {
        protected readonly AudioContext db;

        public CategoryViewComponent(AudioContext injectedContext)
        {
            db = injectedContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            IList<Category> Categories = await db.Categories.Take(5).ToListAsync();
            return View(Categories);
        }
    }
}
