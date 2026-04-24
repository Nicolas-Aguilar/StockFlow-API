namespace StockFlow.Application.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(Guid userId, Guid businessId, string email);
}
