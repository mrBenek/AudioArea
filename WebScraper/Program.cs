using System;
using System.IO;

namespace WebScraper
{
    [Flags]
    public enum RunConfig
    {
        SaveToJson = 1,
        SaveToJsonTest = 2,
        LoadFromJson = 4,
        LoadFromJsonTest = 8,
        GetLocalHtml = 16,
        GetHtmlFromUrl = 32,
        DownloadImages = 64
    }

    public class Program
    {
        public static readonly string ProjPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        public static readonly RunConfig Config = RunConfig.SaveToJsonTest | RunConfig.GetLocalHtml;

        static void Main(string[] args)
        {
            Scraper scraper = new();
            var categories = scraper.Run();

            if (Config.HasFlag(RunConfig.SaveToJson) || Config.HasFlag(RunConfig.LoadFromJson))
            {
                Modifications modifications = new();
                modifications.InsertCategories(categories);
            }
        }
    }
}
