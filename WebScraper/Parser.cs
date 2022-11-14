using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace WebScraper
{

    class Parser
    {
        readonly string projPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        public List<Company> GetCompanies(string url, string fileName = null)
        {
            var companies = ParseCompanies(url, "links");
            foreach (var company in companies)
            {
                ParseCategory(company, false);
                foreach (Category category in company.Categories)
                {
                    if (category.Products == null)
                    {
                        if (category.SubCategories != null)
                        {
                            foreach (CategoryGroup subCategory in category.SubCategories)
                            {
                                ParseCategoryGroup(subCategory, company, false);
                            }
                        }
                    }
                    else
                    {
                        foreach (Product product in category.Products)
                        {
                            //ParseProducts(product, company.Name, false);
                        }
                    }
                }
            }
            return companies;
        }

        public List<Company> ParseCompanies(string url, string fileName = null)
        {
            string filePath = Path.Combine(projPath, "test", "root", "audio-database.html");
            string html = GetHtml(url, filePath, true, false);

            if (!String.IsNullOrEmpty(html))
            {
                var companyList = ParseHtmlCompanies(html, url);
                if (fileName != null)
                {
                    WriteToCsv(companyList, fileName);
                }
                return companyList;
            }
            return null;
        }

        private List<Company> ParseHtmlCompanies(string html, string baseLink)
        {
            List<Company> companies = new();

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var links = htmlDoc.DocumentNode.SelectNodes("//li/a");

            foreach (var link in links)
            {
                if (link.Attributes.Count > 0)
                {
                    Company company = new Company
                    {
                        Name = link.InnerText.Replace('/', '-'),
                        Link = baseLink + '/' + link.Attributes[0].Value,
                    };
                    company.BaseLink = company.Link.Substring(0, company.Link.Length - 10);
                    companies.Add(company);
                }
            }

            return companies;
        }

        public void ParseCategory(Company company, bool saveToFile, string fileName = null)
        {
            string pathFile = Path.Combine(projPath, "test", "companies", company.Name + ".html");
            string html = GetHtml(company.Link, pathFile, true, false);

            if (!String.IsNullOrEmpty(html))
            {
                var categories = ParseHtmlCategory(html, company);

                if (saveToFile)
                {
                    foreach (Category category in categories)
                    {
                        WriteToCsv(category, fileName);
                    }
                }
            }
        }

        private void ParseLinks(HtmlNode linkNode, HtmlNode nodeAreaItem, Company company, ref Category category, CategoryGroup catGroup)
        {
            Regex r = new Regex(@"\w.*\w", RegexOptions.Multiline);
            
            if (linkNode.Attributes.Count > 0)
            {
                string[] categoryGroups = r.Matches(linkNode.InnerText)
                                        .Cast<Match>()
                                        .Select(m => m.Value)
                                        .ToArray();

                if (categoryGroups != null)
                {
                    if (nodeAreaItem.Attributes[0].Value == "brandarea")
                    {
                        CategoryGroup gruop = new CategoryGroup()
                        {
                            Name = String.Join(',', categoryGroups),
                            Link = company.BaseLink + linkNode.Attributes[0].Value,
                            PictureLink = company.BaseLink + linkNode.FirstChild.Attributes[0].Value,
                        };
                        if (category.SubCategories == null)
                        {
                            category.SubCategories = new();
                        }
                        category.SubCategories.Add(gruop);
                    }
                    else if (categoryGroups.Count() == 1)
                    {
                        Product product = new Product()
                        {
                            Name = categoryGroups[0],
                        };
                        if (catGroup == null)
                        {
                            product.Link = company.BaseLink + linkNode.Attributes[0].Value;
                            product.PictureLink = company.BaseLink + linkNode.FirstChild.Attributes[0].Value;
                        }
                        else
                        {
                            string baseLink = catGroup.Link.Substring(0, catGroup.Link.LastIndexOf("/") + 1);
                            product.Link = baseLink + linkNode.Attributes[0].Value;
                            product.PictureLink = baseLink + linkNode.FirstChild.Attributes[0].Value;
                        }
                        if (category.Products == null)
                        {
                            category.Products = new();
                        }
                        category.Products.Add(product);
                    }
                    else
                    {
                        Console.WriteLine("Wrong element:");
                        Console.WriteLine($"Company: {company.Name}");
                        Console.WriteLine($"Name: {String.Join(',', categoryGroups)}");
                        Console.WriteLine($"Link: {company.BaseLink + linkNode.Attributes[0].Value} \n\n");
                    }
                }
            }
        }

        private List<Category> ParseHtmlCategory(string html, Company company, CategoryGroup catGroup = null)
        {
            List<Category> categories = (catGroup == null) ? company.Categories : catGroup.Categories;

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var nodeAreaItems = htmlDoc.DocumentNode.SelectNodes("//main[@id=\"contents\"]/ul[contains(@class, 'area')]");
            //var nodeCategory = htmlDoc.DocumentNode.SelectNodes("//main[@id=\"contents\"]/h5");
            //var nodeAreaItem = htmlDoc.DocumentNode.SelectNodes("//main[@id=\"contents\"]/ul[@class=\"brandarea\"]");

            if (nodeAreaItems != null)
            {
                string catName = nodeAreaItems[0].ParentNode.SelectSingleNode("//h5").InnerText;
                foreach (HtmlNode nodeAreaItem in nodeAreaItems)
                {
                    if (nodeAreaItem.PreviousSibling.Name == "h5")
                    {
                        catName = nodeAreaItem.PreviousSibling.InnerText;
                    }
                    Category category = categories.FirstOrDefault(x => x.Name == catName);
                    
                    if (category == null)
                    {
                        category = new(catName);
                        categories.Add(category);
                    }

                    var linkNodes = nodeAreaItem.SelectNodes(".//li/a");

                    if (linkNodes != null)
                    {
                        foreach (HtmlNode linkNode in linkNodes)
                        {
                            ParseLinks(linkNode, nodeAreaItem, company, ref category, catGroup);
                        }
                    }
                }
            }

            return categories;
        }


        public void ParseCategoryGroup(CategoryGroup catGruop, Company company, bool saveToFile, string fileName = null)
        {
            fileName = Path.GetFileName(company.Name + '_' + catGruop.Name + ".html");
            string pathFile = Path.Combine(projPath, "test", "categoryGroup", fileName);
            string html = GetHtml(catGruop.Link, pathFile, true, false);

            if (!String.IsNullOrEmpty(html))
            {
                var products = ParseHtmlCategory(html, company, catGruop);

                //if (saveToFile)
                //{
                //    foreach (Category category in categories)
                //    {
                //        WriteToCsv(category, fileName);
                //    }
                //}
            }
        }

        public void ParseProducts(Product product, string companyName, bool saveToFile, string fileName = null)
        {
            fileName = product.Link.Substring(Scraper.url.Length + 1).Replace('/', '_');
            string pathFile = Path.Combine(projPath, "test", "products", fileName);
            string html = GetHtml(product.Link, pathFile, true, false);

            if (!String.IsNullOrEmpty(html))
            {
                //var products = ParseHtmlProducts(html, company);

                //if (saveToFile)
                //{
                //    foreach (Category category in categories)
                //    {
                //        WriteToCsv(category, fileName);
                //    }
                //}
            }
        }

        private List<Company> ParseHtmlProducts(string html, string baseLink)
        {
            List<Company> companies = new();

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var links = htmlDoc.DocumentNode.SelectNodes("//li/a");

            foreach (var link in links)
            {
                if (link.Attributes.Count > 0)
                {
                    Company company = new Company
                    {
                        Name = link.InnerText.Replace('/', '-'),
                        Link = baseLink + '/' + link.Attributes[0].Value,
                    };
                    company.BaseLink = company.Link.Substring(0, company.Link.Length - 10);
                    companies.Add(company);
                }
            }

            return companies;
        }

        private async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }

        private string GetHtml(string url, string filePath, bool useHtmlFile, bool saveHtml)
        {
            string response = "";

            if (useHtmlFile)
            {
                if (File.Exists(filePath))
                {
                    response = File.ReadAllText(filePath);
                }
            }
            else
            {
                if (!File.Exists(filePath))
                {
                    response = CallUrl(url).Result;
                    if (saveHtml && !String.IsNullOrEmpty(response))
                    {
                        File.WriteAllText(filePath, response);
                    }
                }
            }

            if (!String.IsNullOrEmpty(response))
            {
                var decodedhtml = HttpUtility.HtmlDecode(response);
                decodedhtml = Regex.Replace(decodedhtml, @"(?<=>)\s+?(?=<)", string.Empty); //remove spaces between html tags

                return decodedhtml;
            }

            return response;
        }

        private bool IsValidPath(string path)
        {
            if (path != null)
            {
                Regex r = new Regex(@"^[\w\-. ]+$");
                return r.IsMatch(path);
            }
            return false;
        }

        private void WriteToCsv(Category category, string fileName = null)
        {
            if (IsValidPath(category.Name))
            {
                String catategoryCompany = String.Join(
                    Environment.NewLine,
                    category.SubCategories.Select(d => $"{d.Name};{d.Link};{d.PictureLink}")
                );

                File.AppendAllText(Path.Combine(projPath, "data", "Categories", fileName ?? category.Name + ".csv"), catategoryCompany + Environment.NewLine);
            }
        }

        private void WriteToCsv(List<Company> companies, string fileName)
        {
            String csv = String.Join(
                Environment.NewLine,
                companies.Select(d => $"{d.Name};{d.Link};")
            );

            File.WriteAllText(Path.Combine(projPath, "data", "Companies", fileName + ".csv"), csv);
        }
    }
}
