namespace StockFlow.Application.DTOs.Reports;

public sealed record SalesSummaryResponse(DateOnly From, DateOnly To, int TotalSales, decimal TotalRevenue);
