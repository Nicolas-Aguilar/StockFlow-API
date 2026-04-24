namespace StockFlow.Application.DTOs.Products;

public sealed record ProductResponse(
    Guid Id,
    Guid BusinessId,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string InternalCode,
    string? Description,
    decimal PurchasePrice,
    decimal SalePrice,
    decimal ProfitPerUnit,
    decimal ProfitMarginPercentage,
    int CurrentStock,
    int MinimumStock,
    bool IsLowStock,
    DateTime? ExpirationDate,
    bool IsExpired,
    int? DaysUntilExpiration,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
