namespace StockFlow.Domain.Exceptions;

public sealed class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string message) : base(message)
    {
    }
}
