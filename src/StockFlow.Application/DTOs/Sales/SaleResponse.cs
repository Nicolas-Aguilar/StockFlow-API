using StockFlow.Domain.Enums;

namespace StockFlow.Application.DTOs.Sales;

public sealed record SaleResponse(Guid Id, Guid BusinessId, decimal Total, decimal EstimatedProfit, PaymentMethod PaymentMethod, DateTime CreatedAt, IReadOnlyCollection<SaleItemResponse> Items);
