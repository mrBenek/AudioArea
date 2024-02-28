using Packt.Shared;

namespace AudioArea.Mvc.Models
{
    public record ProductCardViewModel
    (
        IEnumerable<Product>? Products,
        Pagination? Pagination
    );
}
