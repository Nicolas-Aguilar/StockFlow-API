using StockFlow.Domain.Common;

namespace StockFlow.Domain.Entities;

public sealed class Business : AuditableEntity
{
    public Guid OwnerUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public User? OwnerUser { get; set; }
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
