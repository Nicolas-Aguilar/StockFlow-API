namespace StockFlow.Application.DTOs.Reports;

public sealed record TopSellingProductResponse(Guid ProductId, string ProductName, string InternalCode, int TotalUnitsSold, decimal TotalRevenue);
