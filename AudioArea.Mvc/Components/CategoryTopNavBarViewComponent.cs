using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Packt.Shared;

namespace AudioArea.Mvc.Components
{
    public class CategoryTopNavBarViewComponent : ViewComponent
    {
        private readonly AudioContext db;
        private readonly IHttpClientFactory clientFactory;

        public CategoryTopNavBarViewComponent(AudioContext injectedContext,
                                              IHttpClientFactory httpClientFactory)
        {
            db = injectedContext;
            clientFactory = httpClientFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            string uri = "api/categories/?parentId=0";
            HttpClient client = clientFactory.CreateClient(name: "AudioArea.WebApi");
            HttpRequestMessage request = new(method: HttpMethod.Get, requestUri: uri);
            HttpResponseMessage response = await client.SendAsync(request);
            IEnumerable<Category>? model = await response.Content
                                                 .ReadFromJsonAsync<IEnumerable<Category>>();

            return View(model);
        }
    }
}
