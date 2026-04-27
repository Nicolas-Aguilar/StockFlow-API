using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DotNet.Testcontainers.Builders;
using Respawn;
using Respawn.Graph;
using StockFlow.Infrastructure.Data;
using Testcontainers.MsSql;

namespace StockFlow.IntegrationTests;

public sealed class StockFlowApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string JwtIssuer = "StockFlow.IntegrationTests";
    private const string JwtAudience = "StockFlow.IntegrationTests";

    private readonly string _databaseName = $"StockFlowIntegrationTests_{Guid.NewGuid():N}";
    private readonly MsSqlContainer _sqlServerContainer;
    private readonly string _jwtKey = $"IntegrationTestsSigningKey-{Guid.NewGuid():N}{Guid.NewGuid():N}";

    private Respawner _respawner = null!;
    private string _connectionString = string.Empty;

    public StockFlowApiFactory()
    {
        var containerPassword = $"Tests_{Guid.NewGuid():N}!Aa1";

        _sqlServerContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword(containerPassword)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["Jwt:Key"] = _jwtKey,
                ["Jwt:ExpirationMinutes"] = "120"
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _sqlServerContainer.StartAsync();

        var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(_sqlServerContainer.GetConnectionString())
        {
            InitialCatalog = _databaseName,
            TrustServerCertificate = true
        };

        _connectionString = sqlConnectionStringBuilder.ConnectionString;

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _connectionString);
        Environment.SetEnvironmentVariable("Jwt__Issuer", JwtIssuer);
        Environment.SetEnvironmentVariable("Jwt__Audience", JwtAudience);
        Environment.SetEnvironmentVariable("Jwt__Key", _jwtKey);
        Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", "120");

        await WaitForSqlServerReadyAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        await InitializeRespawnerAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        SqlConnection.ClearAllPools();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        Environment.SetEnvironmentVariable("Jwt__Issuer", null);
        Environment.SetEnvironmentVariable("Jwt__Audience", null);
        Environment.SetEnvironmentVariable("Jwt__Key", null);
        Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", null);

        SqlConnection.ClearAllPools();
        await _sqlServerContainer.DisposeAsync();
        Dispose();
    }

    private async Task InitializeRespawnerAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = new[]
            {
                new Table("__EFMigrationsHistory")
            }
        });
    }

    private async Task WaitForSqlServerReadyAsync()
    {
        var adminConnectionString = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master"
        }.ConnectionString;

        var attempts = 0;

        while (true)
        {
            try
            {
                await using var connection = new SqlConnection(adminConnectionString);
                await connection.OpenAsync();
                return;
            }
            catch (SqlException) when (attempts < 30)
            {
                attempts++;
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}
