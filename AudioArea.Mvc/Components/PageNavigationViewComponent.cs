using Microsoft.AspNetCore.Mvc;
using Packt.Shared;

namespace AudioArea.Mvc.Components
{
    public class PageNavigationViewComponent : ViewComponent
    {
        public PageNavigationViewComponent()
        {
        }

        public IViewComponentResult Invoke(Pagination pagination)
        {
            return View(pagination);
        }
    }
}
