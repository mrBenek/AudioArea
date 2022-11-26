using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace WebScraper
{
    public enum Category_t
    {
        stereo = 1,
        etc,
        surround,
        unit,
        speaker,
        tuner,
        player,
        amp,
        portable,
        radio,
        headphone,
        kit
    }

    class Parser
    {
        private int categoryId = (int)Enum.GetValues(typeof(Category_t)).Cast<Category_t>().Max() + 1;
        List<Category> categories = new List<Category>();
        readonly string projPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        public List<Company> GetCompanies(string url, string fileName = null)
        {
            var companies = ParseCompanies(url, "links");
            int i = 0;
            foreach (var company in companies)
            {
                ParseCategory(company, false);
                company.Id = i++;
            }
            var options = new JsonSerializerOptions { WriteIndented = true };

            string filePath = Path.Combine(projPath, "data", "Categories.json");
            string jsonString = JsonSerializer.Serialize(categories, options);
            File.WriteAllText("Categories.json", jsonString);

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

        public void ParseCategory(Company company, bool saveToFile, string pathFile = null, string fileName = null, string mainCategory = null)
        {
            pathFile ??= Path.Combine(projPath, "test", "companies", company.Name + ".html");
            string html = GetHtml(company.Link, pathFile, true, false);

            if (!String.IsNullOrEmpty(html))
            {
                var categories = ParseHtmlCategory(html, company, mainCategory);

                if (saveToFile)
                {
                    foreach (Category category in categories)
                    {
                        WriteToCsv(category, fileName);
                    }
                }
            }
        }

        private void ParseLinks(HtmlNode linkNode, HtmlNode nodeAreaItem, Company company, ref Category category)
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
                        int index = linkNode.Attributes[0].Value.IndexOf('/');
                        if (index != -1)
                        {
                            string mainCategory = linkNode.Attributes[0].Value.Substring(0, index);
                            string fileName = Path.GetFileName(company.Name + '_' + String.Join(',', categoryGroups) + ".html");
                            string pathFile = Path.Combine(projPath, "test", "categoryGroup", fileName);
                            ParseCategory(company, false, pathFile, null, mainCategory);
                        }
                    }
                    else if (categoryGroups.Count() == 1)
                    {
                        Product product = new Product(category, company)
                        {
                            Name = categoryGroups[0],
                        };
                        int index = linkNode.Attributes[0].Value.IndexOf('/');
                        if (index != -1)
                        {
                            string mainCategory = linkNode.Attributes[0].Value.Substring(0, index);
                            if (mainCategory == "..")
                            {
                                linkNode.Attributes[0].Value = linkNode.Attributes[0].Value.Replace("../", "");
                                linkNode.FirstChild.Attributes[0].Value = linkNode.FirstChild.Attributes[0].Value.Replace("../", "");
                            }
                            index = linkNode.Attributes[0].Value.IndexOf('/');
                            mainCategory = linkNode.Attributes[0].Value.Substring(0, index);

                            product.Link = company.BaseLink + linkNode.Attributes[0].Value;
                            product.PictureLink = company.BaseLink + linkNode.FirstChild.Attributes[0].Value;

                            Category cat = categories.FirstOrDefault(x => x.Name == mainCategory);

                            if (cat == null)
                            {
                                if (Enum.TryParse(mainCategory.ToLower(), out Category_t mainCat))
                                {
                                    cat = new(mainCategory)
                                    {
                                        Id = (int)mainCat,
                                        ParentId = 0,
                                    };
                                    categories.Add(cat);
                                }
                            }
                            if (cat != null)
                            {
                                category.ParentId = cat.Id;
                                category.Products.Add(product);
                            }
                            else
                            {
                                Console.WriteLine("1. company.BaseLink: " + company.BaseLink + ", Attribute value: " + linkNode.Attributes[0].Value);
                            }
                        }
                        else
                        {
                            if (category.BaseLink == null)
                            {
                                category.BaseLink = company.BaseLink.Remove(company.BaseLink.Length - 1);
                            }
                            product.Link = category.BaseLink + '/' + linkNode.Attributes[0].Value;
                            product.PictureLink = category.BaseLink + '/' + linkNode.FirstChild.Attributes[0].Value;

                            int index2 = category.BaseLink.LastIndexOf('/');
                            string mainCategory2;
                            if (category.ParentId == 0)
                            {
                                mainCategory2 = category.BaseLink.Substring(index2 + 1);
                            }
                            else
                            {
                                mainCategory2 = ((Category_t)category.ParentId).ToString();
                            }
                            Category cat = categories.FirstOrDefault(x => x.Name == mainCategory2);
                                
                            if (cat == null)
                            {
                                if (Enum.TryParse(mainCategory2.ToLower(), out Category_t mainCat))
                                {
                                    cat = new(mainCategory2)
                                    {
                                        Id = (int)mainCat,
                                        ParentId = 0,
                                    };
                                    categories.Add(cat);
                                }
                            }
                            if (cat != null)
                            {
                                category.ParentId = cat.Id;
                                category.Products.Add(product);
                            }
                            else
                            {
                                Console.WriteLine("2. company.BaseLink: " + company.BaseLink + ", Attribute value: " + linkNode.Attributes[0].Value);
                            }
                        }
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

        private string MainCategory(ref string name)
        {
            string mainCategory = Translation.ResourceManager.GetString(name.ToLower());
            if (mainCategory is not null)
            {
                name = mainCategory;
            }
            return name;
        }

        private List<Category> ParseHtmlCategory(string html, Company company, string mainCategory = null)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var nodeAreaItems = htmlDoc.DocumentNode.SelectNodes("//main[@id=\"contents\"]/ul[contains(@class, 'area')]");

            if (nodeAreaItems != null)
            {
                string catName = nodeAreaItems[0].ParentNode.SelectSingleNode("//h5").InnerText;
                foreach (HtmlNode nodeAreaItem in nodeAreaItems)
                {
                    var linkNodes = nodeAreaItem.SelectNodes(".//li/a");
                    if (nodeAreaItem.PreviousSibling.Name == "h5")
                    {
                        catName = nodeAreaItem.PreviousSibling.InnerText;
                    }
                    Category category = categories.FirstOrDefault(x => x.Name == MainCategory(ref catName));

                    if (nodeAreaItem.Attributes[0].Value != "brandarea" && linkNodes != null)
                    {
                        if (category == null)
                        {
                            category = new(catName)
                            {
                                Id = categoryId++,
                            };

                            if (mainCategory != null)
                            {
                                if (Enum.TryParse(mainCategory, out Category_t mainCat))
                                {
                                    category.ParentId = (int)mainCat;
                                }
                                else
                                {
                                    Console.WriteLine($"Main category: {mainCategory} doesn't exist");
                                    category.ParentId = 1000;
                                }
                                category.BaseLink = company.BaseLink + mainCategory;
                            }
                            categories.Add(category);
                        }
                    }

                    if (linkNodes != null)
                    {
                        foreach (HtmlNode linkNode in linkNodes)
                        {
                            ParseLinks(linkNode, nodeAreaItem, company, ref category);
                        }
                    }
                }
            }

            return categories;
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
            //if (IsValidPath(category.Name))
            //{
            //    String catategoryCompany = String.Join(
            //        Environment.NewLine,
            //        category.SubCategories.Select(d => $"{d.Name};{d.Link};{d.PictureLink}")
            //    );

            //    File.AppendAllText(Path.Combine(projPath, "data", "Categories", fileName ?? category.Name + ".csv"), catategoryCompany + Environment.NewLine);
            //}
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
