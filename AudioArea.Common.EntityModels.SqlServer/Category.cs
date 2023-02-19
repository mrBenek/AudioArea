using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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

	public partial class Category
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ParentId { get; set; } = -1;
        public string? Name { get; set; }
        public string? Link { get; set; }
        public string? ImageName { get; set; }
        public string? ImageLink { get; set; }
        public string? BaseLink { get; set; }
        public string? FileName { get; set; }
        public virtual List<Product> Products { get; set; } = new List<Product>();

        [NotMapped]
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
