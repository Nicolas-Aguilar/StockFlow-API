namespace StockFlow.Application.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
