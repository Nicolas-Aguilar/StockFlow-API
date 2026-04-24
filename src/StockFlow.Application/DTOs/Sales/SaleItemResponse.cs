namespace StockFlow.Application.DTOs.Sales;

public sealed record SaleItemResponse(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal UnitPurchasePrice, decimal Subtotal, decimal EstimatedProfit);
