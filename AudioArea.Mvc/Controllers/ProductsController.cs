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
        public IActionResult CompanyCheckedId(int[] companyIds)
        {
            return ViewComponent("CategoryFilter", companyIds);
        }
    }
}
