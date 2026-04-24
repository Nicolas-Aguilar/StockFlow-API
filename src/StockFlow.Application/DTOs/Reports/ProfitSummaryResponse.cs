namespace StockFlow.Application.DTOs.Reports;

public sealed record ProfitSummaryResponse(DateOnly From, DateOnly To, int TotalSales, decimal EstimatedProfit);
