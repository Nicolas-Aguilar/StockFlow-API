namespace StockFlow.Application.DTOs.Businesses;

public sealed record BusinessResponse(Guid Id, Guid OwnerUserId, string Name, string? Description, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);
