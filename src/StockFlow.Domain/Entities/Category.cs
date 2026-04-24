using StockFlow.Domain.Common;

namespace StockFlow.Domain.Entities;

public sealed class Category : AuditableEntity
{
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public Business? Business { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
