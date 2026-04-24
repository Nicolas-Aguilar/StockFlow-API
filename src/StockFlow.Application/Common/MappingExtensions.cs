using StockFlow.Application.DTOs.Auth;
using StockFlow.Application.DTOs.Businesses;
using StockFlow.Application.DTOs.Categories;
using StockFlow.Application.DTOs.Inventory;
using StockFlow.Application.DTOs.Products;
using StockFlow.Application.DTOs.Sales;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Common;

public static class MappingExtensions
{
    public static UserProfileResponse ToProfileResponse(this User user) => new(user.Id, user.FullName, user.Email, user.IsActive, user.CreatedAt, user.UpdatedAt);

    public static BusinessResponse ToResponse(this Business business) => new(business.Id, business.OwnerUserId, business.Name, business.Description, business.IsActive, business.CreatedAt, business.UpdatedAt);

    public static CategoryResponse ToResponse(this Category category) => new(category.Id, category.BusinessId, category.Name, category.Description, category.IsActive, category.CreatedAt, category.UpdatedAt);

    public static ProductResponse ToResponse(this Product product) => new(
        product.Id,
        product.BusinessId,
        product.CategoryId,
        product.Category?.Name ?? string.Empty,
        product.Name,
        product.InternalCode,
        product.Description,
        product.PurchasePrice,
        product.SalePrice,
        product.ProfitPerUnit,
        product.ProfitMarginPercentage,
        product.CurrentStock,
        product.MinimumStock,
        product.IsLowStock,
        product.ExpirationDate,
        product.IsExpired,
        product.DaysUntilExpiration,
        product.IsActive,
        product.CreatedAt,
        product.UpdatedAt);

    public static InventoryMovementResponse ToResponse(this InventoryMovement movement) => new(
        movement.Id,
        movement.BusinessId,
        movement.ProductId,
        movement.Product?.Name ?? string.Empty,
        movement.MovementType,
        movement.Quantity,
        movement.Reason,
        movement.CreatedAt);

    public static SaleResponse ToResponse(this Sale sale) => new(
        sale.Id,
        sale.BusinessId,
        sale.Total,
        sale.EstimatedProfit,
        sale.PaymentMethod,
        sale.CreatedAt,
        sale.Items.Select(item => new SaleItemResponse(
            item.ProductId,
            item.Product?.Name ?? string.Empty,
            item.Quantity,
            item.UnitPrice,
            item.UnitPurchasePrice,
            item.Subtotal,
            item.EstimatedProfit)).ToArray());
}
