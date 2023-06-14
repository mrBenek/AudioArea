using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Packt.Shared;

namespace AudioArea.Mvc.Components
{
    public class CompanyFilterViewComponent : ViewComponent
    {
        private readonly AudioContext db;

        public CompanyFilterViewComponent(AudioContext injectedContext)
        {
            db = injectedContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            IList<Company> companies = await db.Companies.ToListAsync();
            return View(companies);
        }
    }
}
