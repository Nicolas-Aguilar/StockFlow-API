using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;
using StockFlow.Infrastructure.Data;

namespace StockFlow.Infrastructure.Repositories;

public sealed class BusinessRepository : IBusinessRepository
{
    private readonly AppDbContext _dbContext;

    public BusinessRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<Business?> GetByOwnerUserIdAsync(Guid ownerUserId, CancellationToken cancellationToken = default) => _dbContext.Businesses.AsNoTracking().FirstOrDefaultAsync(business => business.OwnerUserId == ownerUserId, cancellationToken);

    public Task<Business?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => _dbContext.Businesses.FirstOrDefaultAsync(business => business.Id == id, cancellationToken);

    public Task AddAsync(Business business, CancellationToken cancellationToken = default) => _dbContext.Businesses.AddAsync(business, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _dbContext.SaveChangesAsync(cancellationToken);
}
