using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Categories;

namespace StockFlow.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<CategoryResponse>> GetAllAsync(PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task<CategoryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<CategoryResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
