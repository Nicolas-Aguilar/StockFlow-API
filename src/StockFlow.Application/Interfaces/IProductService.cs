using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Products;

namespace StockFlow.Application.Interfaces;

public interface IProductService
{
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<ProductResponse>> GetAllAsync(PaginationQuery paginationQuery, Guid? categoryId = null, CancellationToken cancellationToken = default);
    Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<ProductResponse>> SearchAsync(string term, PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task<PagedResponse<ProductResponse>> GetLowStockAsync(PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task<PagedResponse<ProductResponse>> GetExpiringSoonAsync(PaginationQuery paginationQuery, int days = 30, CancellationToken cancellationToken = default);
    Task<PagedResponse<ProductResponse>> GetExpiredAsync(PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
