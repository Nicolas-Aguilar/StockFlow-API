using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Enums;
using StockFlow.Infrastructure.Data;

namespace StockFlow.Infrastructure.Bootstrap;

public sealed class DemoDataSeeder
{
    public const string DemoEmail = "demo@stockflow.local";
    public const string DemoPassword = "Demo12345!";
    public const string DemoBusinessName = "StockFlow Demo Store";

    private readonly AppDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<DemoDataSeeder> _logger;

    public DemoDataSeeder(
        AppDbContext dbContext,
        IPasswordService passwordService,
        IDateTimeProvider dateTimeProvider,
        ILogger<DemoDataSeeder> logger)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var normalizedEmail = DemoEmail.ToLowerInvariant();
        if (await _dbContext.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken))
        {
            _logger.LogInformation("Demo data already exists. Skipping seed.");
            return;
        }

        var now = _dateTimeProvider.UtcNow;

        var user = new User
        {
            FullName = "Demo Owner",
            Email = normalizedEmail,
            IsActive = true
        };
        user.PasswordHash = _passwordService.HashPassword(user, DemoPassword);

        var business = new Business
        {
            OwnerUserId = user.Id,
            Name = DemoBusinessName,
            Description = "Demo business for local Docker onboarding.",
            IsActive = true
        };

        var beverages = CreateCategory(business.Id, "Beverages", "Cold drinks and refreshment products.");
        var snacks = CreateCategory(business.Id, "Snacks", "Fast-moving snacks for demo sales.");
        var household = CreateCategory(business.Id, "Household", "Basic home supplies.");

        var cola = Product.Create(business.Id, beverages.Id, "Cola 600ml", "BEV-001", "Popular carbonated drink.", 0.85m, 1.50m, 36, 8, null, beverages);
        var orangeJuice = Product.Create(business.Id, beverages.Id, "Orange Juice 1L", "BEV-002", "Orange juice bottle.", 1.10m, 2.10m, 18, 6, now.Date.AddDays(20), beverages);
        var potatoChips = Product.Create(business.Id, snacks.Id, "Potato Chips", "SNK-001", "Salted chips bag.", 0.60m, 1.25m, 28, 10, null, snacks);
        var chocolateCookies = Product.Create(business.Id, snacks.Id, "Chocolate Cookies", "SNK-002", "Family-size cookie pack.", 0.95m, 1.90m, 22, 7, now.Date.AddDays(45), snacks);
        var dishSoap = Product.Create(business.Id, household.Id, "Dish Soap", "HOU-001", "500ml dish soap bottle.", 1.40m, 2.60m, 14, 5, null, household);

        var sales = new List<Sale>
        {
            CreateSale(business.Id, now.AddDays(-3), PaymentMethod.Card, (cola, 4), (potatoChips, 3)),
            CreateSale(business.Id, now.AddDays(-2), PaymentMethod.Cash, (orangeJuice, 2), (dishSoap, 1)),
            CreateSale(business.Id, now.AddDays(-1), PaymentMethod.Transfer, (chocolateCookies, 2), (cola, 1))
        };

        var movements = sales
            .SelectMany(sale => sale.Items.Select(item => new InventoryMovement
            {
                BusinessId = business.Id,
                ProductId = item.ProductId,
                MovementType = InventoryMovementType.Sale,
                Quantity = item.Quantity,
                Reason = "Created by demo seed",
                CreatedAt = sale.CreatedAt
            }))
            .ToList();

        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.Businesses.AddAsync(business, cancellationToken);
        await _dbContext.Categories.AddRangeAsync(new[] { beverages, snacks, household }, cancellationToken);
        await _dbContext.Products.AddRangeAsync(new[] { cola, orangeJuice, potatoChips, chocolateCookies, dishSoap }, cancellationToken);
        await _dbContext.Sales.AddRangeAsync(sales, cancellationToken);
        await _dbContext.InventoryMovements.AddRangeAsync(movements, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Demo data created for {Email}.", DemoEmail);
    }

    private static Category CreateCategory(Guid businessId, string name, string description)
        => new()
        {
            BusinessId = businessId,
            Name = name,
            Description = description,
            IsActive = true
        };

    private static Sale CreateSale(Guid businessId, DateTime createdAtUtc, PaymentMethod paymentMethod, params (Product Product, int Quantity)[] items)
    {
        var sale = new Sale
        {
            BusinessId = businessId,
            PaymentMethod = paymentMethod,
            CreatedAt = createdAtUtc
        };

        foreach (var (product, quantity) in items)
        {
            product.EnsureCanBeSold(quantity, createdAtUtc);
            product.ApplyInventoryMovement(InventoryMovementType.Sale, quantity, createdAtUtc);

            var subtotal = product.SalePrice * quantity;
            var estimatedProfit = (product.SalePrice - product.PurchasePrice) * quantity;

            sale.Items.Add(new SaleItem
            {
                ProductId = product.Id,
                Quantity = quantity,
                UnitPrice = product.SalePrice,
                UnitPurchasePrice = product.PurchasePrice,
                Subtotal = subtotal,
                EstimatedProfit = estimatedProfit
            });

            sale.Total += subtotal;
            sale.EstimatedProfit += estimatedProfit;
        }

        return sale;
    }
}
