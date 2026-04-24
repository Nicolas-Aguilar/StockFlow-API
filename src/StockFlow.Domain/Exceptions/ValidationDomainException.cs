namespace StockFlow.Domain.Exceptions;

public sealed class ValidationDomainException : DomainException
{
    public ValidationDomainException(string message) : base(message)
    {
    }
}
