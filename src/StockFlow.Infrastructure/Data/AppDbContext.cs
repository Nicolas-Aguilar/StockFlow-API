using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Common;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, IDateTimeProvider dateTimeProvider) : base(options)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = _dateTimeProvider.UtcNow;

        var entries = ChangeTracker.Entries<AuditableEntity>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.UpdatedAt = utcNow;
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
            }
        }

        SetCreatedAtIfMissing(ChangeTracker.Entries<Sale>(), utcNow);
        SetCreatedAtIfMissing(ChangeTracker.Entries<InventoryMovement>(), utcNow);

        return base.SaveChangesAsync(cancellationToken);
    }

    private static void SetCreatedAtIfMissing<T>(IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T>> entries, DateTime utcNow)
        where T : class
    {
        foreach (var entry in entries.Where(entry => entry.State == EntityState.Added))
        {
            var createdAtProperty = entry.Property(nameof(Sale.CreatedAt));

            if (createdAtProperty.CurrentValue is DateTime createdAt && createdAt == default)
            {
                createdAtProperty.CurrentValue = utcNow;
            }
        }
    }
}
