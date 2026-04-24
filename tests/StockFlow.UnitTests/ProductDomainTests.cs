using StockFlow.Domain.Entities;

namespace StockFlow.UnitTests;

public sealed class ProductDomainTests
{
    [Fact]
    public void ComputedFields_ReturnExpectedValues()
    {
        var product = new Product
        {
            Name = "Painkiller",
            PurchasePrice = 5m,
            SalePrice = 8m,
            CurrentStock = 4,
            MinimumStock = 5,
            ExpirationDate = DateTime.UtcNow.Date.AddDays(10)
        };

        Assert.Equal(3m, product.ProfitPerUnit);
        Assert.Equal(37.5m, product.ProfitMarginPercentage);
        Assert.True(product.IsLowStock);
        Assert.False(product.IsExpired);
    }

    [Fact]
    public void ExpiredProduct_IsMarkedAsExpired()
    {
        var product = new Product
        {
            ExpirationDate = DateTime.UtcNow.Date.AddDays(-1)
        };

        Assert.True(product.IsExpired);
        Assert.True(product.DaysUntilExpiration < 0);
    }
}
