using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Products;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Exceptions;

namespace StockFlow.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, IUserContext userContext, IDateTimeProvider dateTimeProvider)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _userContext = userContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var category = await ValidateCategoryAsync(request.CategoryId, cancellationToken);
        await EnsureInternalCodeAvailableAsync(request.InternalCode, null, cancellationToken);
        ValidateProductRules(request.PurchasePrice, request.SalePrice, request.CurrentStock, request.MinimumStock, request.ExpirationDate);

        var product = new Product
        {
            BusinessId = _userContext.BusinessId,
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            InternalCode = request.InternalCode.Trim(),
            Description = request.Description?.Trim(),
            PurchasePrice = request.PurchasePrice,
            SalePrice = request.SalePrice,
            CurrentStock = request.CurrentStock,
            MinimumStock = request.MinimumStock,
            ExpirationDate = request.ExpirationDate,
            IsActive = true,
            Category = category
        };

        await _productRepository.AddAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);
        return product.ToResponse();
    }

    public async Task<IReadOnlyCollection<ProductResponse>> GetAllAsync(Guid? categoryId = null, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(_userContext.BusinessId, categoryId, cancellationToken);
        return products.Select(product => product.ToResponse()).ToArray();
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return (await GetEntityAsync(id, cancellationToken)).ToResponse();
    }

    public async Task<IReadOnlyCollection<ProductResponse>> SearchAsync(string term, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.SearchAsync(_userContext.BusinessId, term.Trim(), cancellationToken);
        return products.Select(product => product.ToResponse()).ToArray();
    }

    public async Task<IReadOnlyCollection<ProductResponse>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetLowStockAsync(_userContext.BusinessId, cancellationToken);
        return products.Select(product => product.ToResponse()).ToArray();
    }

    public async Task<IReadOnlyCollection<ProductResponse>> GetExpiringSoonAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        if (days <= 0)
        {
            throw new ValidationDomainException("The days parameter must be greater than zero.");
        }

        var limitDate = _dateTimeProvider.UtcNow.Date.AddDays(days);
        var products = await _productRepository.GetExpiringSoonAsync(_userContext.BusinessId, limitDate, cancellationToken);
        return products.Select(product => product.ToResponse()).ToArray();
    }

    public async Task<IReadOnlyCollection<ProductResponse>> GetExpiredAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetExpiredAsync(_userContext.BusinessId, _dateTimeProvider.UtcNow.Date, cancellationToken);
        return products.Select(product => product.ToResponse()).ToArray();
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await GetEntityAsync(id, cancellationToken);
        var category = await ValidateCategoryAsync(request.CategoryId, cancellationToken);
        await EnsureInternalCodeAvailableAsync(request.InternalCode, id, cancellationToken);
        ValidateProductRules(request.PurchasePrice, request.SalePrice, product.CurrentStock, request.MinimumStock, request.ExpirationDate);

        product.CategoryId = request.CategoryId;
        product.Name = request.Name.Trim();
        product.InternalCode = request.InternalCode.Trim();
        product.Description = request.Description?.Trim();
        product.PurchasePrice = request.PurchasePrice;
        product.SalePrice = request.SalePrice;
        product.MinimumStock = request.MinimumStock;
        product.ExpirationDate = request.ExpirationDate;
        product.IsActive = request.IsActive;
        product.Category = category;
        product.UpdatedAt = _dateTimeProvider.UtcNow;

        await _productRepository.SaveChangesAsync(cancellationToken);
        return product.ToResponse();
    }

    public async Task<ProductResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await GetEntityAsync(id, cancellationToken);
        product.IsActive = false;
        product.UpdatedAt = _dateTimeProvider.UtcNow;
        await _productRepository.SaveChangesAsync(cancellationToken);
        return product.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await GetEntityAsync(id, cancellationToken);
        if (await _productRepository.HasHistoryAsync(id, _userContext.BusinessId, cancellationToken))
        {
            product.IsActive = false;
            product.UpdatedAt = _dateTimeProvider.UtcNow;
        }
        else
        {
            _productRepository.Remove(product);
        }

        await _productRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<StockFlow.Domain.Entities.Category> ValidateCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, _userContext.BusinessId, cancellationToken);
        if (category is null)
        {
            throw new EntityNotFoundException("Category was not found for the current business.");
        }

        return category;
    }

    private async Task EnsureInternalCodeAvailableAsync(string internalCode, Guid? excludeId, CancellationToken cancellationToken)
    {
        if (await _productRepository.ExistsInternalCodeAsync(_userContext.BusinessId, internalCode.Trim(), excludeId, cancellationToken))
        {
            throw new DuplicateInternalCodeException("The internal code already exists in this business.");
        }
    }

    private static void ValidateProductRules(decimal purchasePrice, decimal salePrice, int currentStock, int minimumStock, DateTime? expirationDate)
    {
        if (salePrice < purchasePrice)
        {
            throw new ValidationDomainException("Sale price must be greater than or equal to purchase price.");
        }

        if (currentStock < 0 || minimumStock < 0)
        {
            throw new ValidationDomainException("Stock values cannot be negative.");
        }

        if (expirationDate.HasValue && expirationDate.Value.Year < 2000)
        {
            throw new ValidationDomainException("Expiration date is invalid.");
        }
    }

    private async Task<Product> GetEntityAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _productRepository.GetByIdAsync(id, _userContext.BusinessId, cancellationToken)
            ?? throw new EntityNotFoundException("Product was not found.");
    }
}
