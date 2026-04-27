using StockFlow.Domain.Common;
using StockFlow.Domain.Enums;

namespace StockFlow.Domain.Entities;

public sealed class Sale : Entity
{
    public Guid BusinessId { get; set; }
    public decimal Total { get; set; }
    public decimal EstimatedProfit { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }

    public Business? Business { get; set; }
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}
