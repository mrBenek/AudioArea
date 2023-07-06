using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace Packt.Shared
{
    [JsonObject(IsReference = true)]
    [DataContract(IsReference = true)]
    public partial class Company
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Link { get; set; }
        public string? BaseLink { get; set; }

        [InverseProperty("Company")]
        [XmlIgnore]
        public virtual List<Product> Products { get; set; } = new List<Product>();
    }
}
