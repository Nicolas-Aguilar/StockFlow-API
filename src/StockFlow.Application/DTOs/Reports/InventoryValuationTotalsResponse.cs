namespace StockFlow.Application.DTOs.Reports;

public sealed record InventoryValuationTotalsResponse(int TotalProducts, decimal PurchaseValue, decimal PotentialSaleValue);
