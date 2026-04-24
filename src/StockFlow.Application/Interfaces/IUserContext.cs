namespace StockFlow.Application.Interfaces;

public interface IUserContext
{
    Guid UserId { get; }
    Guid BusinessId { get; }
    bool IsAuthenticated { get; }
}
