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
        static string projPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        static void Main(string[] args)
        {
            Parser parser = new Parser();
            //ClearCSVFiles();
            var companies = parser.GetCompanies("https://audio-database.com", "links");
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
        internal List<Category> Categories = new List<Category>();
    }

    class Category
    {
        internal string Name { get; set; }
        internal List<SubCategory> SubCategories = new List<SubCategory>();

        public Category(string name)
        {
            Name = name;
        }
    }

    class SubCategory
    {
        internal string Name { get; set; }
        internal string Link { get; set; }
        internal string PictureLink { get; set; }

        public SubCategory()
        {
        }
    }

    class Product
    {
        internal string Name { get; set; }
        internal string Link { get; set; }
        internal string PictureLink { get; set; }
    }
}
