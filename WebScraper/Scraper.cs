//#define SAVE_DATA_TO_JSON

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebScraper
{
    public class Scraper
    {
        public const string url = "https://audio-database.com";
        static string projPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        static void Main(string[] args)
        {
            Parser parser = new Parser();
            //ClearCSVFiles();
#if SAVE_DATA_TO_JSON
            parser.SaveCategoriesToJson(url, "links");
#endif
            var categories = parser.LoadProductsJsonFile();
        }

        static void ClearCSVFiles()
        {
            var files = Directory.GetFiles(Path.Combine(projPath, "data"), "*.csv").ToList();
            files.ForEach(file => File.WriteAllText(file, string.Empty));
        }
    }

    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string BaseLink { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();
    }

    public class Category
    {
        public int Id { get; set; }
        public int ParentId { get; set; } = -1;
        public string Name { get; set; }
        public string Link { get; set; }
        public string PictureLink { get; set; }
        public string BaseLink { get; set; }
        public string FileName { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();

        public Category() { } //need for json deserialize

        public Category(string name)
        {
            Name = name;
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string PictureLink { get; set; }
        public string Description { get; set; }
        
        [NotMapped] //need for deserialize json file
        public Dictionary<string, string> Propierties { get; set; } = new(); //json

        public int CategoryId { get; set; }
        public Category Category { get; set; }
        
        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public Product() { } //need for json deserialize

        public Product(Category category, Company company)
        {
            if (category != null)
            {
                Category = category;
                CategoryId = category.Id;
            }
            if (company != null)
            {
                Company = company;
                CompanyId = company.Id;
            }
        }
    }
}
