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

        product.ApplyInventoryMovement(request.MovementType, request.Quantity, _dateTimeProvider.UtcNow);

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

    public async Task<PagedResponse<InventoryMovementResponse>> GetMovementsAsync(PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
    {
        var movements = await _inventoryMovementRepository.GetByBusinessPagedAsync(_userContext.BusinessId, paginationQuery, cancellationToken);
        return movements.ToPagedResponse(movement => movement.ToResponse());
    }

    public async Task<PagedResponse<InventoryMovementResponse>> GetProductHistoryAsync(Guid productId, PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
    {
        var movements = await _inventoryMovementRepository.GetProductHistoryPagedAsync(_userContext.BusinessId, productId, paginationQuery, cancellationToken);
        return movements.ToPagedResponse(movement => movement.ToResponse());
    }
}
