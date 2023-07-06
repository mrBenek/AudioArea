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
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string? Name { get; set; }
        [DataMember]
        public string? Link { get; set; }
        [DataMember]
        public string? BaseLink { get; set; }

        [InverseProperty("Company")]
        [XmlIgnore]
        public virtual List<Product> Products { get; set; } = new List<Product>();
    }
}
