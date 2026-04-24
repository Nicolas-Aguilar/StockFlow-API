using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Sales;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;
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
            var products = await _productRepository.GetAllAsync(_userContext.BusinessId, null, ct);
            var productMap = products.Where(product => productIds.Contains(product.Id)).ToDictionary(product => product.Id);

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

                var subtotal = product.SalePrice * item.Quantity;
                var estimatedProfit = (product.SalePrice - product.PurchasePrice) * item.Quantity;

                sale.Items.Add(new SaleItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.SalePrice,
                    UnitPurchasePrice = product.PurchasePrice,
                    Subtotal = subtotal,
                    EstimatedProfit = estimatedProfit,
                    Product = product
                });

                sale.Total += subtotal;
                sale.EstimatedProfit += estimatedProfit;
                product.CurrentStock -= item.Quantity;
                product.UpdatedAt = _dateTimeProvider.UtcNow;
            }

            await _saleRepository.AddAsync(sale, ct);
            await _inventoryMovementRepository.AddRangeAsync(sale.Items.Select(item => new InventoryMovement
            {
                BusinessId = _userContext.BusinessId,
                ProductId = item.ProductId,
                MovementType = StockFlow.Domain.Enums.InventoryMovementType.Sale,
                Quantity = item.Quantity,
                Reason = "Created by sale",
                CreatedAt = _dateTimeProvider.UtcNow
            }), ct);

            createdSale = sale;
        }, cancellationToken);

        return (await GetByIdAsync(createdSale!.Id, cancellationToken));
    }

    public async Task<IReadOnlyCollection<SaleResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sales = await _saleRepository.GetAllAsync(_userContext.BusinessId, cancellationToken);
        return sales.Select(sale => sale.ToResponse()).ToArray();
    }

    public async Task<SaleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await _saleRepository.GetByIdAsync(id, _userContext.BusinessId, cancellationToken)
            ?? throw new EntityNotFoundException("Sale was not found.");
        return sale.ToResponse();
    }

    public async Task<IReadOnlyCollection<SaleResponse>> GetByDateRangeAsync(DateRangeQuery query, CancellationToken cancellationToken = default)
    {
        if (query.From > query.To)
        {
            throw new ValidationDomainException("The from date must be less than or equal to the to date.");
        }

        var fromUtc = query.From.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = query.To.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        var sales = await _saleRepository.GetByDateRangeAsync(_userContext.BusinessId, fromUtc, toUtc, cancellationToken);
        return sales.Select(sale => sale.ToResponse()).ToArray();
    }

    private void ValidateProductAvailability(Product product, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ValidationDomainException("Sale item quantity must be greater than zero.");
        }

        if (!product.IsActive)
        {
            throw new ProductInactiveException($"Product '{product.Name}' is inactive and cannot be sold.");
        }

        if (product.IsExpired)
        {
            throw new ProductExpiredException($"Product '{product.Name}' is expired and cannot be sold.");
        }

        if (product.CurrentStock < quantity)
        {
            throw new InsufficientStockException($"Product '{product.Name}' does not have enough stock.");
        }
    }
}
