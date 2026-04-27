namespace StockFlow.Domain.Exceptions;

public sealed class InvalidUserContextException : DomainException
{
    public InvalidUserContextException(string message) : base(message)
    {
    }
}
