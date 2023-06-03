using Packt.Shared;

namespace AudioArea.Mvc.Models
{
    public record HomeProductsViewModel
    (
        IList<Category> Categories,
        IList<Company> Companies
    );
}
