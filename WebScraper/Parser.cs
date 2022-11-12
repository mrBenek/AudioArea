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
            var companies = ParseCompanies(url, "links");
            foreach (var company in companies)
            {
                ParseCategory(company, false);
            }
            return companies;
        }

        public List<Company> ParseCompanies(string url, string fileName = null)
        {
            string filePath = Path.Combine(projPath, "test", "audio-database.html");
            string html = GetHtml(url, filePath);

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
            string pathFile = Path.Combine(projPath, "test", company.Name + ".html");
            string html = GetHtml(company.Link, pathFile);

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

        private List<Category> ParseHtmlCategory(string html, Company company)
        {
            List<Category> categories = company.Categories;
            Regex r = new Regex(@"\w.*\w", RegexOptions.Multiline);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var nodeCategory = htmlDoc.DocumentNode.SelectNodes("//main[@id=\"contents\"]/h5");
            var nodeSubcategory = htmlDoc.DocumentNode.SelectNodes("//main[@id=\"contents\"]/ul[contains(@class, 'area')]");

            if (nodeCategory != null && nodeSubcategory != null)
            {
                for (int i = 0; i < nodeCategory.Count; i++)
                {
                    string catName = nodeCategory[i].InnerText;
                    Category category = new (catName);
                    categories.Add(category);

                    if (i < nodeSubcategory.Count())
                    {
                        var link = nodeSubcategory[i].SelectNodes(".//li/a");
                        if (link != null)
                        {
                            foreach (HtmlNode nodeSubCatLink in link)
                            {
                                if (nodeSubCatLink.Attributes.Count > 0)
                                {
                                    string[] subCategories = r.Matches(nodeSubCatLink.InnerText)
                                                            .Cast<Match>()
                                                            .Select(m => m.Value)
                                                            .ToArray();

                                    if (subCategories != null)
                                    {
                                        foreach (string subCategory in subCategories)
                                        {
                                            SubCategory subCat = new SubCategory()
                                            {
                                                Name = subCategory,
                                                Link = company.BaseLink + nodeSubCatLink.Attributes[0].Value,
                                                PictureLink = company.BaseLink + nodeSubCatLink.FirstChild.Attributes[0].Value,
                                            };
                                            category.SubCategories.Add(subCat);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return categories;
        }

        private async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }

        private string GetHtml(string url, string filePath)
        {
            string response = "";

#if USE_HTML_FILE
            if (File.Exists(filePath))
            {
                response = File.ReadAllText(filePath);
            }
#else
            response = CallUrl(url).Result;

#endif
            if (!String.IsNullOrEmpty(response))
            {
                var decodedhtml = HttpUtility.HtmlDecode(response);
                decodedhtml = Regex.Replace(decodedhtml, @"(?<=>)\s+?(?=<)", string.Empty); //remove spaces between html tags
                return decodedhtml;
#if SAVE_HTML
                File.WriteAllText(filePath, html);
#endif
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
