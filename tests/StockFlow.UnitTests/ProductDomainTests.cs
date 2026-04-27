using StockFlow.Domain.Entities;
using StockFlow.Domain.Enums;
using StockFlow.Domain.Exceptions;

namespace StockFlow.UnitTests;

public sealed class ProductDomainTests
{
    private static readonly DateTime ReferenceDate = new(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ComputedFields_ReturnExpectedValues()
    {
        var product = CreateProduct(
            purchasePrice: 5m,
            salePrice: 8m,
            currentStock: 4,
            minimumStock: 5,
            expirationDate: ReferenceDate.AddDays(10));

        Assert.Equal(3m, product.ProfitPerUnit);
        Assert.Equal(37.5m, product.ProfitMarginPercentage);
        Assert.True(product.IsLowStock);
        Assert.False(product.IsExpiredAt(ReferenceDate));
        Assert.Equal(10, product.GetDaysUntilExpiration(ReferenceDate));
    }

    [Fact]
    public void ExpiredProduct_IsMarkedAsExpired()
    {
        var product = CreateProduct(expirationDate: ReferenceDate.AddDays(-1));

        Assert.True(product.IsExpiredAt(ReferenceDate));
        Assert.Equal(-1, product.GetDaysUntilExpiration(ReferenceDate));
    }

    [Fact]
    public void EnsureCanBeSold_ThrowsWhenProductIsInactive()
    {
        var product = CreateProduct(currentStock: 10);
        product.Deactivate(ReferenceDate);

        var exception = Assert.Throws<ProductInactiveException>(() => product.EnsureCanBeSold(1, ReferenceDate));

        Assert.Contains("inactive", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyInventoryMovement_ThrowsWhenStockWouldBeNegative()
    {
        var product = CreateProduct(currentStock: 2, minimumStock: 1);

        Assert.Throws<InsufficientStockException>(() => product.ApplyInventoryMovement(InventoryMovementType.Exit, 3, ReferenceDate));
    }

    [Fact]
    public void UpdateCatalogDetails_ThrowsWhenSalePriceIsBelowPurchasePrice()
    {
        var product = CreateProduct(currentStock: 5);

        Assert.Throws<ValidationDomainException>(() => product.UpdateCatalogDetails(
            Guid.NewGuid(),
            "Painkiller",
            "PK-1",
            null,
            10m,
            9m,
            1,
            null,
            true,
            ReferenceDate));
    }

    [Fact]
    public void Create_AndUpdateCatalogDetails_KeepMutationInsideDomain()
    {
        var originalCategory = new Category { Name = "Medicine" };
        var updatedCategory = new Category { Name = "Supplements" };
        var product = Product.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "  Painkiller  ",
            "  PK-1  ",
            "  Fast relief  ",
            5m,
            8m,
            6,
            2,
            ReferenceDate.AddDays(30),
            originalCategory);

        product.UpdateCatalogDetails(
            Guid.NewGuid(),
            "  Vitamin C  ",
            "  VC-1  ",
            "  Daily use  ",
            4m,
            9m,
            3,
            ReferenceDate.AddDays(45),
            false,
            ReferenceDate,
            updatedCategory);

        Assert.Equal("Vitamin C", product.Name);
        Assert.Equal("VC-1", product.InternalCode);
        Assert.Equal("Daily use", product.Description);
        Assert.Equal(4m, product.PurchasePrice);
        Assert.Equal(9m, product.SalePrice);
        Assert.Equal(3, product.MinimumStock);
        Assert.Equal(updatedCategory, product.Category);
        Assert.False(product.IsActive);
        Assert.Equal(ReferenceDate, product.UpdatedAt);
    }

    private static Product CreateProduct(
        decimal purchasePrice = 2m,
        decimal salePrice = 3m,
        int currentStock = 5,
        int minimumStock = 1,
        DateTime? expirationDate = null)
        => Product.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Painkiller",
            $"PK-{Guid.NewGuid():N}",
            null,
            purchasePrice,
            salePrice,
            currentStock,
            minimumStock,
            expirationDate);
}
