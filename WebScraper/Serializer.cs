using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace WebScraper
{
    public class Serializer
    {
        private const string JsonFileName = "Categories";
        private readonly string filePathJson = Path.Combine(Program.ProjPath, "data", JsonFileName + ".json");
        private readonly string filePathJsonTest = Path.Combine(Program.ProjPath, "data", JsonFileName + "_test.json");
        private readonly string filePath;

        private readonly JsonSerializerSettings jsonSerializerSettings = new()
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            MaxDepth = 300
        };

        public Serializer()
        {
            if (Program.Config.HasFlag(RunConfig.SaveToJson) ||
                Program.Config.HasFlag(RunConfig.LoadFromJson))
            {
                jsonSerializerSettings.Formatting = Formatting.None;
                filePath = filePathJson;
            }
            else if (Program.Config.HasFlag(RunConfig.SaveToJsonTest) ||
                     Program.Config.HasFlag(RunConfig.LoadFromJsonTest))
            {
                jsonSerializerSettings.Formatting = Formatting.Indented;
                filePath = filePathJsonTest;
            }
        }

        //Categories are save to JSON file, because only they contain main categories
        public void SerializeToJson<T>(List<T> categories)
        {
            string jsonString = JsonConvert.SerializeObject(categories, jsonSerializerSettings);
            File.WriteAllText(filePath, jsonString);
        }

        public List<T> DeserializeFromJson<T>()
        {
            string jsonString = File.ReadAllText(filePath);
            return (List<T>)JsonConvert.DeserializeObject(jsonString, typeof(List<T>), jsonSerializerSettings);
        }

        private void CompareJson<T>(List<T> categories)
        {
            string jsonString = JsonConvert.SerializeObject(categories, Formatting.Indented, jsonSerializerSettings);
            string jsonStringOld = File.ReadAllText(filePath);

            JValue s1 = new JValue(jsonStringOld);
            JValue s2 = new JValue(jsonString);

            if (!JToken.DeepEquals(s1, s2))
            {
                string filePathOld = Path.Combine(Program.ProjPath, "data", JsonFileName + "Old.json");
                File.WriteAllText(filePathOld + "_old", jsonStringOld);
                File.WriteAllText(filePath, jsonString);
                Console.WriteLine("categories are not the same");
            }
            else
            {
                Console.WriteLine("categories are the same");
            }
        }
    }
}
