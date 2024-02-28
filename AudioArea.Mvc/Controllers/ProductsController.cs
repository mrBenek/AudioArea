using AudioArea.Mvc.Components;
using AudioArea.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Packt.Shared;

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
        public IActionResult GetItemsPerPage(int[] companyIds, int[] categoryIds, int pageNumber, int itemsPerPage, string sortedBy)
        {
            Pagination pagination = new Pagination();
			pagination.CurrentPage = pageNumber;
			pagination.PageSize = itemsPerPage;

            return ViewComponent("ProductCards", new { companyIds, categoryIds, pagination, sortedBy });
        }
    }
}
