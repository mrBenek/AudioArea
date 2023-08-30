using Microsoft.AspNetCore.Mvc;
using Packt.Shared;

namespace AudioArea.Mvc.Components
{
    public class PageNavigationViewComponent : ViewComponent
    {
        private readonly AudioContext db;

        public PageNavigationViewComponent(AudioContext injectedContext)
        {
            db = injectedContext;
        }

        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
