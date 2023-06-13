using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Packt.Shared
{
	public partial class Product
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Link { get; set; }
        public string? ImageName { get; set; }
        public string? ImageLink { get; set; }
        public string? Description { get; set; }

        public Dictionary<string, string>? Properties { get; set; } = new(); //json

        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public int CompanyId { get; set; }
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
