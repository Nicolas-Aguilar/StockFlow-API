using StockFlow.Domain.Entities;

namespace StockFlow.Application.Interfaces;

public interface IPasswordService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string password, string passwordHash);
}
