//#define SAVE_HTML
#define USE_HTML_FILE

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
        string projPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

        public List<Company> GetCompanies(string url, string fileName = null)
        {
            var companies = ParseCompanies("https://audio-database.com", "links");
            foreach (var company in companies)
            {
                ParseCategory(company, false);
                //ParseCategory("https://audio-database.com/MARANTZ", "Marantz");
            }
            return companies;
        }

        public List<Company> ParseCompanies(string url, string fileName = null)
        {
#if USE_HTML_FILE
            string response = File.ReadAllText(Path.Combine(projPath, "test", "audio-database.html")); //for tests
#else
            string response = CallUrl(url).Result;
#endif
            var decodedhtml = HttpUtility.HtmlDecode(response);
            decodedhtml = Regex.Replace(decodedhtml, @"(?<=>)\s+?(?=<)", string.Empty); //remove spaces between html tags
            var companyList = ParseHtmlCompanies(decodedhtml, url);

#if SAVE_HTML
            File.WriteAllText(Path.Combine(projPath, "test", "audio-database" + ".html"), decodedhtml);
#endif
            if (fileName != null)
            {
                WriteToCsv(companyList, fileName);
            }
            return companyList;
        }

        private List<Company> ParseHtmlCompanies(string html, string baseLink)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var links = htmlDoc.DocumentNode.SelectNodes("//li/a");

            List<Company> companies = new List<Company>();
            //Dictionary<string, string> companyLink = new Dictionary<string, string>();

            foreach (var link in links)
            {
                if (link.Attributes.Count > 0)
                {
                    Company company = new Company
                    {
                        Name = link.InnerText.Replace('/', '-'),
                        Link = baseLink + '/' + link.Attributes[0].Value,
                    };
                    companies.Add(company);
                }
                //companyLink.Add(link.InnerText, baseLink + '/' + link.ParentNode.FirstChild.Attributes[0].Value);
            }

            return companies;
        }

        private void WriteToCsv(List<Company> companies, string fileName)
        {
            String csv = String.Join(
                Environment.NewLine,
                companies.Select(d => $"{d.Name};{d.Link};")
            );

            File.WriteAllText(Path.Combine(projPath, "data", fileName + ".csv"), csv);
        }

        public void ParseCategory(string url, string fileName)
        {
            //string response = CallUrl(url).Result;
            string response = File.ReadAllText(Path.Combine(projPath, "test", "MARANTZ.html")); //for tests
            var decodedhtml = HttpUtility.HtmlDecode(response);
            var linkList = ParseHtmlCategory(decodedhtml, url);

            //WriteToCsv(linkList, fileName);
        }

        public void ParseCategory(Company company, bool saveToFile, string fileName = null)
        {
            string response = "";
#if USE_HTML_FILE
            string pathFile = Path.Combine(projPath, "test", company.Name + ".html");
            if (File.Exists(pathFile))
            {
                response = File.ReadAllText(pathFile);
            }
#else
            response = CallUrl(company.Link).Result;
#endif
            if (!String.IsNullOrEmpty(response))
            {
                var decodedhtml = HttpUtility.HtmlDecode(response);
                decodedhtml = Regex.Replace(decodedhtml, @"(?<=>)\s+?(?=<)", string.Empty); //remove spaces between html tags
                var categories = ParseHtmlCategory(decodedhtml, company);

#if SAVE_HTML
                File.WriteAllText(Path.Combine(projPath, "test", company.Name + ".html"), decodedhtml);
#endif
                if (saveToFile)
                {
                    foreach (Category category in categories)
                    {
                        WriteToCsv(category, fileName);
                    }
                }
            }
        }

        private List<Category> ParseHtmlCategory(string html, Company company)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var nodeCategory = htmlDoc.DocumentNode.SelectNodes("//main[@id=\"contents\"]/h5");
            var nodeSubcategory = htmlDoc.DocumentNode.SelectNodes("//main[@id=\"contents\"]/ul[contains(@class, 'area')]");

            Regex r = new Regex(@"\w.*\w", RegexOptions.Multiline);

            List<Category> categories = company.Categories;
            if (nodeCategory != null && nodeSubcategory != null)
            {
                for (int i = 0; i < nodeCategory.Count; i++)
                {
                    string catName = nodeCategory[i].InnerText;
                    Category category = new Category(catName);
                    categories.Add(category);

                    if (i < nodeSubcategory.Count())
                    {
                        foreach (HtmlNode nodeSubCatLink in nodeSubcategory[i].SelectNodes("//li/a"))
                        {
                            if (nodeSubCatLink.Attributes.Count > 0)
                            {
                                string[] subCategories = r.Matches(nodeSubCatLink.InnerText)
                                                        .Cast<Match>()
                                                        .Select(m => m.Value)
                                                        .ToArray();

                                if (subCategories != null)
                                {
                                    string baseLink = company.Link.Substring(0, company.Link.Length - 10);
                                    foreach (string subCategory in subCategories)
                                    {
                                        SubCategory subCat = new SubCategory()
                                        {
                                            Name = subCategory,
                                            Link = baseLink + nodeSubCatLink.FirstChild.Attributes[0].Value,
                                            PictureLink = baseLink + nodeSubCatLink.Attributes[0].Value,
                                        };
                                        category.SubCategories.Add(subCat);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return categories;
        }

        private Dictionary<string, string> ParseHtmlCategory(string html, string baseLink)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var node = htmlDoc.DocumentNode.SelectNodes("//li/a");

            Dictionary<string, string> wikiLink = new Dictionary<string, string>();
            Regex r = new Regex(@"\w.*\w", RegexOptions.Multiline);

            List<Category> categories = new List<Category>();
            if (node != null)
            {
                foreach (var link in node)
                {
                    Category category = categories.LastOrDefault();
                    string catName = link.ParentNode.ParentNode.PreviousSibling.PreviousSibling.InnerText;

                    if (catName != category?.Name)
                    {
                        category = new Category(catName);
                        categories.Add(category);
                    }
                    if (link.Attributes.Count > 0)
                    {
                        string[] subcategories = r.Matches(link.InnerText)
                                                .Cast<Match>()
                                                .Select(m => m.Value)
                                                .ToArray();

                        if (subcategories != null)
                        {
                            SubCategory subCategory = new SubCategory()
                            {
                                Name = String.Join(',', subcategories),
                                Link = baseLink + '/' + link.FirstChild.Attributes[0].Value,
                                PictureLink = baseLink + '/' + link.Attributes[0].Value,
                            };
                            category.SubCategories.Add(subCategory);
                        }
                    }
                }
                foreach (Category category in categories)
                {
                    WriteToCsv(category);
                }
            }

            return wikiLink;
        }

        private async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            var response = client.GetStringAsync(fullUrl);
            return await response;
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

                File.AppendAllText(Path.Combine(projPath, "data", fileName ?? category.Name + ".csv"), catategoryCompany);
            }
        }
    }
}
