using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace StockFlow.Infrastructure.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var apiProjectPath = ResolveApiProjectPath();

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(apiProjectPath, "appsettings.json"), optional: false)
            .AddJsonFile(Path.Combine(apiProjectPath, $"appsettings.{environment}.json"), optional: true)
            .AddUserSecrets(Assembly.Load("StockFlow.Api"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("__SET_IN_USER_SECRETS_OR_ENV__", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is not set for EF Core design-time operations. Configure it with dotnet user-secrets on src/StockFlow.Api or with environment variables before running dotnet ef.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }

    private static string ResolveApiProjectPath()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory is not null)
        {
            var candidate = Path.Combine(currentDirectory.FullName, "src", "StockFlow.Api");

            if (File.Exists(Path.Combine(candidate, "appsettings.json")))
            {
                return candidate;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException("Could not locate src/StockFlow.Api for EF Core design-time configuration.");
    }
}
