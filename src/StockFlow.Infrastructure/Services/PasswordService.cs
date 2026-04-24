using Microsoft.AspNetCore.Identity;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Services;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(User user, string password) => _passwordHasher.HashPassword(user, password);

    public bool VerifyPassword(User user, string password, string passwordHash) => _passwordHasher.VerifyHashedPassword(user, passwordHash, password) != PasswordVerificationResult.Failed;
}
