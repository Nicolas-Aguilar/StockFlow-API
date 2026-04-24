using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Businesses;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Exceptions;

namespace StockFlow.Application.Services;

public sealed class BusinessService : IBusinessService
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IUserContext _userContext;

    public BusinessService(IBusinessRepository businessRepository, IUserContext userContext)
    {
        _businessRepository = businessRepository;
        _userContext = userContext;
    }

    public async Task<BusinessResponse> GetMyBusinessAsync(CancellationToken cancellationToken = default)
    {
        var business = await GetCurrentBusinessAsync(cancellationToken);
        return business.ToResponse();
    }

    public async Task<BusinessResponse> UpdateMyBusinessAsync(UpdateBusinessRequest request, CancellationToken cancellationToken = default)
    {
        var business = await GetCurrentBusinessAsync(cancellationToken);
        business.Name = request.Name.Trim();
        business.Description = request.Description?.Trim();
        business.UpdatedAt = DateTime.UtcNow;
        await _businessRepository.SaveChangesAsync(cancellationToken);
        return business.ToResponse();
    }

    private async Task<StockFlow.Domain.Entities.Business> GetCurrentBusinessAsync(CancellationToken cancellationToken)
    {
        if (!_userContext.IsAuthenticated)
        {
            throw new BusinessAccessDeniedException("Authentication is required.");
        }

        var business = await _businessRepository.GetByIdAsync(_userContext.BusinessId, cancellationToken)
            ?? throw new EntityNotFoundException("Business was not found.");

        if (business.OwnerUserId != _userContext.UserId)
        {
            throw new BusinessAccessDeniedException("You cannot access another business.");
        }

        return business;
    }
}
