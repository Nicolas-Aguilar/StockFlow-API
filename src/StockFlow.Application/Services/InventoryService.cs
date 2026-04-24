using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Inventory;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Enums;
using StockFlow.Domain.Exceptions;

namespace StockFlow.Application.Services;

public sealed class InventoryService : IInventoryService
{
    private readonly IInventoryMovementRepository _inventoryMovementRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public InventoryService(
        IInventoryMovementRepository inventoryMovementRepository,
        IProductRepository productRepository,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider)
    {
        _inventoryMovementRepository = inventoryMovementRepository;
        _productRepository = productRepository;
        _userContext = userContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<InventoryMovementResponse> CreateMovementAsync(CreateInventoryMovementRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, _userContext.BusinessId, cancellationToken)
            ?? throw new EntityNotFoundException("Product was not found.");

        if (request.MovementType == InventoryMovementType.Sale)
        {
            throw new ValidationDomainException("Sale movements are created automatically by sales.");
        }

        if ((request.MovementType == InventoryMovementType.Exit || request.MovementType == InventoryMovementType.Adjustment) && string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ValidationDomainException("Reason is required for exit and adjustment movements.");
        }

        if (request.MovementType != InventoryMovementType.Entry && product.CurrentStock < request.Quantity)
        {
            throw new InsufficientStockException("The product does not have enough stock.");
        }

        product.CurrentStock += request.MovementType == InventoryMovementType.Entry ? request.Quantity : -request.Quantity;
        product.UpdatedAt = _dateTimeProvider.UtcNow;

        var movement = new InventoryMovement
        {
            BusinessId = _userContext.BusinessId,
            ProductId = product.Id,
            MovementType = request.MovementType,
            Quantity = request.Quantity,
            Reason = request.Reason?.Trim(),
            CreatedAt = _dateTimeProvider.UtcNow
        };

        await _inventoryMovementRepository.AddAsync(movement, cancellationToken);
        await _inventoryMovementRepository.SaveChangesAsync(cancellationToken);
        movement.Product = product;
        return movement.ToResponse();
    }

    public async Task<IReadOnlyCollection<InventoryMovementResponse>> GetMovementsAsync(CancellationToken cancellationToken = default)
    {
        var movements = await _inventoryMovementRepository.GetByBusinessAsync(_userContext.BusinessId, cancellationToken);
        return movements.Select(movement => movement.ToResponse()).ToArray();
    }

    public async Task<IReadOnlyCollection<InventoryMovementResponse>> GetProductHistoryAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var movements = await _inventoryMovementRepository.GetProductHistoryAsync(_userContext.BusinessId, productId, cancellationToken);
        return movements.Select(movement => movement.ToResponse()).ToArray();
    }
}
