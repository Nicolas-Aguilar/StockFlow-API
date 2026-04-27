using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Sales;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Enums;
using StockFlow.Domain.Exceptions;

namespace StockFlow.Application.Services;

public sealed class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryMovementRepository _inventoryMovementRepository;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SaleService(
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        IInventoryMovementRepository inventoryMovementRepository,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _inventoryMovementRepository = inventoryMovementRepository;
        _userContext = userContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<SaleResponse> CreateAsync(CreateSaleRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new ValidationDomainException("A sale must have at least one product.");
        }

        Sale? createdSale = null;

        await _saleRepository.ExecuteInTransactionAsync(async ct =>
        {
            var productIds = request.Items.Select(item => item.ProductId).Distinct().ToArray();
            var products = await _productRepository.GetByIdsAsync(_userContext.BusinessId, productIds, ct);
            var productMap = products.ToDictionary(product => product.Id);

            if (productMap.Count != productIds.Length)
            {
                throw new EntityNotFoundException("One or more sale products were not found for this business.");
            }

            var sale = new Sale
            {
                BusinessId = _userContext.BusinessId,
                PaymentMethod = request.PaymentMethod,
                CreatedAt = _dateTimeProvider.UtcNow
            };

            foreach (var item in request.Items)
            {
                var product = productMap[item.ProductId];
                ValidateProductAvailability(product, item.Quantity);

                var stockUpdatedAt = _dateTimeProvider.UtcNow;
                var stockUpdated = await _productRepository.TryUpdateStockAsync(product.Id, _userContext.BusinessId, -item.Quantity, stockUpdatedAt, ct);

                if (!stockUpdated)
                {
                    throw new InsufficientStockException($"Product '{product.Name}' does not have enough stock.");
                }

                product.ApplyInventoryMovement(InventoryMovementType.Sale, item.Quantity, stockUpdatedAt);

                var subtotal = product.SalePrice * item.Quantity;
                var estimatedProfit = (product.SalePrice - product.PurchasePrice) * item.Quantity;

                sale.Items.Add(new SaleItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.SalePrice,
                    UnitPurchasePrice = product.PurchasePrice,
                    Subtotal = subtotal,
                    EstimatedProfit = estimatedProfit
                });

                sale.Total += subtotal;
                sale.EstimatedProfit += estimatedProfit;
            }

            await _saleRepository.AddAsync(sale, ct);
            await _inventoryMovementRepository.AddRangeAsync(sale.Items.Select(item => new InventoryMovement
            {
                BusinessId = _userContext.BusinessId,
                ProductId = item.ProductId,
                MovementType = InventoryMovementType.Sale,
                Quantity = item.Quantity,
                Reason = "Created by sale",
                CreatedAt = _dateTimeProvider.UtcNow
            }), ct);

            createdSale = sale;
        }, cancellationToken);

        return (await GetByIdAsync(createdSale!.Id, cancellationToken));
    }

    public async Task<PagedResponse<SaleResponse>> GetAllAsync(PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
    {
        var sales = await _saleRepository.GetPagedAsync(_userContext.BusinessId, paginationQuery, cancellationToken);
        return sales.ToPagedResponse(sale => sale.ToResponse());
    }

    public async Task<SaleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await _saleRepository.GetByIdAsync(id, _userContext.BusinessId, cancellationToken)
            ?? throw new EntityNotFoundException("Sale was not found.");
        return sale.ToResponse();
    }

    public async Task<PagedResponse<SaleResponse>> GetByDateRangeAsync(DateRangeQuery query, PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
    {
        if (query.From > query.To)
        {
            throw new ValidationDomainException("The from date must be less than or equal to the to date.");
        }

        var fromUtc = query.From.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = query.To.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        var sales = await _saleRepository.GetByDateRangePagedAsync(_userContext.BusinessId, fromUtc, toUtc, paginationQuery, cancellationToken);
        return sales.ToPagedResponse(sale => sale.ToResponse());
    }

    private void ValidateProductAvailability(Product product, int quantity)
    {
        product.EnsureCanBeSold(quantity, _dateTimeProvider.UtcNow);
    }
}
