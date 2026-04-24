namespace StockFlow.Domain.Exceptions;

public sealed class BusinessAccessDeniedException : DomainException
{
    public BusinessAccessDeniedException(string message) : base(message)
    {
    }
}
