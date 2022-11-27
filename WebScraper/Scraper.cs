using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;


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
            var companies = parser.GetCompanies(url, "links");

            //foreach (string htmlFile in Directory.GetFiles(Path.Combine(projPath, "test", "products", "amp")))
            //{
            //    parser.ParseHtmlProducts(null, htmlFile, "");
            //}
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
        public string Propierties { get; set; } //json

        public int CategoryId { get; set; }
        
        [JsonIgnore]
        public Category Category { get; set; }

        public int CompanyId { get; set; }

        [JsonIgnore]
        public Company Company { get; set; }

        public Product(Category category, Company company)
        {
            Category = category;
            CategoryId = category.Id;

            Company = company;
            CompanyId = company.Id;
        }
    }
}
