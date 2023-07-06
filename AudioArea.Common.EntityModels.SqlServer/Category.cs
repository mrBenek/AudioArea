using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Packt.Shared
{
	public enum CategoryType
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

    [JsonObject(IsReference = true)]
    [DataContract(IsReference = true)]
    public partial class Category
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int ParentId { get; set; } = -1;
        [DataMember]
        public string? Name { get; set; }
        [DataMember]
        public string? Link { get; set; }
        [DataMember]
        public string? ImageName { get; set; }
        [DataMember]
        public string? ImageLink { get; set; }
        [DataMember]
        public string? BaseLink { get; set; }
        [DataMember]
        public string? FileName { get; set; }
        [XmlIgnore]
        public virtual List<Product> Products { get; set; } = new List<Product>();

        [NotMapped]
        [JsonIgnore]
        public string MainCategoryId
        {
            get
            {
                int id = ParentId;
                if (ParentId == 0)
                {
                    id = Id;
                }
                return ((CategoryType)id).ToString();
            }
        }

        public Category() { } //need for json deserialize

        public Category(string name)
        {
            Name = name;
        }
    }
}
