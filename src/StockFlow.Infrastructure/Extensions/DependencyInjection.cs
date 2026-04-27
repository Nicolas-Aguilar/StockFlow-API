using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StockFlow.Application.Interfaces;
using StockFlow.Application.Services;
using StockFlow.Infrastructure.Data;
using StockFlow.Infrastructure.Repositories;
using StockFlow.Infrastructure.Services;

namespace StockFlow.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        EnsureSecretConfigured("ConnectionStrings:DefaultConnection", connectionString);

        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = new JwtOptions
        {
            Issuer = jwtSection["Issuer"] ?? string.Empty,
            Audience = jwtSection["Audience"] ?? string.Empty,
            Key = jwtSection["Key"] ?? string.Empty,
            ExpirationMinutes = int.TryParse(jwtSection["ExpirationMinutes"], out var expirationMinutes) ? expirationMinutes : 120
        };
        services.AddSingleton(Options.Create(jwtOptions));

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBusinessRepository, BusinessRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBusinessService, BusinessService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }

    private static void EnsureSecretConfigured(string settingName, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && !value.Contains("__SET_IN_USER_SECRETS_OR_ENV__", StringComparison.Ordinal))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Configuration '{settingName}' is not set. Configure local secrets with dotnet user-secrets or environment variables before running StockFlow.");
    }
}
