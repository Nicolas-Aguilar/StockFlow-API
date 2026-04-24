using StockFlow.Domain.Common;

namespace StockFlow.Domain.Entities;

public sealed class Product : AuditableEntity
{
    public Guid BusinessId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InternalCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Business? Business { get; set; }
    public Category? Category { get; set; }
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();

    public decimal ProfitPerUnit => SalePrice - PurchasePrice;
    public decimal ProfitMarginPercentage => SalePrice <= 0 ? 0 : Math.Round((ProfitPerUnit / SalePrice) * 100m, 2);
    public bool IsLowStock => CurrentStock <= MinimumStock;
    public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value.Date < DateTime.UtcNow.Date;
    public int? DaysUntilExpiration => ExpirationDate.HasValue ? (ExpirationDate.Value.Date - DateTime.UtcNow.Date).Days : null;
}
