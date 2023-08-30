using AudioArea.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using WebApiPagination.Entities.Dtos;

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
            PaginationParams paginationParams = new()
            {
                Page = pageNumber,
                ItemsPerPage = itemsPerPage
            };
            return ViewComponent("ProductCards", new { companyIds, categoryIds, paginationParams, sortedBy });
        }
    }
}
