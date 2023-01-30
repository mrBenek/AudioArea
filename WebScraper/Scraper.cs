//#define SAVE_DATA_TO_JSON

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;

namespace WebScraper
{
    public class Scraper
    {
        public const string url = "https://audio-database.com";
        static string projPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        static string filePathJson = Path.Combine(projPath, "data", "Categories.json");
        static string filePathJsonTest = Path.Combine(projPath, "data", "Categories_test.json");

        static void Main(string[] args)
        {
            Parser parser = new Parser();
            //ClearCSVFiles();
#if SAVE_DATA_TO_JSON
            parser.SaveCategoriesToJson(url, filePathJsonTest);
#endif
            var categories = parser.LoadCategoriesJsonFile(filePathJsonTest);

            using (var db = new AudioContext())
            {
                db.Database.OpenConnection();
                try
                {
                    //db.Categories.ExecuteDelete();
                    //db.Products.ExecuteDelete();
                    //db.Companies.ExecuteDelete();

                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Categories ON"); //allow insert primary key to db
                    db.Categories.AddRange(categories);
                    db.SaveChanges(true);
                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Categories OFF");
                }
                finally
                {
                    db.Database.CloseConnection();
                }
            }
        }

        static void ClearCSVFiles()
        {
            var files = Directory.GetFiles(Path.Combine(projPath, "data"), "*.csv").ToList();
            files.ForEach(file => File.WriteAllText(file, string.Empty));
        }
    }

    public class Company
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string BaseLink { get; set; }
        public virtual List<Product> Products { get; set; } = new List<Product>();
    }

    public class Category
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ParentId { get; set; } = -1;
        public string Name { get; set; }
        public string Link { get; set; }
        public string PictureLink { get; set; }
        public string BaseLink { get; set; }
        public string FileName { get; set; }
        public virtual List<Product> Products { get; set; } = new List<Product>();

        public Category() { } //need for json deserialize

        public Category(string name)
        {
            Name = name;
        }
    }

    public class Product
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string PictureLink { get; set; }
        public string Description { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new(); //json

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

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
