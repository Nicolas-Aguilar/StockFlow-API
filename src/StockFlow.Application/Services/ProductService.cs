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

        var product = Product.Create(
            _userContext.BusinessId,
            request.CategoryId,
            request.Name,
            request.InternalCode,
            request.Description,
            request.PurchasePrice,
            request.SalePrice,
            request.CurrentStock,
            request.MinimumStock,
            request.ExpirationDate,
            category);

        await _productRepository.AddAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);
        return product.ToResponse(_dateTimeProvider.UtcNow);
    }

    public async Task<PagedResponse<ProductResponse>> GetAllAsync(PaginationQuery paginationQuery, Guid? categoryId = null, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetPagedAsync(_userContext.BusinessId, paginationQuery, categoryId, cancellationToken);
        var currentDateUtc = _dateTimeProvider.UtcNow;
        return products.ToPagedResponse(product => product.ToResponse(currentDateUtc));
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return (await GetEntityAsync(id, cancellationToken)).ToResponse(_dateTimeProvider.UtcNow);
    }

    public async Task<PagedResponse<ProductResponse>> SearchAsync(string term, PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
    {
        var normalizedTerm = term.Trim();

        if (string.IsNullOrWhiteSpace(normalizedTerm))
        {
            throw new ValidationDomainException("The search term must not be blank.");
        }

        var products = await _productRepository.SearchPagedAsync(_userContext.BusinessId, normalizedTerm, paginationQuery, cancellationToken);
        var currentDateUtc = _dateTimeProvider.UtcNow;
        return products.ToPagedResponse(product => product.ToResponse(currentDateUtc));
    }

    public async Task<PagedResponse<ProductResponse>> GetLowStockAsync(PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetLowStockPagedAsync(_userContext.BusinessId, paginationQuery, cancellationToken);
        var currentDateUtc = _dateTimeProvider.UtcNow;
        return products.ToPagedResponse(product => product.ToResponse(currentDateUtc));
    }

    public async Task<PagedResponse<ProductResponse>> GetExpiringSoonAsync(PaginationQuery paginationQuery, int days = 30, CancellationToken cancellationToken = default)
    {
        if (days <= 0)
        {
            throw new ValidationDomainException("The days parameter must be greater than zero.");
        }

        var currentDateUtc = _dateTimeProvider.UtcNow;
        var limitDate = currentDateUtc.Date.AddDays(days);
        var products = await _productRepository.GetExpiringSoonPagedAsync(_userContext.BusinessId, currentDateUtc.Date, limitDate, paginationQuery, cancellationToken);
        return products.ToPagedResponse(product => product.ToResponse(currentDateUtc));
    }

    public async Task<PagedResponse<ProductResponse>> GetExpiredAsync(PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
    {
        var currentDateUtc = _dateTimeProvider.UtcNow;
        var products = await _productRepository.GetExpiredPagedAsync(_userContext.BusinessId, currentDateUtc.Date, paginationQuery, cancellationToken);
        return products.ToPagedResponse(product => product.ToResponse(currentDateUtc));
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await GetEntityAsync(id, cancellationToken);
        var category = await ValidateCategoryAsync(request.CategoryId, cancellationToken);
        await EnsureInternalCodeAvailableAsync(request.InternalCode, id, cancellationToken);
        var updatedAtUtc = _dateTimeProvider.UtcNow;

        product.UpdateCatalogDetails(
            request.CategoryId,
            request.Name,
            request.InternalCode,
            request.Description,
            request.PurchasePrice,
            request.SalePrice,
            request.MinimumStock,
            request.ExpirationDate,
            request.IsActive,
            updatedAtUtc,
            category);

        await _productRepository.SaveChangesAsync(cancellationToken);
        return product.ToResponse(updatedAtUtc);
    }

    public async Task<ProductResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await GetEntityAsync(id, cancellationToken);
        var updatedAtUtc = _dateTimeProvider.UtcNow;
        product.Deactivate(updatedAtUtc);
        await _productRepository.SaveChangesAsync(cancellationToken);
        return product.ToResponse(updatedAtUtc);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await GetEntityAsync(id, cancellationToken);
        if (await _productRepository.HasHistoryAsync(id, _userContext.BusinessId, cancellationToken))
        {
            product.Deactivate(_dateTimeProvider.UtcNow);
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

    private async Task<Product> GetEntityAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _productRepository.GetByIdAsync(id, _userContext.BusinessId, cancellationToken)
            ?? throw new EntityNotFoundException("Product was not found.");
    }
}
