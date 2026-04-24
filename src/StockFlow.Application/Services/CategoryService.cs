using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Categories;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Exceptions;

namespace StockFlow.Application.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserContext _userContext;

    public CategoryService(ICategoryRepository categoryRepository, IUserContext userContext)
    {
        _categoryRepository = categoryRepository;
        _userContext = userContext;
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureNameAvailableAsync(request.Name, null, cancellationToken);
        var category = new Category
        {
            BusinessId = _userContext.BusinessId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsActive = true
        };

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
        return category.ToResponse();
    }

    public async Task<IReadOnlyCollection<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllAsync(_userContext.BusinessId, cancellationToken);
        return categories.Select(category => category.ToResponse()).ToArray();
    }

    public async Task<CategoryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await GetCategoryAsync(id, cancellationToken);
        return category.ToResponse();
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await GetCategoryAsync(id, cancellationToken);
        await EnsureNameAvailableAsync(request.Name, id, cancellationToken);
        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();
        category.UpdatedAt = DateTime.UtcNow;
        await _categoryRepository.SaveChangesAsync(cancellationToken);
        return category.ToResponse();
    }

    public async Task<CategoryResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await GetCategoryAsync(id, cancellationToken);
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;
        await _categoryRepository.SaveChangesAsync(cancellationToken);
        return category.ToResponse();
    }

    private async Task EnsureNameAvailableAsync(string name, Guid? excludeId, CancellationToken cancellationToken)
    {
        if (await _categoryRepository.ExistsByNameAsync(_userContext.BusinessId, name.Trim(), excludeId, cancellationToken))
        {
            throw new ValidationDomainException("A category with the same name already exists in this business.");
        }
    }

    private async Task<Category> GetCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _categoryRepository.GetByIdAsync(id, _userContext.BusinessId, cancellationToken)
            ?? throw new EntityNotFoundException("Category was not found.");
    }
}
