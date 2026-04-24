using StockFlow.Application.DTOs.Businesses;

namespace StockFlow.Application.DTOs.Auth;

public sealed record AuthResponse(string Token, DateTime ExpiresAtUtc, UserProfileResponse User, BusinessResponse Business);
