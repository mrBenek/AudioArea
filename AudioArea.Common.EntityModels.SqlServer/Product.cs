using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Packt.Shared
{
    [JsonObject(IsReference = true)]
    [DataContract(IsReference = true)]
    public partial class Product
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string? Name { get; set; }
        [DataMember]
        public string? Link { get; set; }
        [DataMember]
        public string? ImageName { get; set; }
        [DataMember]
        public string? ImageLink { get; set; }
        [DataMember]
        public string? Description { get; set; }

        [XmlIgnore]
        public Dictionary<string, string>? Properties { get; set; } = new(); //json

        [DataMember]
        public int CategoryId { get; set; }

        //[InverseProperty("Category")]
        [XmlIgnore]
        public virtual Category? Category { get; set; }
        [DataMember]
        public int CompanyId { get; set; }

        
        [XmlIgnore]
        public virtual Company? Company { get; set; }

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

        public string GetImagePath()
        {
            char searchChar = '/';
            int occurrencePosition = 4;

            if (ImageLink != null)
            {
                Match? match = Regex.Matches(ImageLink, Regex.Escape(searchChar.ToString()))
                                   .Cast<Match>()
                                   .Skip(occurrencePosition - 1)
                                   .FirstOrDefault();

                if (match != null)
                    return ImageLink.Substring(match.Index + 1);
            }

            return "";
        }
    }
}
