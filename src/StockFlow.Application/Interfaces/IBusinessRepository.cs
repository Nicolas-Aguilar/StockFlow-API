using StockFlow.Domain.Entities;

namespace StockFlow.Application.Interfaces;

public interface IBusinessRepository
{
    Task<Business?> GetByOwnerUserIdAsync(Guid ownerUserId, CancellationToken cancellationToken = default);
    Task<Business?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Business business, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
