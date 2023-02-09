using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        kit,
        boombox,
        photo,
        professional,
        diatoneds,
        diatonesp,
        speaker_unit,
    }

    class Parser
    {
        private int categoryId = (int)Enum.GetValues(typeof(Category_t)).Cast<Category_t>().Max() + 1;
        private int productId;
        private List<Category> categories { get; } = new List<Category>();
        
        public List<Category> GetCategories(string url, int count)
        {
            var companies = ParseCompanies(url);

            for (int i = 0; i < count; i++) //max number of companies is reduce to 165 due to deserialize error: stack overflow (all companies: 171)
            {
                ParseCategory(companies[i]);
            }
            return categories;
        }

        public List<Company> ParseCompanies(string url, string fileName = null)
        {
            string filePath = Path.Combine(Program.ProjPath, "test", "root", "audio-database.html");
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

            for (int i = 0; i < links.Count; i++)
            {
                if (links[i].Attributes.Count > 0)
                {
                    string href = links[i].Attributes["href"].Value;
                    int index = href.IndexOf('/');

                    Company company = new Company
                    {
                        Name = links[i].InnerText.Replace('/', '-'),
                        Link = baseLink + '/' + href,
                        BaseLink = baseLink + '/' + href.Substring(0, index + 1),
                        Id = i
                    };
                    companies.Add(company);
                }
            }

            return companies;
        }

        public void ParseCategory(Company company, string pathFile = null, string mainCategory = null, string imageName = null, string urlSubCategory = null)
        {
            pathFile ??= Path.Combine(Program.ProjPath, "test", "companies", company.Name + ".html");
            string html = GetHtml(urlSubCategory ?? company.Link, pathFile);

            if (!String.IsNullOrEmpty(html))
            {
                ParseHtmlCategory(html, company, mainCategory, imageName);
            }
        }

        private Category AddMainCategory(string mainCategory)
        {
            Category cat = null;
            if (Enum.TryParse(mainCategory, out Category_t mainCat))
            {
                cat = new(mainCategory)
                {
                    Id = (int)mainCat,
                    ParentId = 0,
                };
                categories.Add(cat);
            }
            return cat;
        }

        private void FillProducts(Category category, Product product)
        {
            string mainCategory;
            if (category.ParentId != 0)
            {
                mainCategory = ((Category_t)category.ParentId).ToString();
            }
            else
            {
                mainCategory = ((Category_t)category.Id).ToString();
            }
            int index = product.Link.LastIndexOf('/'); //sometimes product.Name != link
            if (index != -1)
            {
                string fileName = Path.GetFileName(product.Company.Name + '_' + product.Link.Substring(index + 1));
                string pathFile = Path.Combine(Program.ProjPath, "test", Scraper.ProductsDir, mainCategory, fileName);
                ParseHtmlProducts(product, pathFile);
            }
            else
            {
                Console.WriteLine("Product: " + product.Name + " link: " + product.Link);
            }
        }

        private void FillCategory(Category category, Product product, int? mainCatID)
        {
            if (mainCatID != null)
            {
                category.ParentId = (int)mainCatID;
                category.Products.Add(product);
                product.Company.Products.Add(product);
                FillProducts(category, product);
            }
            else
            {
                Console.WriteLine("Product: " + product.Link);
            }
        }

        private void SetProductImage(Product product, string imageName, string baselink)
        {
            if (imageName != null)
            {
                product.ImageLink = baselink + imageName;
                int index = imageName.LastIndexOf('/'); // sometimes name is speaker/nameImg.jpg

                if (index != -1)
                {
                    imageName = imageName.Substring(index + 1);
                }
                product.ImageName = imageName;
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
                    if (nodeAreaItem.Attributes[0].Value == "brandarea") //search in subcategory
                    {
                        string subCategoryLink = linkNode.Attributes[0].Value;
                        int index = subCategoryLink.IndexOf('/');
                        if (index != -1)
                        {
                            string mainCategory = subCategoryLink.Substring(0, index);
                            string fileName = Path.GetFileName(company.Name + '_' + String.Join(',', categoryGroups) + ".html");
                            string pathFile = Path.Combine(Program.ProjPath, "test", "categoryGroup", fileName);
                            string imageName = linkNode.ChildNodes["img"].Attributes[0].Value;
                            ParseCategory(company, pathFile, mainCategory, imageName, company.BaseLink + subCategoryLink);
                        }
                    }
                    else if (categoryGroups.Count() == 1) //get products
                    {
                        Product product = new Product(category, company)
                        {
                            Name = categoryGroups[0],
                            Id = productId++,
                        };
                        int indexMainCat = linkNode.Attributes[0].Value.IndexOf('/'); //check if link contain main category e.g speaker/E-101.html
                        if (indexMainCat != -1)
                        {
                            if (category.ParentId == 0) //category other is in category other, e.g Audiotechnika Other booster -> other
                            {
                                product.Link = company.BaseLink + linkNode.Attributes[0].Value;
                                string imageName = linkNode.ChildNodes["img"]?.Attributes["src"]?.Value;
                                SetProductImage(product, imageName, company.BaseLink);

                                category.Products.Add(product);
                                company.Products.Add(product);
                                FillProducts(category, product);
                            }
                            else
                            {
                                string mainCategory = linkNode.Attributes[0].Value.Substring(0, indexMainCat);
                                if (mainCategory == "..")
                                {
                                    linkNode.Attributes[0].Value = linkNode.Attributes[0].Value.Replace("../", "");
                                    linkNode.FirstChild.Attributes[0].Value = linkNode.FirstChild.Attributes[0].Value.Replace("../", "");
                                    indexMainCat = linkNode.Attributes[0].Value.IndexOf('/');
                                    mainCategory = linkNode.Attributes[0].Value.Substring(0, indexMainCat);
                                }

                                product.Link = company.BaseLink + linkNode.Attributes[0].Value;
                                string imageName = linkNode.ChildNodes["img"]?.Attributes["src"]?.Value;
                                SetProductImage(product, imageName, company.BaseLink);
                                
                                Category mainCat = categories.FirstOrDefault(x => CheckCatNames(ref mainCategory, x.Name));
                                mainCat ??= AddMainCategory(mainCategory.ToLower());
                                FillCategory(category, product, mainCat?.Id);
                            }
                        }
                        else
                        {
                            string imageName = linkNode.ChildNodes["img"]?.Attributes["src"]?.Value;

                            if (category.ParentId == 0) //category other is in category other, e.g Audiotechnika Other booster -> other
                            {
                                product.Link = category.BaseLink + '/' + category.Name + '/' + linkNode.Attributes[0].Value;
                                SetProductImage(product, imageName, category.BaseLink + '/' + category.Name + '/');

                                category.Products.Add(product);
                                company.Products.Add(product);
                                FillProducts(category, product);
                            }
                            else
                            {
                                if (category.BaseLink == null) //exitst category in categories
                                {
                                    category.BaseLink = company.BaseLink.Remove(company.BaseLink.Length - 1);
                                }

                                string mainCategory;
                                if (category.ParentId == -1)
                                {
                                    int index = category.BaseLink.LastIndexOf('/');
                                    mainCategory = category.BaseLink.Substring(index + 1);
                                }
                                else
                                {
                                    mainCategory = ((Category_t)category.ParentId).ToString();
                                }
                                Category mainCat = categories.FirstOrDefault(x => CheckCatNames(ref mainCategory, x.Name));
                                mainCat ??= AddMainCategory(mainCategory.ToLower());
                                if (mainCat != null)
                                {
                                    product.Link = company.BaseLink + mainCat.Name + '/' + linkNode.Attributes[0].Value;
                                    SetProductImage(product, imageName, company.BaseLink + mainCat.Name + '/');
                                }
                                FillCategory(category, product, mainCat?.Id);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Wrong element: Company: {company.Name}, Link: {company.BaseLink + linkNode.Attributes[0].Value}");
                    }
                }
            }
        }

        private bool CheckCatNames(ref string catName, string existCatName)
        {
            catName = catName.ToLower();
            if (existCatName == catName)
            {
                return true;
            }
            string mainCategory = Translation.ResourceManager.GetString(catName);
            if (mainCategory is not null)
            {
                catName = mainCategory;
                if (existCatName == catName)
                {
                    return true;
                }
            }
            return false;
        }

        private List<Category> ParseHtmlCategory(string html, Company company, string mainCategory = null, string imageName = null)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var nodeAreaItems = htmlDoc.DocumentNode.SelectNodes("//main[@id=\"contents\"]/ul[contains(@class, 'area')]");

            if (nodeAreaItems != null)
            {
                string catName = nodeAreaItems[0].ParentNode.SelectSingleNode("//h5")?.InnerText;
                if (catName != null) //np. MITSUBISHI-DIATONE/tech
                {
                    foreach (HtmlNode nodeAreaItem in nodeAreaItems)
                    {
                        var linkNodes = nodeAreaItem.SelectNodes(".//li/a");
                        if (nodeAreaItem.PreviousSibling.Name == "h5")
                        {
                            catName = nodeAreaItem.PreviousSibling.InnerText;
                        }
                        Category category = categories.FirstOrDefault(x => CheckCatNames(ref catName, x.Name));

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
                                    if (imageName != null)
                                    {
                                        category.ImageLink = company.BaseLink + imageName;
                                        category.ImageName = imageName;
                                    }
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
            }

            return categories;
        }

        public List<Company> ParseHtmlProducts(Product product, string htmlPath)
        {
            string html = GetHtml(product?.Link, htmlPath);

            if (!string.IsNullOrEmpty(html))
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                var commentaryNode = htmlDoc.DocumentNode.SelectNodes("//main[@id=\"contents\"]/div[@id=\"detailarea\"]/p[@class=\"detail\"]");
                var specNodes = htmlDoc.DocumentNode.SelectNodes("//main[@id=\"contents\"]/div[@id=\"specarea\"]/table[@class=\"spec\"]/tr");

                if (commentaryNode != null)
                {
                    product.Description = string.Join(Environment.NewLine, commentaryNode[0].InnerHtml.Trim().Split('\n').Select(s => s.Trim().Replace(" <br><br>", Environment.NewLine).Replace(" <br>", "")));
                    if (specNodes != null)
                    {
                        foreach (HtmlNode propiertyNode in specNodes)
                        {
                            if (propiertyNode.ChildNodes.Count == 2)
                            {
                                string propierty = string.Join(Environment.NewLine, propiertyNode.ChildNodes[0].InnerText.Trim().Split('\n').Select(s => s.Trim()));
                                string value = string.Join(Environment.NewLine, propiertyNode.ChildNodes[1].InnerText.Trim().Split('\n').Select(s => s.Trim()));
                                if (product.Properties.ContainsKey(propierty))
                                {
                                    product.Properties[propierty] += Environment.NewLine + value; //rows with the same propierty
                                }
                                else
                                {
                                    product.Properties.Add(propierty, value);
                                }
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
            }

            return null;
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

            if (Program.Config.HasFlag(RunConfig.GetLocalHtml))
            {
                if (File.Exists(filePath))
                {
                    response = File.ReadAllText(filePath);
                }
                else
                {
                    Console.WriteLine("File: " + filePath + " doesn't exist");
                    return response;
                }
            }
            else
            {
                if (!File.Exists(filePath))
                {
                    try
                    {
                        response = CallUrl(url).Result;
                        if (!String.IsNullOrEmpty(response))
                        {
                            File.WriteAllText(filePath, response);
                            response = File.ReadAllText(filePath);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Can't connect to url: {url}, Exception {e.Message}");
                    }
                }
                else
                {
                    response = File.ReadAllText(filePath);
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

        private void WriteToCsv(List<Company> companies, string fileName)
        {
            var files = Directory.GetFiles(Path.Combine(Program.ProjPath, "data"), "*.csv").ToList();
            files.ForEach(file => File.WriteAllText(file, string.Empty));

            String csv = String.Join(
                Environment.NewLine,
                companies.Select(d => $"{d.Name};{d.Link};")
            );

            File.WriteAllText(Path.Combine(Program.ProjPath, "data", fileName + ".csv"), csv);
        }
    }
}
