namespace StockFlow.Application.DTOs.Reports;

public sealed record InventoryValuationResponse(int TotalProducts, decimal PurchaseValue, decimal PotentialSaleValue, decimal PotentialProfit);
