using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebScraper
{
    public class Scraper
    {
        private const string url = "https://audio-database.com";
        private List<Category> categories;
        private Parser parser = new();
        private Serializer serializer = new();

        public const string ProductsDir = "products";
        public const string CategoriesDir = "categories";

        public List<Category> Run()
        {
            if (Program.Config.HasFlag(RunConfig.SaveToJson))
            {
                categories = parser.GetCategories(url, 165);
                serializer.SerializeToJson(categories);
            }
            else if (Program.Config.HasFlag(RunConfig.SaveToJsonTest))
            {
                categories = parser.GetCategories(url, 5);
                serializer.SerializeToJson(categories);
            }
            else if (Program.Config.HasFlag(RunConfig.LoadFromJson) ||
                     Program.Config.HasFlag(RunConfig.LoadFromJsonTest))
            {
                categories = serializer.DeserializeFromJson<Category>();
            }

            if (Program.Config.HasFlag(RunConfig.DownloadImages))
            {
                DownloadImage();
            }
            return categories;
        }

        private void DownloadImage()
        {
            Task taskDownloadImage = Task.Run(() => DownloadImageAsynch(categories));
            taskDownloadImage.Wait();
        }

        private async Task DownloadFileAsynch(HttpClient httpClient, string filePath, string imageLink)
        {
            if (!File.Exists(filePath))
            {
                try
                {
                    var imageBytes = await httpClient.GetByteArrayAsync(new Uri(imageLink));
                    await File.WriteAllBytesAsync(filePath, imageBytes);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Can't download file from url: " + imageLink + " Exception: " + e.Message);
                }
            }
        }

        public async Task DownloadImageAsynch(List<Category> categories)
        {
            using (HttpClient httpClient = new())
            {
                foreach (Product product in categories.SelectMany(x => x.Products))
                {
                    if (product.ImageLink != null && product.ImageName != null)
                    {
                        string filePath = Path.Combine(Program.ProjPath, "images", ProductsDir, product.Category.MainCategoryId, product.ImageName);
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        await DownloadFileAsynch(httpClient, filePath, product.ImageLink);
                    }
                    else
                    {
                        Console.WriteLine("Function DownloadImageAsynch() -> Company: " + product.Company.Name + ", Category: " + product.Category.Name + ", Product: " + product.Name);
                    }

                    foreach (Category category in categories)
                    {
                        if (category.ImageLink != null && category.ImageName != null)
                        {
                            string filePath = Path.Combine(Program.ProjPath, "images", CategoriesDir, category.ImageName);
                            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                            await DownloadFileAsynch(httpClient, filePath, category.ImageLink);
                        }
                    }
                }
            }
        }
    }
}
