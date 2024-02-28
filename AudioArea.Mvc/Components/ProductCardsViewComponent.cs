using AudioArea.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Packt.Shared;

namespace AudioArea.Mvc.Components
{
    public class ProductCardsViewComponent : ViewComponent
    {
        private readonly AudioContext db;
        private readonly IHttpClientFactory clientFactory;

        public ProductCardsViewComponent(AudioContext injectedContext, IHttpClientFactory httpClientFactory)
        {
            db = injectedContext;
            clientFactory = httpClientFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(int[] companyIds, int[] categoryIds, Pagination? pagination,
            string sortedBy)
        {
            IEnumerable<Product>? Products;

            if (companyIds != null && companyIds.Any() &&
                categoryIds != null && categoryIds.Any() &&
                pagination != null)
            {
                string uri = $"api/products/?companyId={String.Join(',', companyIds)}&categoryId={String.Join(',', categoryIds)}&CurrentPage={pagination.CurrentPage}&PageSize={pagination.PageSize}&sortedBy={sortedBy}";
                HttpClient client = clientFactory.CreateClient(name: "AudioArea.WebApi");
                HttpRequestMessage request = new(method: HttpMethod.Get, requestUri: uri);
                HttpResponseMessage response = await client.SendAsync(request);
                Products = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();
                string HeaderPagination = response.Headers.GetValues("X-pagination").FirstOrDefault("");
                pagination = JsonConvert.DeserializeObject<Pagination>(HeaderPagination);
            }
            else
            {
                Products = await db.Products.Take(3).ToListAsync();
                pagination = new Pagination();
            }

            ProductCardViewModel model = new 
            (
              Products, 
              pagination
            );
            return View(model);
        }
    }
}
