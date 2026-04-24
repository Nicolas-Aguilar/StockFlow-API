namespace StockFlow.Domain.Exceptions;

public sealed class DuplicateInternalCodeException : DomainException
{
    public DuplicateInternalCodeException(string message) : base(message)
    {
    }
}
