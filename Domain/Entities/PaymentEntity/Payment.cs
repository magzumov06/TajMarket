using Domain.Entities.OrderEntity;
using Domain.Enums;

namespace Domain.Entities.PaymentEntity;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
}