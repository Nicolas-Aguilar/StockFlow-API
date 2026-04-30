namespace StockFlow.Infrastructure.Bootstrap;

public sealed class BootstrapOptions
{
    public const string SectionName = "Bootstrap";

    public bool ApplyMigrations { get; init; }

    public bool SeedDemoData { get; init; }
}
