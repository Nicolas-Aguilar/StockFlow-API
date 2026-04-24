using StockFlow.Domain.Common;

namespace StockFlow.Domain.Entities;

public sealed class SaleItem : Entity
{
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitPurchasePrice { get; set; }
    public decimal Subtotal { get; set; }
    public decimal EstimatedProfit { get; set; }

    public Sale? Sale { get; set; }
    public Product? Product { get; set; }
}
