using AudioArea.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Packt.Shared;
using System.Diagnostics;

namespace AudioArea.Mvc.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
        private readonly AudioContext db;

        public HomeController(ILogger<HomeController> logger, AudioContext injectedContext)
		{
			_logger = logger;
            db = injectedContext;
        }

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

        public async Task<IActionResult> Products()
        {
          HomeProductsViewModel model = new
		  (
			Categories: await db.Categories.ToListAsync(),
			Companies: await db.Companies.ToListAsync()
		  );

          return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}