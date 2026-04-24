namespace StockFlow.Application.DTOs.Auth;

public sealed record UserProfileResponse(Guid Id, string FullName, string Email, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);
