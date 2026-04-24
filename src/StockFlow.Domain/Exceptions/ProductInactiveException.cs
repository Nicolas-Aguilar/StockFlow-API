namespace StockFlow.Domain.Exceptions;

public sealed class ProductInactiveException : DomainException
{
    public ProductInactiveException(string message) : base(message)
    {
    }
}
