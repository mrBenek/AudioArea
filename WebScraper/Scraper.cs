using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;


namespace WebScraper
{
    class Scraper
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

    class Company
    {
        internal string Name { get; set; }
        internal string Link { get; set; }
        internal string BaseLink { get; set; }
        internal List<Category> Categories = new List<Category>();
    }

    class Category
    {
        internal string Name { get; set; }
        internal List<Product> Products { get; set; }
        internal List<CategoryGroup> SubCategories;

        public Category(string name)
        {
            Name = name;
        }
    }

    class CategoryGroup
    {
        internal string Name { get; set; }
        internal string Link { get; set; }
        internal string PictureLink { get; set; }
        internal string BaseLink { get; set; }
        internal List<Category> Categories { get; set; } = new();

        public CategoryGroup()
        {
        }
    }

    class Product
    {
        internal string Name { get; set; }
        internal string Link { get; set; }
        internal string PictureLink { get; set; }
        internal string MainCategory { get; set; }
        internal string Description { get; set; }

        public Product()
        {

        }
    }

    class Amplifier : Product
    {
        internal string Type { get; set; }
        internal string RatedOutput { get; set; }
        internal string TotalHarmonicDistortion { get; set; }
        internal string IntermodulationDistortion { get; set; }
    }
}
