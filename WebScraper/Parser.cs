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
                                foreach (Category cat in subCategory.Categories)
                                {
                                    if (cat.Products != null)
                                    {
                                        foreach (Product product in cat.Products)
                                        {
                                            ParseProducts(product, company.Name, false);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("subCategory.Products = null: company: " + company.Name + ", subCategory: " + subCategory.Name);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("catgory.SubCategories = null: company: " + company.Name + ", category: " + category.Name);
                        }
                    }
                    else
                    {
                        foreach (Product product in category.Products)
                        {
                            ParseProducts(product, company.Name, false);
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

                            int index = linkNode.Attributes[0].Value.IndexOf('/');
                            if (index != -1)
                            {
                                product.MainCategory = linkNode.Attributes[0].Value.Substring(0, index);
                            }
                            else
                            {
                                Console.WriteLine("company.BaseLink: " + company.BaseLink + ", Attribute value: " + linkNode.Attributes[0].Value);
                            }
                        }
                        else
                        {
                            int lastIndex = catGroup.Link.LastIndexOf("/");
                            string baseLink = catGroup.Link.Substring(0, lastIndex);
                            product.Link = baseLink + '/' + linkNode.Attributes[0].Value;
                            product.PictureLink = baseLink + '/' + linkNode.FirstChild.Attributes[0].Value;
                            product.MainCategory = baseLink.Substring(baseLink.LastIndexOf("/") + 1);
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
            if (product.MainCategory != null)
            {
                string pathDirectory = Path.Combine(projPath, "test", "products", product.MainCategory);
                if (!Directory.Exists(pathDirectory))
                {
                    Directory.CreateDirectory(pathDirectory);
                }
                fileName = companyName + "_" + Path.GetFileName(product.Link);
                string pathFile = Path.Combine(projPath, "test", "products", product.MainCategory, fileName);
                string html = GetHtml(product.Link, pathFile, false, true);

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
        }

        public List<Company> ParseHtmlProducts(Product product, string htmlPath, string baseLink)
        {
            string html = GetHtml(product?.Link, htmlPath, true, false);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var commentaryNode = htmlDoc.DocumentNode.SelectNodes("//main[@id=\"contents\"]/div[@id=\"detailarea\"]/p[@class=\"detail\"]");
            var specNodes = htmlDoc.DocumentNode.SelectNodes("//main[@id=\"contents\"]/div[@id=\"specarea\"]/table[@class=\"spec\"]/tr");

            if (commentaryNode != null)
            {
                string commentary = string.Join(Environment.NewLine, commentaryNode[0].InnerHtml.Trim().Split('\n').Select(s => s.Trim().Replace(" <br><br>", Environment.NewLine).Replace(" <br>", "")));
                if (specNodes != null)
                {
                    foreach (HtmlNode propiertyNode in specNodes)
                    {
                        if (propiertyNode.ChildNodes.Count == 2)
                        {
                            string propierty = string.Join(Environment.NewLine, propiertyNode.ChildNodes[0].InnerText.Trim().Split('\n').Select(s => s.Trim()));
                            string value = string.Join(Environment.NewLine, propiertyNode.ChildNodes[1].InnerText.Trim().Split('\n').Select(s => s.Trim()));
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No Specification! htmlPath: " + htmlPath);
                }
            }
            else
            {
                Console.WriteLine("No commentary! htmlPath: " + htmlPath);
            }

            return null;
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
