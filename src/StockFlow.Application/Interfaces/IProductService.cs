using StockFlow.Application.DTOs.Products;

namespace StockFlow.Application.Interfaces;

public interface IProductService
{
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductResponse>> GetAllAsync(Guid? categoryId = null, CancellationToken cancellationToken = default);
    Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductResponse>> SearchAsync(string term, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductResponse>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductResponse>> GetExpiringSoonAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductResponse>> GetExpiredAsync(CancellationToken cancellationToken = default);
    Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
