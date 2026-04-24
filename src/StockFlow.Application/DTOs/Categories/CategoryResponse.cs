namespace StockFlow.Application.DTOs.Categories;

public sealed record CategoryResponse(Guid Id, Guid BusinessId, string Name, string? Description, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);
