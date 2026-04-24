namespace StockFlow.Domain.Exceptions;

public sealed class ProductExpiredException : DomainException
{
    public ProductExpiredException(string message) : base(message)
    {
    }
}
