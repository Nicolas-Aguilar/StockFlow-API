using StockFlow.Domain.Common;
using StockFlow.Domain.Enums;
using StockFlow.Domain.Exceptions;

namespace StockFlow.Domain.Entities;

public sealed class Product : AuditableEntity
{
    private Product()
    {
    }

    public Guid BusinessId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string InternalCode { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal PurchasePrice { get; private set; }
    public decimal SalePrice { get; private set; }
    public int CurrentStock { get; private set; }
    public int MinimumStock { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Business? Business { get; private set; }
    public Category? Category { get; private set; }
    public ICollection<InventoryMovement> InventoryMovements { get; } = new List<InventoryMovement>();
    public ICollection<SaleItem> SaleItems { get; } = new List<SaleItem>();

    public decimal ProfitPerUnit => SalePrice - PurchasePrice;
    public decimal ProfitMarginPercentage => SalePrice <= 0 ? 0 : Math.Round((ProfitPerUnit / SalePrice) * 100m, 2);
    public bool IsLowStock => CurrentStock <= MinimumStock;

    public static Product Create(
        Guid businessId,
        Guid categoryId,
        string name,
        string internalCode,
        string? description,
        decimal purchasePrice,
        decimal salePrice,
        int currentStock,
        int minimumStock,
        DateTime? expirationDate,
        Category? category = null)
    {
        ValidateCatalogState(purchasePrice, salePrice, currentStock, minimumStock, expirationDate);

        return new Product
        {
            BusinessId = businessId,
            CategoryId = categoryId,
            Name = name.Trim(),
            InternalCode = internalCode.Trim(),
            Description = description?.Trim(),
            PurchasePrice = purchasePrice,
            SalePrice = salePrice,
            CurrentStock = currentStock,
            MinimumStock = minimumStock,
            ExpirationDate = expirationDate,
            IsActive = true,
            Category = category
        };
    }

    public void UpdateCatalogDetails(
        Guid categoryId,
        string name,
        string internalCode,
        string? description,
        decimal purchasePrice,
        decimal salePrice,
        int minimumStock,
        DateTime? expirationDate,
        bool isActive,
        DateTime updatedAtUtc,
        Category? category = null)
    {
        ValidateCatalogState(purchasePrice, salePrice, CurrentStock, minimumStock, expirationDate);

        CategoryId = categoryId;
        Name = name.Trim();
        InternalCode = internalCode.Trim();
        Description = description?.Trim();
        PurchasePrice = purchasePrice;
        SalePrice = salePrice;
        MinimumStock = minimumStock;
        ExpirationDate = expirationDate;
        IsActive = isActive;
        Category = category;
        UpdatedAt = updatedAtUtc;
    }

    public bool IsExpiredAt(DateTime currentDateUtc)
        => ExpirationDate.HasValue && ExpirationDate.Value.Date < currentDateUtc.Date;

    public int? GetDaysUntilExpiration(DateTime currentDateUtc)
        => ExpirationDate.HasValue ? (ExpirationDate.Value.Date - currentDateUtc.Date).Days : null;

    public void EnsureCanBeSold(int quantity, DateTime currentDateUtc)
    {
        if (quantity <= 0)
        {
            throw new ValidationDomainException("Sale item quantity must be greater than zero.");
        }

        if (!IsActive)
        {
            throw new ProductInactiveException($"Product '{Name}' is inactive and cannot be sold.");
        }

        if (IsExpiredAt(currentDateUtc))
        {
            throw new ProductExpiredException($"Product '{Name}' is expired and cannot be sold.");
        }

        EnsureHasEnoughStock(quantity);
    }

    public void ApplyInventoryMovement(InventoryMovementType movementType, int quantity, DateTime updatedAtUtc)
    {
        if (quantity <= 0)
        {
            throw new ValidationDomainException("Inventory movement quantity must be greater than zero.");
        }

        if (movementType != InventoryMovementType.Entry)
        {
            EnsureHasEnoughStock(quantity);
        }

        CurrentStock += movementType == InventoryMovementType.Entry ? quantity : -quantity;
        UpdatedAt = updatedAtUtc;
    }

    public void Deactivate(DateTime updatedAtUtc)
    {
        IsActive = false;
        UpdatedAt = updatedAtUtc;
    }

    private void EnsureHasEnoughStock(int quantity)
    {
        if (CurrentStock < quantity)
        {
            throw new InsufficientStockException($"Product '{Name}' does not have enough stock.");
        }
    }

    private static void ValidateCatalogState(decimal purchasePrice, decimal salePrice, int currentStock, int minimumStock, DateTime? expirationDate)
    {
        if (salePrice < purchasePrice)
        {
            throw new ValidationDomainException("Sale price must be greater than or equal to purchase price.");
        }

        if (currentStock < 0 || minimumStock < 0)
        {
            throw new ValidationDomainException("Stock values cannot be negative.");
        }

        if (expirationDate.HasValue && expirationDate.Value.Year < 2000)
        {
            throw new ValidationDomainException("Expiration date is invalid.");
        }
    }
}
