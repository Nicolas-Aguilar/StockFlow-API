using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockFlow.Infrastructure.Data;

namespace StockFlow.Infrastructure.Bootstrap;

public static class ServiceProviderExtensions
{
    public static async Task ApplyBootstrapAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StockFlow.Infrastructure.Bootstrap");
        var options = scope.ServiceProvider.GetRequiredService<IOptions<BootstrapOptions>>().Value;

        if (!options.ApplyMigrations && !options.SeedDemoData)
        {
            return;
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (options.ApplyMigrations)
        {
            logger.LogInformation("Applying database migrations during startup bootstrap.");
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        if (options.SeedDemoData)
        {
            logger.LogInformation("Seeding optional demo data during startup bootstrap.");
            var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
            await seeder.SeedAsync(cancellationToken);
        }
    }
}
