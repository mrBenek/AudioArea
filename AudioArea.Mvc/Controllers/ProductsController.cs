using AudioArea.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace AudioArea.Mvc.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CompanyCheckedIds(int[] companyIds)
        {
            return ViewComponent("CategoryFilter", companyIds);
        }

        [HttpPost]
        public IActionResult CheckedIds(int[] companyIds, int[] categoryIds)
        {
           return ViewComponent("ProductItems", new { companyIds, categoryIds });
        }
    }
}
