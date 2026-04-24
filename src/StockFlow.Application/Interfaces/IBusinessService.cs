using StockFlow.Application.DTOs.Businesses;

namespace StockFlow.Application.Interfaces;

public interface IBusinessService
{
    Task<BusinessResponse> GetMyBusinessAsync(CancellationToken cancellationToken = default);
    Task<BusinessResponse> UpdateMyBusinessAsync(UpdateBusinessRequest request, CancellationToken cancellationToken = default);
}
