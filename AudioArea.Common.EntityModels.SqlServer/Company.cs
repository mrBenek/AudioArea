using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Packt.Shared
{
    public partial class Company
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Link { get; set; }
        public string? BaseLink { get; set; }
        public virtual List<Product> Products { get; set; } = new List<Product>();
    }
}
